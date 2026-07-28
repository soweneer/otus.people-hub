import http from 'k6/http';
import { check, sleep } from 'k6';
import crypto from 'k6/crypto';
import { b64encode } from 'k6/encoding';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
const JWT_KEY = __ENV.JWT_KEY || 'people-hub-dev-signing-key-0123456789-abcdefghijklmnopqrstuvwxyz';
const REPORT = __ENV.REPORT || 'dialogs';

const READ_PAIRS = Number(__ENV.READ_PAIRS || 40);
const MESSAGES_PER_PAIR = Number(__ENV.MESSAGES_PER_PAIR || 50);
const WRITE_PAIRS = Number(__ENV.WRITE_PAIRS || 200);

const RUN = Number(__ENV.RUN || 1);

const READ_LEFT = 2000000;
const READ_RIGHT = 3000000;
const WRITE_LEFT = 4000000 + RUN * 100000;
const WRITE_RIGHT = 5000000 + RUN * 100000;

export const options = {
  scenarios: {
    reads: {
      executor: 'constant-vus',
      exec: 'readDialog',
      vus: Number(__ENV.READ_VUS || 30),
      duration: __ENV.DURATION || '120s',
    },
    writes: {
      executor: 'constant-vus',
      exec: 'writeMessage',
      vus: Number(__ENV.WRITE_VUS || 10),
      duration: __ENV.DURATION || '120s',
    },
  },
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    'http_req_failed': ['rate<0.01'],
    'http_req_duration{name:dialog/list}': ['p(95)<10000'],
    'http_req_duration{name:dialog/send}': ['p(95)<10000'],
    'http_reqs{name:dialog/list}': ['count>0'],
    'http_reqs{name:dialog/send}': ['count>0'],
    'http_req_failed{name:dialog/list}': ['rate<0.01'],
    'http_req_failed{name:dialog/send}': ['rate<0.01'],
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
      unique_name: `dialog-load-${userId}@test.com`,
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

  for (let i = 0; i < READ_PAIRS; i++) {
    const left = READ_LEFT + i;
    const right = READ_RIGHT + i;

    const existing = http.get(`${BASE_URL}/dialog/${right}/list`, {
      headers: { Authorization: `Bearer ${tokenFor(left)}` },
      tags: { name: 'seed/list' },
    });

    let already = 0;
    try {
      already = existing.json().length;
    } catch (e) {
      already = 0;
    }

    const batch = [];
    for (let m = already; m < MESSAGES_PER_PAIR; m++) {
      const from = m % 2 === 0 ? left : right;
      const to = m % 2 === 0 ? right : left;
      batch.push(sendRequest(from, to, `seed message ${m} for pair ${i}`, 'seed/send'));
    }

    if (batch.length > 0) {
      seeded += http.batch(batch).filter((r) => r.status === 200).length;
    }
  }

  console.log(`seeded ${seeded} messages, target ${MESSAGES_PER_PAIR} per pair, in ${(Date.now() - started) / 1000}s`);

  return { seeded: seeded };
}

export function readDialog() {
  const i = (__VU + __ITER) % READ_PAIRS;
  const left = READ_LEFT + i;
  const right = READ_RIGHT + i;

  const res = http.get(`${BASE_URL}/dialog/${right}/list`, {
    headers: { Authorization: `Bearer ${tokenFor(left)}` },
    tags: { name: 'dialog/list' },
    timeout: '10s',
  });

  let messages = 0;
  try {
    messages = res.json().length;
  } catch (e) {
    messages = 0;
  }

  check(res, {
    'list 200': (r) => r.status === 200,
    'list is not empty': () => messages >= MESSAGES_PER_PAIR,
  });

  sleep(0.05);
}

export function writeMessage() {
  const i = (__VU * 7919 + __ITER) % WRITE_PAIRS;
  const from = WRITE_LEFT + i;
  const to = WRITE_RIGHT + i;

  const [method, url, body, params] = sendRequest(from, to, `load message from vu ${__VU} iter ${__ITER}`);
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
    'http_req_duration{name:dialog/list}',
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
