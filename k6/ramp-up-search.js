// Run:
//   k6 run k6/ramp-up-search.js
//
// Custom base URL:
//   k6 run -e BASE_URL=http://myserver:5000 k6/ramp-up-search.js
//
// Профиль нагрузки: первые 10 секунд держим 10 VUs, затем каждые 10 секунд
// добавляем по одному VU, пока не дойдём до 100. Полное время прогона = 10 + 90*10 = 910 секунд.

import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5084';
const REPORT_FILE = `.\\reports\\report-search-rampup-10-to-100.html`;

const FIRST_NAME = 'Иван';
const LAST_NAME = 'Петр';
const PAGE_SIZE = 50;
const PAGES = 10;
const SCROLL_DELAY = 0.5;

const START_VUS = 10;
const MAX_VUS = 100;
const STEP_DURATION = '10s';

function buildStages() {
  const stages = [{ duration: STEP_DURATION, target: START_VUS }];
  for (let n = START_VUS + 1; n <= MAX_VUS; n++) {
    stages.push({ duration: '0s', target: n });
    stages.push({ duration: STEP_DURATION, target: n });
  }
  return stages;
}

export const options = {
  scenarios: {
    ramp_up: {
      executor: 'ramping-vus',
      startVUs: START_VUS,
      stages: buildStages(),
      gracefulRampDown: '0s',
    },
  },
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
