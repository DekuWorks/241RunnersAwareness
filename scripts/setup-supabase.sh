#!/usr/bin/env bash
# Fresh Supabase setup for 241 Runners API (no Azure SQL).
set -euo pipefail

REPO="${GITHUB_REPO:-DekuWorks/241RunnersAwareness}"
PROJECT_REF="${SUPABASE_PROJECT_REF:-}"

echo "=== 241 Runners — Supabase fresh database ==="
echo ""
echo "Project dashboard: https://supabase.com/dashboard/project/${PROJECT_REF:-YOUR_REF}"
echo ""
echo "If creating via CLI:"
echo "  supabase projects create 241RunnersAwareness --org-id YOUR_ORG --region us-east-1 --db-password 'STRONG_PASSWORD' --yes"
echo ""
echo "Pooler host is often aws-1-us-east-1 (not aws-0). Test with:"
echo "  Host=aws-1-us-east-1.pooler.supabase.com;Username=postgres.PROJECT_REF;Port=5432"
echo ""

if [[ -z "${SUPABASE_CONNECTION_STRING:-}" ]]; then
  read -r -p "Paste full Npgsql connection string: " SUPABASE_CONNECTION_STRING
fi

gh secret set SUPABASE_CONNECTION_STRING -b "${SUPABASE_CONNECTION_STRING}" -R "${REPO}"
gh secret set ConnectionStrings__DefaultConnection -b "${SUPABASE_CONNECTION_STRING}" -R "${REPO}"

echo "Secrets saved. Deploy API:"
echo "  gh workflow run api-deploy.yml -R ${REPO} --ref main"
