import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

const testFirstName = 'Иван';
const testLastName = 'Петр';
const pageSize = 50;
const idsPerIteration = 5;
const thinkDelay = 0.2;

export const options = {
  stages: [{ duration: '200s', target: 2000 }],
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    'http_req_duration{name:user/search}': ['p(95)<2000'],
    'http_req_duration{name:user/{id}}': ['p(95)<1000']
  }
};

export default function () {
  const searchUrl = `${BASE_URL}/user/search` +
    `?first_name=${encodeURIComponent(testFirstName)}` +
    `&last_name=${encodeURIComponent(testLastName)}` +
    `&skip=0&take=${pageSize}`;

  const searchRes = http.get(searchUrl, { tags: { name: 'user/search' } });
  check(searchRes, { 'search ok': (r) => r.status === 200 });

  let ids = [];
  try {
    ids = searchRes.json().map((u) => u.id);
  } catch (e) {
    // не JSON (например, страница ошибки) — пропускаем итерацию по id
  }

  for (let i = 0; i < idsPerIteration && ids.length > 0; i++) {
    const id = ids[Math.floor(Math.random() * ids.length)];
    const res = http.get(`${BASE_URL}/user/${id}`, { tags: { name: 'user/{id}' } });
    check(res, { 'get by id ok': (r) => r.status === 200 });
    sleep(thinkDelay);
  }
}
