import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';

const BASE_URL = __ENV.BASE_URL || 'localhost';
console.log(BASE_URL);


const FIRST_NAME = 'Иван';
const LAST_NAME = 'Петр';
const PAGE_SIZE = 50;
const PAGES = 10;
const SCROLL_DELAY = 0.3;

export const options = {
  stages: [
    { duration: '240s', target: 2000 }
  ],
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    http_req_duration: ['p(95)<3000', 'p(99)<5000'],
    http_req_failed: ['rate<0.05'],
  },
};

export default function () {
  let skip = 0;
  for (let page = 0; page < PAGES; page++) {
    const url = `${BASE_URL}/Search/Index` +
      `?FirstName=${encodeURIComponent(FIRST_NAME)}` +
      `&LastName=${encodeURIComponent(LAST_NAME)}` +
      `&Skip=${skip}` +
      `&Take=${PAGE_SIZE}`;

    const res = http.get(url, { tags: { name: 'Search/Index' } });
    check(res, { 'search ok': (r) => r.status === 200 });

    sleep(SCROLL_DELAY);
    skip += PAGE_SIZE;
  }
}

