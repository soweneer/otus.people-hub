const fs = require('fs');
const path = require('path');
const http = require('http');

const PROM = process.env.PROM_URL || 'http://localhost:9090';
const OUT = process.env.OUT_DIR || 'docs/images';
const START = Number(process.env.START);
const END = Number(process.env.END);
const MARKS = (process.env.MARKS || '').split(',').filter(Boolean).map((m) => {
  const [at, label] = m.split(':');
  return { at: Number(at), label };
});

const W = 1000;
const H = 300;
const PAD = { top: 22, right: 190, bottom: 34, left: 62 };
const COLORS = ['#378ADD', '#1D9E75', '#D85A30', '#7F77DD', '#BA7517', '#D4537E'];

function get(url) {
  return new Promise((resolve, reject) => {
    http.get(url, (res) => {
      let body = '';
      res.on('data', (c) => (body += c));
      res.on('end', () => resolve(JSON.parse(body)));
    }).on('error', reject);
  });
}

async function query(expr, step) {
  const url = `${PROM}/api/v1/query_range?query=${encodeURIComponent(expr)}&start=${START}&end=${END}&step=${step}`;
  const res = await get(url);
  if (res.status !== 'success') throw new Error(`prometheus: ${JSON.stringify(res)}`);
  return res.data.result;
}

