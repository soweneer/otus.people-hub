# Мониторинг: Prometheus, Zabbix, Grafana

## Запуск

Всё поднимается из корня репозитория:

```bash
docker compose up -d --build
```

После того как поднялся Zabbix, один раз выполняется провижининг хоста:

```bash
powershell -ExecutionPolicy Bypass -File docker/zabbix/provision.ps1
```

Скрипт идемпотентный: повторный запуск не создаёт дубликатов, а переприкрепляет
шаблоны и пропускает уже существующие триггеры.

### Что где живет

| URL |  |
|---|---|
| <http://localhost:8090> | приложение |
| <http://localhost:3000> | Grafana |
| <http://localhost:9090> | Prometheus |
| <http://localhost:8095> | Zabbix (Admin / zabbix) |
| <http://localhost:8091/metrics> | метрики сервиса диалогов |
| <http://localhost:8093/metrics> | метрики сервиса счётчиков |
| <http://localhost:15672> | RabbitMQ (guest / guest) |

Пароль `Admin` в Zabbix — дефолтный из коробки, при первом входе его надо сменить.

## 1. Бизнес-метрики сервиса чатов по RED

RED снимается на транспортном слое сервиса диалогов: в конвейер gRPC добавлен
[MetricsInterceptor](../src/backend/PeopleHub.Chats/Grpc/MetricsInterceptor.cs).
Он стоит снаружи `ErrorHandlingInterceptor`, поэтому видит уже смапленный
gRPC-статус, а не исходное исключение: `DomainException` доезжает до метрики как
`InvalidArgument`, transient-ошибка Npgsql — как `Unavailable`, всё непойманное —
как `Unknown`.

| Метрика | Тип | Что показывает |
|---|---|---|
| `chats_requests_total{method,status}` | counter | **R** и **E**: поток запросов и его разбивка по gRPC-статусам |
| `chats_request_duration_seconds{method}` | histogram | **D**: время обработки, экспоненциальные бакеты от 0.5 мс |
| `chats_requests_in_flight{method}` | gauge | сколько запросов сервис держит прямо сейчас |
| `chats_messages_sent_total` | counter | отправленных сообщений |
| `chats_dialogs_read_total` | counter | отметок «диалог прочитан» |
| `chats_dialog_page_size` | histogram | сколько сообщений вернуло чтение диалога |
| `chats_unread_messages_returned` | histogram | сколько непрочитанных отдано за запрос |

Транспортные метрики говорят, что сервису плохо, бизнесовые — что именно перестало
работать. Падение `chats_messages_sent_total` при живом `chats_requests_total`
означает, что запросы ходят, а сообщения не пишутся.

Семейства метрик публикуются нулями на старте (`ChatsMetrics.Publish`,
`DialogMetrics.Publish`) — иначе prometheus-net не отдаёт метрику с лейблами до
первого обращения, и панели показывают «No data» вместо нуля.

Запросы, на которых построены панели:

```promql
sum by (method) (rate(chats_requests_total[1m]))
sum by (status) (rate(chats_requests_total{status!="OK"}[1m]))
histogram_quantile(0.95, sum by (le, method) (rate(chats_request_duration_seconds_bucket[1m])))
sum(rate(chats_requests_total{status!="OK"}[5m])) / sum(rate(chats_requests_total[5m]))
```

Prometheus уже скрёб `chats:8091` по job `chats` — конфиг
[prometheus.yml](../docker/prometheus/prometheus.yml) править не пришлось.

## 2. Технические метрики сервера в Zabbix

В compose добавлены четыре сервиса: `zabbix-db` (отдельный PostgreSQL, чтобы не
лезть в базу монолита с её репликацией), `zabbix-server`, `zabbix-web` и
`zabbix-agent` на образе agent2.

Агент запущен с `pid: host` и смонтированными `/:/hostfs:ro` и docker-сокетом,
поэтому отдаёт метрики хоста, а не своего контейнера: `/proc` в контейнере без
lxcfs показывает хостовые значения. Провижининг вешает на хост `chats-host` два
штатных шаблона:

* **Linux by Zabbix agent** — CPU, load average, память, swap, файловые системы,
  диски, сетевые интерфейсы, uptime, процессы;
