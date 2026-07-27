#!/usr/bin/env bash
# Smoke-test API flows (login, profile, cases, map, runner, register).
# Usage: API_BASE=https://your-api.example.com ./scripts/smoke-test-api.sh
set -euo pipefail

API_BASE="${API_BASE:-http://127.0.0.1:5051}"
EMAIL="${SMOKE_EMAIL:-dekuworks1@gmail.com}"
PASSWORD="${SMOKE_PASSWORD:-marcus2025}"
TEST_EMAIL="${SMOKE_REGISTER_EMAIL:-smoke-$(date +%s)@example.com}"

pass=0
fail=0

check() {
  local name="$1"
  local code="$2"
  if [[ "$code" =~ ^2 ]]; then
    echo "✅ $name ($code)"
    pass=$((pass + 1))
  else
    echo "❌ $name ($code)"
    fail=$((fail + 1))
  fi
}

echo "Smoke testing API at $API_BASE"
echo "================================"

health=$(curl -s -o /tmp/smoke-health.json -w "%{http_code}" "$API_BASE/health")
check "GET /health" "$health"

login=$(curl -s -o /tmp/smoke-login.json -w "%{http_code}" \
  -X POST "$API_BASE/api/v1/auth/login" \
  -H 'Content-Type: application/json' \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")
check "POST /api/v1/auth/login" "$login"

TOKEN=$(python3 -c "import json; d=json.load(open('/tmp/smoke-login.json')); print(d.get('token',''))" 2>/dev/null || true)
if [[ -z "$TOKEN" ]]; then
  echo "❌ No auth token — stopping authenticated checks"
  exit 1
fi

me=$(curl -s -o /tmp/smoke-me.json -w "%{http_code}" \
  -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/auth/me")
check "GET /api/v1/auth/me" "$me"

map=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE/api/public/map/missing")
check "GET /api/public/map/missing" "$map"

cases=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/cases?page=1&pageSize=5")
check "GET /api/v1/cases" "$cases"

runners=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/runner?page=1&pageSize=1")
check "GET /api/v1/runner" "$runners"

register=$(curl -s -o /tmp/smoke-reg.json -w "%{http_code}" \
  -X POST "$API_BASE/api/v1/auth/register" \
  -H 'Content-Type: application/json' \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"Test1234!\",\"firstName\":\"Smoke\",\"lastName\":\"Test\",\"role\":\"user\",\"phoneNumber\":\"+15555550199\"}")
check "POST /api/v1/auth/register" "$register"

echo "================================"
echo "Passed: $pass | Failed: $fail"
[[ "$fail" -eq 0 ]]
