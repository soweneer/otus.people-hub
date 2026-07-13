#!/bin/bash
set -e

PGDATA=${PGDATA:-/var/lib/postgresql/data}

if [ ! -s "$PGDATA/PG_VERSION" ]; then
    echo "Initializing replica from pg-master (slot: $REPLICATION_SLOT)..."
    export PGPASSWORD="$REPLICATION_PASSWORD"
    until pg_basebackup \
        --pgdata="$PGDATA" \
        --dbname="host=pg-master port=5432 user=${REPLICATION_USER} application_name=${REPLICA_NAME}" \
        --slot="$REPLICATION_SLOT" \
        --wal-method=stream \
        --write-recovery-conf \
        --progress
    do
        echo "pg-master is not ready yet, retrying in 2s..."
        sleep 2
    done
    chmod 0700 "$PGDATA"
fi

exec postgres
