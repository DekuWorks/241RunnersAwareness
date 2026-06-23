#!/usr/bin/env bash
# Migrate all data from Azure SQL to Supabase PostgreSQL.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
TOOL_DIR="${ROOT}/241RunnersAPI/tools/MigrateToSupabase"

if [[ -z "${AZURE_SQL_CONNECTION_STRING:-}" ]]; then
  echo "Set AZURE_SQL_CONNECTION_STRING (Azure SQL / SQL Server)."
  exit 1
fi

if [[ -z "${SUPABASE_CONNECTION_STRING:-}" ]]; then
  if [[ -f "${ROOT}/241RunnersAPI/appsettings.Supabase.local.json" ]]; then
    SUPABASE_CONNECTION_STRING="$(python3 -c "
import json
with open('${ROOT}/241RunnersAPI/appsettings.Supabase.local.json') as f:
    print(json.load(f)['ConnectionStrings']['DefaultConnection'])
")"
    export SUPABASE_CONNECTION_STRING
  else
    echo "Set SUPABASE_CONNECTION_STRING or create 241RunnersAPI/appsettings.Supabase.local.json"
    exit 1
  fi
fi

echo "=== Dry run (source counts only) ==="
dotnet run --project "${TOOL_DIR}" -- --dry-run

echo ""
read -r -p "Proceed with full migration (replaces Supabase data)? [y/N] " confirm
if [[ "${confirm,,}" != "y" && "${confirm,,}" != "yes" ]]; then
  echo "Aborted."
  exit 0
fi

dotnet run --project "${TOOL_DIR}" -- --yes

echo ""
echo "Done. Restart the API to use migrated Supabase data."
