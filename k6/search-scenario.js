// Run (3 раза, по одному прогону на каждое количество VUs):
//   k6 run -e VUS=10   k6/search-scenario.js
//   k6 run -e VUS=100  k6/search-scenario.js
//   k6 run -e VUS=1000 k6/search-scenario.js
//
// Custom base URL:
//   k6 run -e BASE_URL=http://myserver:5000 -e VUS=100 k6/search-scenario.js

import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5084';
const VUS = parseInt(__ENV.VUS || '10', 10);
const REPORT_FILE = `.\\reports\\report-search-${VUS}vus.html`;

const FIRST_NAME = 'Иван';
const LAST_NAME = 'Петр';
const PAGE_SIZE = 50;
const PAGES = 10;
const SCROLL_DELAY = 0.5;
// TODO сделай пользаков с ходом в 100 (1, 10, 100, 200, ..., 1000)
// TODO добавь рандомизаию поисковых запросов (до 1000 уникальных сочетаний)

export const options = {
  vus: VUS,
  duration: '60s',
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

export function handleSummary(data) {
  return { [REPORT_FILE]: htmlReport(data) };
}
