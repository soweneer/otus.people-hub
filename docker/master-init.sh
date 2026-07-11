#!/bin/bash
# Выполняется docker-entrypoint'ом один раз при первичной инициализации кластера.
# Создаёт роль для репликации и физические слоты под каждую реплику,
# а также разрешает replication-подключения в pg_hba.conf.
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE ROLE ${REPLICATION_USER} WITH REPLICATION LOGIN PASSWORD '${REPLICATION_PASSWORD}';
    SELECT pg_create_physical_replication_slot('replica_1_slot');
    SELECT pg_create_physical_replication_slot('replica_2_slot');
EOSQL

# строки "host all ..." не покрывают replication-подключения — нужна отдельная запись
echo "host replication ${REPLICATION_USER} all scram-sha-256" >> "$PGDATA/pg_hba.conf"
