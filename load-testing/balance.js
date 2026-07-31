import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8090';

const firstNames = [
  'Александр', 'Дмитрий', 'Максим', 'Сергей', 'Андрей', 'Алексей', 'Артём',
  'Илья', 'Кирилл', 'Михаил', 'Никита', 'Даниил', 'Егор', 'Матвей', 'Роман',
  'Владимир', 'Олег'
];
const lastNames = [
  'Иванов', 'Смирнов', 'Кузнецов', 'Попов', 'Васильев', 'Петров', 'Соколов',
  'Михайлов', 'Новиков', 'Фёдоров', 'Морозов', 'Волков', 'Алексеев', 'Лебедев',
  'Семёнов', 'Егоров', 'Павлов', 'Козлов', 'Степанов', 'Николаев', 'Орлов'
];

const pageSize = 50;
const pagesPerIteration = 4;
const idsPerIteration = 4;
const scrollDelay = 0.3;
const thinkDelay = 0.3;

export const requestsOk = new Counter('requests_ok');
export const requestsFailed = new Counter('requests_failed');

export const options = {
  stages: [
    { duration: '30s', target: Number(__ENV.TARGET_VUS || 300) },
    { duration: __ENV.HOLD || '300s', target: Number(__ENV.TARGET_VUS || 300) }
  ],
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    'http_req_failed': ['rate<0.05']
  }
};

function randomItem(arr) {
  return arr[Math.floor(Math.random() * arr.length)];
}

function track(res) {
  if (res.status === 200) {
    requestsOk.add(1);
  } else {
    requestsFailed.add(1);
  }
}

export default function () {
  let ids = [];
  const firstName = randomItem(firstNames);
  const lastName = randomItem(lastNames);
  let skip = 0;

  for (let page = 0; page < pagesPerIteration; page++) {
    const url = `${BASE_URL}/user/search` +
      `?first_name=${encodeURIComponent(firstName)}` +
      `&last_name=${encodeURIComponent(lastName)}` +
      `&skip=${skip}` +
      `&take=${pageSize}`;

    const res = http.get(url, { tags: { name: 'user/search' } });
    check(res, { 'search ok': (r) => r.status === 200 });
    track(res);

    try {
      ids = ids.concat(res.json().map((u) => u.id));
    } catch (e) {
      //
    }

    sleep(scrollDelay);
    skip += pageSize;
  }

  for (let i = 0; i < idsPerIteration && ids.length > 0; i++) {
    const id = ids[Math.floor(Math.random() * ids.length)];
    const res = http.get(`${BASE_URL}/user/${id}`, { tags: { name: 'user/{id}' } });
    check(res, { 'get by id ok': (r) => r.status === 200 });
    track(res);
    sleep(thinkDelay);
  }
}

export function handleSummary(data) {
  const out = { stdout: '' };
  if (__ENV.SUMMARY_EXPORT) {
    out[__ENV.SUMMARY_EXPORT] = JSON.stringify(data, null, 2);
  }
  return out;
}