* **Docker by Zabbix agent 2** — состояние демона и по каждому контейнеру, включая
  `people-hub-chats`: CPU, память, рестарты, health-статус.

Сверх шаблонных триггеров скрипт добавляет два своих:

| Триггер | Выражение |
|---|---|
| Загрузка CPU выше 80% пять минут | `min(/chats-host/system.cpu.util,5m)>80` |
| Свободной памяти меньше 20% | `max(/chats-host/vm.memory.size[pavailable],5m)<20` |

Триггер создаётся только если на хосте реально есть элемент с нужным ключом —
скрипт сверяется с `item.get` и пропускает остальные. На 7.4 так отсеялся третий
кандидат: элемента `vm.memory.utilization` в шаблоне нет.

Что важно знать про этот стенд:

* «Сервер» здесь — виртуальная машина WSL2, в которой живёт Docker Desktop.
  Метрики хостовой Windows агент не видит, и это ожидаемо: сервис чатов работает
  именно в этой виртуалке.
* Сетевые интерфейсы агент видит свои, контейнерные, потому что подключён к сети
  compose, а не к сети хоста. CPU, память, диски и файловые системы — хостовые.
* Из 199 элементов 8 остаются unsupported: `docker.data_usage`, `docker.layers_size`,
  `docker.containers_size`, `docker.images_size`, `docker.volumes_size` не
  укладываются в таймаут агента на этом объёме образов, `system.sw.packages.get`
  не работает из alpine-контейнера, а `docker.kernel_mem*` относятся к полям,
  которых больше нет в ответе современного Docker. На сбор CPU, памяти, дисков и
  состояния контейнеров это не влияет.

## 3. Дашборд в Grafana

В [people-hub.json](../docker/grafana/dashboards/people-hub.json) добавлена строка
«Сервис чатов, RED» из шести панелей:

| Панель | Что отвечает |
|---|---|
| Rate: запросы по методам | сколько и чего сервис обслуживает |
| Errors: отказы по gRPC-статусам | какие именно отказы и в каком объёме |
| Duration: p95 по методам | время ответа, плюс общий p99 для хвоста |
| Доля ошибок | одно число для бюджета ошибок, с порогами 1% и 5% |
| Бизнес-операции диалогов | отправка сообщений и отметки о прочтении |
| Объём выдачи, p95 | сколько сообщений уезжает за один запрос |

Zabbix датасорсом в Grafana сознательно не подключался: технические метрики
смотрятся в своём интерфейсе, дашборд остаётся про поведение сервиса.

## Проверка

Нагрузка — штатный сценарий диалогов прямо в gRPC, мимо веб-слоя:

```bash
DURATION=90s READ_VUS=10 WRITE_VUS=5 k6 run load-testing/dialogs-grpc.js
```

Что показал прогон (90 секунд, 10 читающих и 5 пишущих VU):

| Показатель | Значение |
|---|---|
| checks | 29908 из 29908 |
| Rate, `Dialogs/List` | 44.4 rps |
| Rate, `Dialogs/Send` | 12.0 rps |
| p95 `List`, со стороны сервиса | 13 мс |
| p95 `Send`, со стороны сервиса | 203 мс |
| p95 `List` / `Send`, со стороны k6 | 38 мс / 199 мс |
| отправлено сообщений | 12.0 в секунду |
| p95 объёма выдачи диалога | 62 сообщения |

Расхождение p95 у `List` (13 мс против 38 мс) — это сеть и сериализация на
клиенте: гистограмма сервиса меряет только обработку внутри него.

Отдельно проверялась ветка ошибок: три десятка `Send` с пустым текстом дали
`InvalidArgument` 0.42 rps и долю ошибок 0.7% на соответствующих панелях.

Технические метрики проверены на стороне Zabbix: элемент `system.cpu.util` на
`chats-host` пишет значения с шагом 60 секунд (27.6% на пике нагрузки против 7.8%
в покое), LLD обнаружил все 24 контейнера стенда, включая `people-hub-chats`.
Доступность агента отдельно подтверждается так:

```bash
docker exec zabbix-server zabbix_get -s zabbix-agent -k system.cpu.util
```

Все цели Prometheus в состоянии `up`: backend 3, chats, counters, haproxy, nginx,
postgres 3.
