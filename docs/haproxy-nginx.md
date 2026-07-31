# Балансировка: nginx перед приложением, haproxy перед репликами

```mermaid
flowchart TD
    k6["k6 / браузер"]

    NG["nginx :8090<br/>upstream app_backend<br/>least_conn, max_fails, proxy_next_upstream"]

    subgraph apps["Инстансы приложения"]
        A1["app-1 :8080"]
        A2["app-2 :8080"]
        A3["app-3 :8080"]
    end

    subgraph hap["haproxy"]
        HR[":5001<br/>backend pg_read<br/>leastconn + pgsql-check"]
    end

    M[("pg-master :5432<br/>primary")]
    R1[("pg-replica-1<br/>standby")]
    R2[("pg-replica-2<br/>standby")]

    k6 --> NG
    NG --> A1
    NG --> A2
    NG --> A3

    apps -- "запись напрямую<br/>TargetSessionAttributes=Primary" --> M
    apps -- "чтение<br/>PreferStandby" --> HR

    HR --> R1
    HR --> R2

    M -. "потоковая репликация, слоты<br/>ANY 1 (replica_1, replica_2)" .-> R1
    M -.-> R2
```

Строка подключения приложения: `Host=pg-master:5432,haproxy:5001`.
Npgsql выбирает хост по `TargetSessionAttributes` (см. `PeopleHub.Infrastructure/Db/DbClient.cs`):
запись уходит на мастер напрямую, чтение — на haproxy, который раскидывает его по живым репликам.
