import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';

const ADDRESS = __ENV.GRPC_ADDRESS || 'localhost:8081';
const REPORT = __ENV.REPORT || 'dialogs-grpc';

const READ_PAIRS = Number(__ENV.READ_PAIRS || 40);
const MESSAGES_PER_PAIR = Number(__ENV.MESSAGES_PER_PAIR || 50);
const WRITE_PAIRS = Number(__ENV.WRITE_PAIRS || 200);

const RUN = Number(__ENV.RUN || 1);

const READ_LEFT = 2000000;
const READ_RIGHT = 3000000;
const WRITE_LEFT = 4000000 + RUN * 100000;
const WRITE_RIGHT = 5000000 + RUN * 100000;

const client = new grpc.Client();
client.load(['../src/backend/PeopleHub.Chats/Protos'], 'dialogs.proto');

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
    'grpc_req_duration{name:Dialogs/List}': ['p(95)<10000'],
    'grpc_req_duration{name:Dialogs/Send}': ['p(95)<10000'],
    'checks{name:Dialogs/List}': ['rate>0.99'],
    'checks{name:Dialogs/Send}': ['rate>0.99'],
  },
};

let connected = false;

function ensureConnected() {
  if (!connected) {
    client.connect(ADDRESS, { plaintext: true });
    connected = true;
  }
}

export function readDialog() {
  ensureConnected();

  const i = (__VU + __ITER) % READ_PAIRS;

  const response = client.invoke(
    'dialogs.Dialogs/List',
    { user_id_1: READ_LEFT + i, user_id_2: READ_RIGHT + i },
    { tags: { name: 'Dialogs/List' } }
  );

  const messages = response.message && response.message.messages ? response.message.messages.length : 0;

  check(
    response,
    {
      'list ok': (r) => r.status === grpc.StatusOK,
      'list is not empty': () => messages >= MESSAGES_PER_PAIR,
    },
    { name: 'Dialogs/List' }
  );

  sleep(0.05);
}

export function writeMessage() {
  ensureConnected();

  const i = (__VU * 7919 + __ITER) % WRITE_PAIRS;

  const response = client.invoke(
    'dialogs.Dialogs/Send',
    {
      from_user_id: WRITE_LEFT + i,
      to_user_id: WRITE_RIGHT + i,
      text: `grpc load message from vu ${__VU} iter ${__ITER}`,
    },
    { tags: { name: 'Dialogs/Send' } }
  );

  check(
    response,
    {
      'send ok': (r) => r.status === grpc.StatusOK,
    },
    { name: 'Dialogs/Send' }
  );

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
    'grpc_req_duration{name:Dialogs/List}',
    'grpc_req_duration{name:Dialogs/Send}',
    'grpc_reqs',
    'checks',
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
