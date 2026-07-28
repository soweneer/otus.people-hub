local clock = require('clock')
local fiber = require('fiber')
local netbox = require('net.box')

local MODE = os.getenv('BENCH_MODE') or 'read'
local CONCURRENCY = tonumber(os.getenv('BENCH_CONCURRENCY')) or 8
local DURATION = tonumber(os.getenv('BENCH_DURATION')) or 20

local latencies = {}
local finished = 0
local deadline = clock.monotonic() + DURATION

local function measure(conn)
    local started = clock.monotonic()

    if MODE == 'read' then
        local i = math.random(0, 39)
        conn:call('dialog_list', { 2000000 + i, 3000000 + i })
    else
        local i = math.random(0, 199)
        conn:call('dialog_send', { 4200000 + i, 5200000 + i, 'storage bench' })
    end

    table.insert(latencies, (clock.monotonic() - started) * 1000)
end

local function worker()
    local conn = netbox.connect('127.0.0.1:3301')

    while clock.monotonic() < deadline do
        measure(conn)
    end

    conn:close()
    finished = finished + 1
end

for _ = 1, CONCURRENCY do
    fiber.create(worker)
end

while finished < CONCURRENCY do
    fiber.sleep(0.05)
end

table.sort(latencies)

local sum = 0
for _, value in ipairs(latencies) do
    sum = sum + value
end

local function percentile(p)
    return latencies[math.max(1, math.floor(#latencies * p))]
end

print(string.format('tarantool %s: n=%d tps=%.1f avg=%.3fms p95=%.3fms p99=%.3fms max=%.3fms',
    MODE,
    #latencies,
    #latencies / DURATION,
    sum / #latencies,
    percentile(0.95),
    percentile(0.99),
    latencies[#latencies]))

os.exit(0)
