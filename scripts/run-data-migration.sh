#!/usr/bin/env bash
# Copy Azure SQL data to Supabase PostgreSQL (staging only)
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [[ -f .env ]]; then
  set -a
  # shellcheck disable=SC1091
  source .env
  set +a
fi

: "${AZURE_SQL_CONNECTION_STRING:?Set AZURE_SQL_CONNECTION_STRING}"
: "${SUPABASE_DB_URL:?Set SUPABASE_DB_URL}"

export MIGRATION_MANIFEST_PATH="${MIGRATION_MANIFEST_PATH:-config/migration-manifest.json}"
export MIGRATION_DRY_RUN="${MIGRATION_DRY_RUN:-true}"

echo "Running data migration tool (MIGRATION_DRY_RUN=$MIGRATION_DRY_RUN) ..."
dotnet run --project tools/241Runners.DataMigration/241Runners.DataMigration.csproj
