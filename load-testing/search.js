import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

const firstNames = [
  'Александр', 'Дмитрий', 'Максим', 'Сергей', 'Андрей', 'Алексей', 'Артём',
  'Илья', 'Кирилл', 'Михаил', 'Никита', 'Даниил', 'Егор', 'Матвей', 'Роман',
  'Владимир', 'Олег'
];
const lastNames = [
  'Иванов', 'Смирнов', 'Кузнецов', 'Попов', 'Васильев', 'Петров', 'Соколов',
  'Михайлов', 'Новиков', 'Фёдоров', 'Морозов', 'Волков', 'Алексеев', 'Лебедев',
  'Семёнов', 'Егоров', 'Павлов', 'Козлов', 'Степанов', 'Николаев', 'Орлов',
  'Андреев', 'Макаров', 'Никитин', 'Захаров'
];

const pageSize = 50;
const pagesCount = 10;
const pagesPerName = 5;
const scrollDelay = 0.2;
const idsPerIteration = 5;
const thinkDelay = 0.2;

export const options = {
  stages: [{ duration: '200s', target: Number(__ENV.TARGET_VUS || 2000) }],
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    'http_req_duration{name:user/search}': ['p(95)<2000'],
    'http_req_duration{name:user/{id}}': ['p(95)<1000']
  }
};

function randomItem(arr) {
  return arr[Math.floor(Math.random() * arr.length)];
}

export default function () {
  let ids = [];

  let firstName, lastName;
  let skip = 0;
  for (let page = 0; page < pagesCount; page++) {
    if (page % pagesPerName === 0) {
      firstName = randomItem(firstNames);
      lastName = randomItem(lastNames);
      skip = 0;
    }

    const url = `${BASE_URL}/api/user/search` +
      `?first_name=${encodeURIComponent(firstName)}` +
      `&last_name=${encodeURIComponent(lastName)}` +
      `&skip=${skip}` +
      `&take=${pageSize}`;

    const res = http.get(url, { tags: { name: 'user/search' } });
    check(res, { 'search ok': (r) => r.status === 200 });

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
    const res = http.get(`${BASE_URL}/api/user/${id}`, { tags: { name: 'user/{id}' } });
    check(res, { 'get by id ok': (r) => r.status === 200 });
    sleep(thinkDelay);
  }
}
