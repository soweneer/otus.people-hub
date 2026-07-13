import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

const confirmed = new Counter('writes_confirmed');
const failed = new Counter('writes_failed');

export const options = {
  scenarios: {
    writes: {
      executor: 'constant-vus',
      vus: Number(__ENV.VUS || 20),
      duration: __ENV.DURATION || '120s',
    },
  },
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
};

export default function () {
  const payload = JSON.stringify({
    first_name: 'Write',
    second_name: 'Load',
    age: 30,
    biography: 'failover write test',
    city: '__failover_test__',
    gender: 0,
  });

  const res = http.post(`${BASE_URL}/user/register`, payload, {
    headers: { 'Content-Type': 'application/json' },
    tags: { name: 'user/register' },
    timeout: '10s',
  });

  let ok = false;
  if (res.status === 200) {
    try {
      ok = !!res.json().user_id;
    } catch (e) {
      ok = false;
    }
  }

  if (ok) {
    confirmed.add(1);
  } else {
    failed.add(1);
  }
  check(res, { 'register confirmed': () => ok });

  sleep(0.1);
}
