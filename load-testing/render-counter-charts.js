const fs = require('fs');
const path = require('path');
const { query, chart, build, OUT } = require('./render-charts');

async function combined(name, title, unit, definitions, step = 10) {
  const series = [];

  for (const definition of definitions) {
    const raw = await query(definition.expr, step);
    for (const result of raw) {
      const points = result.values.map(([t, v]) => [Number(t), Number(v)]).filter(([, v]) => Number.isFinite(v));
      if (points.length) {
        series.push({ name: definition.legend(result.metric), points });
      }
    }
  }

  if (!series.length) {
    console.log(`  ${name}: нет данных, пропускаю`);
    return null;
  }

  const file = path.join(OUT, `${name}.svg`);
  fs.writeFileSync(file, chart({ title, series, unit }));

  const peak = Math.max(...series.flatMap((s) => s.points.map(([, v]) => v)));
  console.log(`  ${name}: серий ${series.length}, пик ${peak.toFixed(3)} -> ${file}`);

  return file;
}

(async () => {
  fs.mkdirSync(OUT, { recursive: true });

  await combined('counters-read-latency', 'Чтение счётчиков внутри сервиса', 'секунд', [
    {
      expr: 'histogram_quantile(0.95, sum by (le) (rate(counters_read_duration_seconds_bucket[1m])))',
      legend: () => 'p95'
    },
    {
      expr: 'histogram_quantile(0.99, sum by (le) (rate(counters_read_duration_seconds_bucket[1m])))',
      legend: () => 'p99'
    }
  ]);

  await build('counters-read-rps', 'Чтения счётчиков в секунду',
    'sum(rate(counters_read_duration_seconds_count[1m]))', () => 'GetCounters', 'запросов/с');

  await combined('counters-apply', 'Применение событий саги', 'событий/с', [
    {
      expr: 'sum by (type) (rate(counters_events_applied_total[1m]))',
      legend: (m) => m.type
    },
    {
      expr: 'sum(rate(counters_events_failed_total[1m]))',
      legend: () => 'dead-letter'
    }
  ]);

  await build('counters-outbox-lag', 'Отставание аутбокса сервиса диалогов',
    'chats_outbox_lag_seconds', () => 'возраст самого старого события', 'секунд');

  await build('counters-outbox-pending', 'Неопубликованные события в аутбоксе',
    'chats_outbox_pending', () => 'строк в очереди', 'событий');
})();
