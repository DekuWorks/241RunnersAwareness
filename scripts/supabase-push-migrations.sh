#!/usr/bin/env bash
# Apply Supabase migrations to linked project (staging)
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if ! command -v supabase >/dev/null 2>&1; then
  echo "supabase CLI not found. Install: https://supabase.com/docs/guides/cli"
  exit 1
fi

echo "Pushing migrations from supabase/migrations ..."
supabase db push

echo "Done. Verify with: supabase db diff"
