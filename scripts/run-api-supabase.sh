#!/usr/bin/env bash
# Run API against Supabase PostgreSQL + Storage (local dev)
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [[ -f .env ]]; then
  set -a
  # shellcheck disable=SC1091
  source .env
  set +a
fi

# Strip surrounding quotes from connection string if present
ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection%\"}"
ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection#\"}"
export ConnectionStrings__DefaultConnection

: "${SUPABASE_URL:?Set SUPABASE_URL}"
: "${SUPABASE_SERVICE_ROLE_KEY:?Set SUPABASE_SERVICE_ROLE_KEY}"
: "${ConnectionStrings__DefaultConnection:?Set ConnectionStrings__DefaultConnection or SUPABASE_DB_URL}"

export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Supabase}"
export DATABASE_PROVIDER="${DATABASE_PROVIDER:-Postgres}"
export STORAGE_PROVIDER="${STORAGE_PROVIDER:-Supabase}"
export PUBLIC_API_BASE_URL="${PUBLIC_API_BASE_URL:-http://localhost:5051}"

if [[ -z "${ConnectionStrings__DefaultConnection:-}" && -n "${SUPABASE_DB_URL:-}" ]]; then
  export ConnectionStrings__DefaultConnection="$SUPABASE_DB_URL"
fi

echo "Starting API on $PUBLIC_API_BASE_URL (Supabase Postgres + Storage)..."
dotnet run --project 241RunnersAPI/241RunnersAPI.csproj --launch-profile Supabase
