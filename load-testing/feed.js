import http from 'k6/http';
import { check, sleep } from 'k6';
import crypto from 'k6/crypto';
import { b64encode } from 'k6/encoding';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5084';
const JWT_KEY = 'people-hub-dev-signing-key-0123456789-abcdefghijklmnopqrstuvwxyz';

const feedUsers = [
  { id: 2, email: 'carol-feed@test.com' },
  { id: 14, email: 'reader2-feed@test.com' },
  { id: 15, email: 'reader3-feed@test.com' },
  { id: 16, email: 'reader4-feed@test.com' },
  { id: 17, email: 'reader5-feed@test.com' },
  { id: 18, email: 'reader6-feed@test.com' },
  { id: 19, email: 'reader7-feed@test.com' },
  { id: 20, email: 'reader8-feed@test.com' },
  { id: 21, email: 'reader9-feed@test.com' },
  { id: 22, email: 'reader10-feed@test.com' },
];

export const options = {
  scenarios: {
    feed: {
      executor: 'constant-vus',
      vus: Number(__ENV.VUS || 100),
      duration: __ENV.DURATION || '120s',
    },
  },
  summaryTrendStats: ['min', 'avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    'http_req_duration{name:post/feed}': ['p(95)<1000'],
  },
};

function makeToken(user) {
  const header = b64encode(JSON.stringify({ alg: 'HS256', typ: 'JWT' }), 'rawurl');
  const payload = b64encode(
    JSON.stringify({
      unique_name: user.email,
      nameid: String(user.id),
      exp: Math.floor(Date.now() / 1000) + 4 * 3600,
      iss: 'PeopleHub',
      aud: 'PeopleHub',
    }),
    'rawurl'
  );
  const signature = crypto.hmac('sha256', JWT_KEY, `${header}.${payload}`, 'base64rawurl');
  return `${header}.${payload}.${signature}`;
}

const tokens = feedUsers.map(makeToken);

export default function () {
  const token = tokens[(__VU - 1) % tokens.length];

  const res = http.get(`${BASE_URL}/api/post/feed`, {
    headers: { Authorization: `Bearer ${token}` },
    tags: { name: 'post/feed' },
    timeout: '10s',
  });

  let posts = 0;
  try {
    posts = res.json().length;
  } catch (e) {
    posts = 0;
  }

  check(res, {
    'feed 200': (r) => r.status === 200,
    'feed has 30-40 posts': () => posts >= 30 && posts <= 40,
  });

  sleep(0.1);
}
