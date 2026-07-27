#!/usr/bin/env bash
# Run read-only Azure SQL audit scripts and write sanitized outputs to artifacts/.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SQL_DIR="$SCRIPT_DIR/sql"
TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"
OUT_DIR="$SCRIPT_DIR/../../artifacts/database-audit/$TIMESTAMP"

SERVER="${AZURE_SQL_SERVER:-}"
DATABASE="${AZURE_SQL_DATABASE:-241RunnersAwarenessDB}"
USER="${AZURE_SQL_USER:-}"
PASSWORD="${AZURE_SQL_PASSWORD:-}"

if [[ -z "$SERVER" || -z "$USER" ]]; then
  echo "Set AZURE_SQL_SERVER, AZURE_SQL_USER, and AZURE_SQL_PASSWORD (read-only account)."
  echo "Optional: AZURE_SQL_DATABASE (default: 241RunnersAwarenessDB)"
  exit 1
fi

if ! command -v sqlcmd >/dev/null 2>&1; then
  echo "sqlcmd not found. Install SQL tools or run scripts manually in Azure Data Studio."
  echo "Scripts: $SQL_DIR/*.sql"
  exit 1
fi

mkdir -p "$OUT_DIR"

run_sql() {
  local file="$1"
  local base
  base="$(basename "$file" .sql)"
  echo "Running $base..."
  sqlcmd \
    -S "$SERVER" \
    -d "$DATABASE" \
    -U "$USER" \
    -P "$PASSWORD" \
    -C \
    -i "$file" \
    -s "|" \
    -W \
    -o "$OUT_DIR/${base}.txt"
}

for f in "$SQL_DIR"/*.sql; do
  run_sql "$f"
done

cat > "$OUT_DIR/README.txt" <<EOF
241RunnersAwareness database audit
Generated (UTC): $TIMESTAMP
Server: $SERVER
Database: $DATABASE
DO NOT COMMIT THIS FOLDER — may contain environment-specific metadata.
EOF

echo ""
echo "Audit complete: $OUT_DIR"
echo "Summarize into docs/migration/azure-sql-audit.md (no PII)."
