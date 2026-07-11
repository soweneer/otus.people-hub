#!/bin/bash
# Entrypoint реплики: при пустом каталоге данных снимает базовую копию с мастера
# (pg_basebackup -R создаёт standby.signal и primary_conninfo), затем стартует postgres
# в режиме hot standby.
set -e

PGDATA=${PGDATA:-/var/lib/postgresql/data}

if [ ! -s "$PGDATA/PG_VERSION" ]; then
    echo "Initializing replica from pg-master (slot: $REPLICATION_SLOT)..."
    export PGPASSWORD="$REPLICATION_PASSWORD"
    until pg_basebackup \
        --pgdata="$PGDATA" \
        --host=pg-master \
        --port=5432 \
        --username="$REPLICATION_USER" \
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
