// Run:   k6 run k6/scenario.js
// Custom base URL:        k6 run -e BASE_URL=http://myserver:5000 k6/scenario.js

import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export const options = {
  vus: 100,
  duration: '60s',
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    http_req_duration: ['p(95)<3000', 'p(99)<5000'],
    http_req_failed: ['rate<0.05'],
  },
};

// ── helpers ──────────────────────────────────────────────────────────────────

const _names     = ['Алексей','Дмитрий','Иван','Сергей','Андрей','Михаил','Николай','Артём','Владимир','Павел',
                   'Анна','Мария','Екатерина','Ольга','Татьяна','Наталья','Елена','Юлия','Ирина','Дарья'];
const _surnames  = ['Иванов','Смирнов','Кузнецов','Попов','Васильев','Петров','Соколов','Михайлов','Новиков','Фёдоров',
                   'Иванова','Смирнова','Кузнецова','Попова','Васильева','Петрова','Соколова','Михайлова','Новикова','Фёдорова'];
const _cities    = ['Москва','Санкт-Петербург','Казань','Новосибирск','Екатеринбург','Нижний-Новгород','Самара','Омск','Челябинск','Уфа'];
const _bios = ['Люблю путешествовать','Увлекаюсь спортом','Читаю книги','Занимаюсь музыкой','Программирую в свободное время',
                   'Обожаю кино','Интересуюсь историей','Люблю готовить','Занимаюсь йогой','Играю в шахматы'];

function pick(arr) { return arr[Math.floor(Math.random() * arr.length)]; }

function generateUser(vu, iter) {
  const ts = Date.now();
  return {
    Name:            pick(_names),
    Surname:         pick(_surnames),
    City:            pick(_cities),
    Email:           `user_${vu}_${iter}_${ts}@peoplehub.test`,
    Password:        'asdzxc',
    ConfirmPassword: 'asdzxc',
    Bio:             `${pick(_bios)}`,
    Age:             String(18 + Math.floor(Math.random() * 43)),
    Gender:          String(Math.round(Math.random())),
  };
}

function extractToken(body) {
  let m = body.match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/);
  if (m) return m[1];
  m = body.match(/value="([^"]+)"[^>]*name="__RequestVerificationToken"/);
  return m ? m[1] : '';
}

// Returns all unique targetPersonId values found in hrefs that contain `keyword`
function extractTargetIds(body, keyword) {
  const re = new RegExp(`${keyword}[^"]*targetPersonId=(\\d+)`, 'g');
  const ids = [];
  const seen = new Set();
  let m;
  while ((m = re.exec(body)) !== null) {
    if (!seen.has(m[1])) {
      seen.add(m[1]);
      ids.push(m[1]);
    }
  }
  return ids;
}

function shuffle(arr) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    const tmp = arr[i];
    arr[i] = arr[j];
    arr[j] = tmp;
  }
  return arr;
}

// ── scenario ─────────────────────────────────────────────────────────────────

export default function () {
  const user = generateUser(__VU, __ITER);

  // ── 1. Регистрация ──────────────────────────────────────────────────────

  let res = http.get(`${BASE_URL}/Account/SignUp`);
  check(res, { 'signup page loaded': (r) => r.status === 200 });

  const signupToken = extractToken(res.body);

  res = http.post(
    `${BASE_URL}/Account/SignUp`,
    Object.assign({ __RequestVerificationToken: signupToken }, user),
  );
  check(res, {
    'signup success': (r) => r.url.includes('/Person') && !r.url.includes('/Person/Index') ||
                             r.url.includes('/Account/SignUp')
  });

  sleep(0.3);

  // ── 2. Выход, чтобы протестировать вход отдельно ──────────────────────

  http.get(`${BASE_URL}/Account/SignOut`);

  // ── 3. Логин ────────────────────────────────────────────────────────────

  res = http.get(`${BASE_URL}/Account/SignIn`);
  check(res, { 'signin page loaded': (r) => r.status === 200 });

  const signinToken = extractToken(res.body);

  res = http.post(`${BASE_URL}/Account/SignIn`, {
    __RequestVerificationToken: signinToken,
    Email: user.Email,
    Password: user.Password,
  });
  check(res, {
    'signin success': (r) => r.url.includes('/Person') && !r.url.includes('/Person/Index') ||
                             r.url.includes('/Account/SignIn'),
  });

  sleep(0.3);

  // ── 4. Страница людей → отправить 10 заявок в друзья ───────────────────

  res = http.get(`${BASE_URL}/Person/Index`);
  check(res, { 'person index loaded': (r) => r.status === 200 });

  const personIds = shuffle(extractTargetIds(res.body, 'SendFriendRequest'));
  const targets = personIds.slice(0, Math.min(10, personIds.length));

  for (const id of targets) {
    res = http.get(
      `${BASE_URL}/Friends/SendFriendRequest?targetPersonId=${id}&returnUrl=/Person/Index`,
    );
    check(res, { 'friend request sent': (r) => r.status === 200 || r.status === 302 });
    sleep(0.1);
  }

  sleep(0.3);

  // ── 5. Страница друзей → отменить 2 случайные заявки ───────────────────

  res = http.get(`${BASE_URL}/Friends/Index`);
  check(res, { 'friends index loaded': (r) => r.status === 200 });

  const outgoingIds = shuffle(extractTargetIds(res.body, 'CancelRequest'));
  const toCancel = outgoingIds.slice(0, Math.min(2, outgoingIds.length));

  for (const id of toCancel) {
    res = http.get(`${BASE_URL}/Friends/CancelRequest?targetPersonId=${id}`);
    check(res, { 'request cancelled': (r) => r.status === 200 || r.status === 302 });
    sleep(0.1);
  }

  sleep(0.3);

  // ── 6. Разлогиниться ────────────────────────────────────────────────────

  res = http.get(`${BASE_URL}/Account/SignOut`);
  check(res, {
    'signout success': (r) => r.url.includes('/Account/SignIn'),
  });
}

export function handleSummary(data) {
  return { 'report.html': htmlReport(data) };
}
