#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE ROLE ${REPLICATION_USER} WITH REPLICATION LOGIN PASSWORD '${REPLICATION_PASSWORD}';
    SELECT pg_create_physical_replication_slot('replica_1_slot');
    SELECT pg_create_physical_replication_slot('replica_2_slot');
    ALTER SYSTEM SET synchronous_standby_names = 'ANY 1 (replica_1, replica_2)';
EOSQL

echo "host replication ${REPLICATION_USER} all scram-sha-256" >> "$PGDATA/pg_hba.conf"