function esc(s) {
  return String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function niceMax(v) {
  if (v <= 0) return 1;
  const pow = Math.pow(10, Math.floor(Math.log10(v)));
  const norm = v / pow;
  const step = norm <= 1 ? 1 : norm <= 2 ? 2 : norm <= 5 ? 5 : 10;
  return step * pow;
}

function fmt(v, max) {
  if (max >= 1000000) return (v / 1000000).toFixed(1) + 'M';
  if (max >= 1000) return (v / 1000).toFixed(0) + 'k';
  if (max >= 10) return v.toFixed(0);
  return v.toFixed(1);
}

function hhmm(ts) {
  const d = new Date(ts * 1000);
  return String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0');
}

function chart({ title, series, unit, forceMax }) {
  const plotW = W - PAD.left - PAD.right;
  const plotH = H - PAD.top - PAD.bottom;
  const x = (t) => PAD.left + ((t - START) / (END - START)) * plotW;

  let peak = 0;
  for (const s of series) for (const [, v] of s.points) if (v > peak) peak = v;
  const max = forceMax !== undefined ? forceMax : niceMax(peak * 1.1);
  const y = (v) => PAD.top + plotH - (v / max) * plotH;

  const parts = [];
  parts.push(`<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}" font-family="system-ui, -apple-system, Segoe UI, sans-serif">`);
  parts.push(`<rect width="${W}" height="${H}" fill="#ffffff"/>`);
  parts.push(`<text x="${PAD.left}" y="15" font-size="13" font-weight="500" fill="#2C2C2A">${esc(title)}</text>`);

  for (let i = 0; i <= 4; i++) {
    const v = (max / 4) * i;
    const yy = y(v);
    parts.push(`<line x1="${PAD.left}" y1="${yy.toFixed(1)}" x2="${PAD.left + plotW}" y2="${yy.toFixed(1)}" stroke="#E6E4DD" stroke-width="1"/>`);
    parts.push(`<text x="${PAD.left - 8}" y="${(yy + 4).toFixed(1)}" font-size="11" fill="#5F5E5A" text-anchor="end">${fmt(v, max)}</text>`);
  }

  const ticks = 6;
  for (let i = 0; i <= ticks; i++) {
    const t = START + ((END - START) / ticks) * i;
    parts.push(`<text x="${x(t).toFixed(1)}" y="${H - 12}" font-size="11" fill="#5F5E5A" text-anchor="middle">${hhmm(t)}</text>`);
  }

  for (const m of MARKS) {
    if (m.at < START || m.at > END) continue;
    const xx = x(m.at);
    parts.push(`<line x1="${xx.toFixed(1)}" y1="${PAD.top}" x2="${xx.toFixed(1)}" y2="${PAD.top + plotH}" stroke="#A32D2D" stroke-width="1.2" stroke-dasharray="5 4"/>`);
    parts.push(`<text x="${(xx + 4).toFixed(1)}" y="${PAD.top + 12}" font-size="11" fill="#A32D2D">${esc(m.label)}</text>`);
  }

  series.forEach((s, i) => {
    const color = COLORS[i % COLORS.length];
    const d = s.points
      .map(([t, v], idx) => `${idx === 0 ? 'M' : 'L'}${x(t).toFixed(1)},${y(v).toFixed(1)}`)
      .join(' ');
    parts.push(`<path d="${d}" fill="none" stroke="${color}" stroke-width="1.8" stroke-linejoin="round"/>`);
    const ly = PAD.top + 6 + i * 18;
    parts.push(`<line x1="${W - PAD.right + 12}" y1="${ly}" x2="${W - PAD.right + 32}" y2="${ly}" stroke="${color}" stroke-width="2.5"/>`);
    parts.push(`<text x="${W - PAD.right + 38}" y="${ly + 4}" font-size="11" fill="#2C2C2A">${esc(s.name)}</text>`);
  });

  parts.push(`<text x="${PAD.left}" y="${H - 12}" font-size="11" fill="#888780">${esc(unit)}</text>`);
  parts.push('</svg>');
  return parts.join('\n');
}

async function build(name, title, expr, legend, unit, opts = {}) {
  const step = opts.step || 10;
  const raw = await query(expr, step);
  const series = raw.map((r) => ({
    name: legend(r.metric),
    points: r.values.map(([t, v]) => [Number(t), Number(v)]).filter(([, v]) => Number.isFinite(v))
  })).filter((s) => s.points.length);
  if (!series.length) {
    console.log(`  ${name}: нет данных, пропускаю`);
    return null;
  }
  const svg = chart({ title, series, unit, forceMax: opts.forceMax });
  const file = path.join(OUT, `${name}.svg`);
  fs.writeFileSync(file, svg);
  const peak = Math.max(...series.flatMap((s) => s.points.map(([, v]) => v)));
  console.log(`  ${name}: серий ${series.length}, пик ${peak.toFixed(1)} -> ${file}`);
  return file;
}

(async () => {
  fs.mkdirSync(OUT, { recursive: true });
  console.log(`окно ${new Date(START * 1000).toLocaleTimeString()} .. ${new Date(END * 1000).toLocaleTimeString()}`);

  await build('availability-pg', 'Доступность нод PostgreSQL (1 = отвечает)', 'pg_up',
    (m) => m.node, 'по данным postgres-exporter', { forceMax: 1.2 });

  await build('availability-backend', 'Доступность инстансов приложения (1 = отвечает)', 'up{job="backend"}',
    (m) => m.node, 'по данным Prometheus', { forceMax: 1.2 });

  await build('haproxy-status', 'Реплика в ротации haproxy (1 = UP)', 'haproxy_server_status{proxy="pg_read", state="UP"}',
    (m) => m.server, 'по данным haproxy', { forceMax: 1.2 });

  await build('nginx-rps', 'nginx: запросы в секунду по статусам', 'sum by (status) (rate(nginx_http_response_count_total[1m]))',
    (m) => `HTTP ${m.status}`, 'запросов/с, финальный статус клиенту');

  await build('nginx-retries', 'nginx: отказы апстримов, скрытые ретраем', 'sum by (upstream_status) (rate(nginx_http_response_count_total{upstream_status!~"200|-"}[1m]))',
    (m) => m.upstream_status, 'попыток/с');

  await build('backend-rps', 'Запросы в секунду по инстансам приложения', 'sum by (node) (rate(http_requests_received_total[1m]))',
    (m) => m.node, 'запросов/с');

  await build('pg-reads', 'PostgreSQL: прочитано строк в секунду', 'sum by (node) (rate(pg_stat_database_tup_fetched{datname="people_hub"}[1m]))',
    (m) => m.node, 'строк/с');

  await build('haproxy-sessions', 'haproxy: активные соединения к репликам', 'haproxy_server_current_sessions{proxy="pg_read"}',
    (m) => m.server, 'соединений');
})();
