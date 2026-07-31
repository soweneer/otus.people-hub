import http from 'k6/http';
import { check, sleep } from 'k6';
import crypto from 'k6/crypto';
import { b64encode } from 'k6/encoding';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8090';
const JWT_KEY = __ENV.JWT_KEY || 'people-hub-dev-signing-key-0123456789-abcdefghijklmnopqrstuvwxyz';
const REPORT = __ENV.REPORT || 'counters';

const PAIRS = Number(__ENV.PAIRS || 40);
const UNREAD_PER_PAIR = Number(__ENV.UNREAD_PER_PAIR || 50);

const SENDER_LEFT = 6000000;
const RECIPIENT_LEFT = 6500000;

export const options = {
  scenarios: {
    counters: {
      executor: 'constant-vus',
      exec: 'readCounters',
      vus: Number(__ENV.READ_VUS || 40),
      duration: __ENV.DURATION || '60s',
    },
    baseline: {
      executor: 'constant-vus',
      exec: 'readBaseline',
      vus: Number(__ENV.BASELINE_VUS || 10),
      duration: __ENV.DURATION || '60s',
    },
    writes: {
      executor: 'constant-vus',
      exec: 'writeMessage',
      vus: Number(__ENV.WRITE_VUS || 5),
      duration: __ENV.DURATION || '60s',
    },
  },
  setupTimeout: __ENV.SETUP_TIMEOUT || '600s',
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    'http_req_failed{name:counters/unread}': ['rate<0.01'],
    'http_req_duration{name:counters/unread}': ['p(95)<1000'],
    'http_req_duration{name:baseline/list}': ['p(95)<10000'],
    'http_req_duration{name:dialog/send}': ['p(95)<10000'],
    'http_reqs{name:counters/unread}': ['count>0'],
    'http_reqs{name:baseline/list}': ['count>0'],
  },
};

const tokens = {};

function tokenFor(userId) {
  if (tokens[userId]) {
    return tokens[userId];
  }

  const header = b64encode(JSON.stringify({ alg: 'HS256', typ: 'JWT' }), 'rawurl');
  const payload = b64encode(
    JSON.stringify({
      unique_name: `counters-load-${userId}@test.com`,
      nameid: String(userId),
      exp: Math.floor(Date.now() / 1000) + 4 * 3600,
      iss: 'PeopleHub',
      aud: 'PeopleHub',
    }),
    'rawurl'
  );
  const signature = crypto.hmac('sha256', JWT_KEY, `${header}.${payload}`, 'base64rawurl');

  tokens[userId] = `${header}.${payload}.${signature}`;
  return tokens[userId];
}

function sendRequest(fromUserId, toUserId, text, tag) {
  return [
    'POST',
    `${BASE_URL}/dialog/${toUserId}/send`,
    JSON.stringify({ text: text }),
    {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${tokenFor(fromUserId)}`,
      },
      tags: { name: tag || 'dialog/send' },
      timeout: '10s',
    },
  ];
}

export function setup() {
  const started = Date.now();
  let seeded = 0;

  for (let i = 0; i < PAIRS; i++) {
    const sender = SENDER_LEFT + i;
    const recipient = RECIPIENT_LEFT + i;

    const existing = http.get(`${BASE_URL}/dialog/${sender}/list`, {
      headers: { Authorization: `Bearer ${tokenFor(recipient)}` },
      tags: { name: 'seed/list' },
    });

    let already = 0;
    try {
      already = existing.json().length;
    } catch (e) {
      already = 0;
    }

    const batch = [];
    for (let m = already; m < UNREAD_PER_PAIR; m++) {
      batch.push(sendRequest(sender, recipient, `unread seed ${m} for pair ${i}`, 'seed/send'));
    }

    if (batch.length > 0) {
      seeded += http.batch(batch).filter((r) => r.status === 200).length;
    }
  }

  console.log(`seeded ${seeded} unread messages across ${PAIRS} pairs in ${(Date.now() - started) / 1000}s`);

  return { seeded: seeded };
}

export function readCounters() {
  const recipient = RECIPIENT_LEFT + ((__VU + __ITER) % PAIRS);

  const res = http.get(`${BASE_URL}/api/counters/unread`, {
    headers: { Authorization: `Bearer ${tokenFor(recipient)}` },
    tags: { name: 'counters/unread' },
    timeout: '10s',
  });

  check(res, {
    'counters 200': (r) => r.status === 200,
    'counters not empty': (r) => {
      try {
        return r.json().total > 0;
      } catch (e) {
        return false;
      }
    },
  });

  sleep(0.05);
}

export function readBaseline() {
  const i = (__VU + __ITER) % PAIRS;
  const sender = SENDER_LEFT + i;
  const recipient = RECIPIENT_LEFT + i;

  const res = http.get(`${BASE_URL}/dialog/${sender}/list`, {
    headers: { Authorization: `Bearer ${tokenFor(recipient)}` },
    tags: { name: 'baseline/list' },
    timeout: '10s',
  });

  check(res, {
    'baseline 200': (r) => r.status === 200,
  });

  sleep(0.05);
}

export function writeMessage() {
  const i = (__VU * 7919 + __ITER) % PAIRS;
  const sender = SENDER_LEFT + i;
  const recipient = RECIPIENT_LEFT + i;

  const [method, url, body, params] = sendRequest(sender, recipient, `live message from vu ${__VU} iter ${__ITER}`);
  const res = http.request(method, url, body, params);

  check(res, {
    'send 200': (r) => r.status === 200,
  });

  sleep(0.05);
}

export function handleSummary(data) {
  const out = {};
  out[`my-reports/${REPORT}-summary.json`] = JSON.stringify(data, null, 2);
  out['stdout'] = textSummary(data);

  return out;
}

function textSummary(data) {
  const lines = [''];
  const metrics = [
    'http_req_duration{name:counters/unread}',
    'http_req_duration{name:baseline/list}',
    'http_req_duration{name:dialog/send}',
    'http_reqs',
    'http_req_failed',
  ];

  for (const name of metrics) {
    const metric = data.metrics[name];
    if (!metric) {
      continue;
    }

    if (metric.values.count !== undefined) {
      lines.push(`${name}: count=${metric.values.count} rps=${metric.values.rate.toFixed(2)}`);
      continue;
    }

    if (metric.values.avg === undefined) {
      lines.push(`${name}: rate=${metric.values.rate.toFixed(4)} passes=${metric.values.passes} fails=${metric.values.fails}`);
      continue;
    }

    lines.push(
      `${name}: avg=${metric.values.avg.toFixed(2)}ms med=${metric.values.med.toFixed(2)}ms ` +
        `p90=${metric.values['p(90)'].toFixed(2)}ms p95=${metric.values['p(95)'].toFixed(2)}ms ` +
        `p99=${metric.values['p(99)'].toFixed(2)}ms max=${metric.values.max.toFixed(2)}ms`
    );
  }

  lines.push('');
  return lines.join('\n');
}
