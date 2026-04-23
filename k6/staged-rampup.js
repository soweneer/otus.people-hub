// Run:
//   k6 run k6/staged-rampup.js
//
// Custom base URL:
//   k6 run -e BASE_URL=http://myserver:5000 k6/staged-rampup.js

import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5084';
const REPORT_FILE = './reports/staged-rampup.html';

const FIRST_NAME = 'Иван';
const LAST_NAME = 'Петр';
const PAGE_SIZE = 50;
const PAGES = 10;
const SCROLL_DELAY = 0.3;

export const options = {
  stages: [
    // Phase 1: 0 → 100 VUs, +10 per 5s (50s total)
    { duration: '5s', target: 10 },
    { duration: '5s', target: 20 },
    { duration: '5s', target: 30 },
    { duration: '5s', target: 40 },
    { duration: '5s', target: 50 },
    { duration: '5s', target: 60 },
    { duration: '5s', target: 70 },
    { duration: '5s', target: 80 },
    { duration: '5s', target: 90 },
    { duration: '5s', target: 100 },
    // Phase 2: 100 → 1000 VUs, +25 per 10s
    { duration: '10s', target: 125 },
    { duration: '10s', target: 150 },
    { duration: '10s', target: 175 },
    { duration: '10s', target: 200 },
    { duration: '10s', target: 225 },
    { duration: '10s', target: 250 },
    { duration: '10s', target: 275 },
    { duration: '10s', target: 300 },
    { duration: '10s', target: 325 },
    { duration: '10s', target: 350 },
    { duration: '10s', target: 375 },
    { duration: '10s', target: 400 },
    { duration: '10s', target: 425 },
    { duration: '10s', target: 450 },
    { duration: '10s', target: 475 },
    { duration: '10s', target: 500 },
    { duration: '10s', target: 525 },
    { duration: '10s', target: 550 },
    { duration: '10s', target: 575 },
    { duration: '10s', target: 600 },
    { duration: '10s', target: 625 },
    { duration: '10s', target: 650 },
    { duration: '10s', target: 675 },
    { duration: '10s', target: 700 },
    { duration: '10s', target: 725 },
    { duration: '10s', target: 750 },
    { duration: '10s', target: 775 },
    { duration: '10s', target: 800 },
    { duration: '10s', target: 825 },
    { duration: '10s', target: 850 },
    { duration: '10s', target: 875 },
    { duration: '10s', target: 900 },
    { duration: '10s', target: 925 },
    { duration: '10s', target: 950 },
    { duration: '10s', target: 975 },
    { duration: '10s', target: 1000 },
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

export function handleSummary(data) {
  return { [REPORT_FILE]: htmlReport(data) };
}
