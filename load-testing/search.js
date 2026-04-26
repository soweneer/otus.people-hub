import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

const testFirstName = 'Иван';
const testLastName = 'Петр';
const pageSize = 50;
const pagesCount = 10;
const scrollDelay = 0.2;

export const options = {
  stages: [{ duration: '200s', target: 2000 }],
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max']
};

export default function () {
  let skip = 0;
  for (let page = 0; page < pagesCount; page++) {
    const url = `${BASE_URL}/search` +
      `?firstName=${encodeURIComponent(testFirstName)}` +
      `&lastName=${encodeURIComponent(testLastName)}` +
      `&skip=${skip}` +
      `&take=${pageSize}`;

    const res = http.get(url);
    check(res, { 'search ok': (r) => r.status === 200 });
    sleep(scrollDelay);
    skip += pageSize;
  }
}
