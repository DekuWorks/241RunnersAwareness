#!/usr/bin/env bash
# Deploy 241RunnersAPI to Fly.io (requires active Fly billing).
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

: "${FLY_API_TOKEN:?Set FLY_API_TOKEN (fly auth token)}"

if [[ -f .env ]]; then
  set -a
  # shellcheck disable=SC1091
  source .env
  set +a
fi

APP_NAME="${FLY_APP_NAME:-241runners-api}"
JWT_KEY="${JWT_KEY:-241RunnersAwareness2024-Staging-SuperSecure-JWT-Key-For-Authentication-With-Enhanced-Security-Staging-Environment-Only}"
CONN="${ConnectionStrings__DefaultConnection%\"}"
CONN="${CONN#\"}"
PUBLIC_URL="https://${APP_NAME}.fly.dev"

flyctl apps create "$APP_NAME" --org personal 2>/dev/null || true

flyctl secrets set \
  "JWT_KEY=${JWT_KEY}" \
  "SUPABASE_URL=${SUPABASE_URL}" \
  "SUPABASE_SERVICE_ROLE_KEY=${SUPABASE_SERVICE_ROLE_KEY}" \
  "ConnectionStrings__DefaultConnection=${CONN}" \
  "PUBLIC_API_BASE_URL=${PUBLIC_URL}" \
  --app "$APP_NAME" --stage

flyctl deploy --remote-only --app "$APP_NAME"
echo "Deployed: ${PUBLIC_URL}/health"
