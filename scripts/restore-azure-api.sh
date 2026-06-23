#!/usr/bin/env bash
# Start 241 Runners API on Azure and apply app settings from GitHub secrets.
set -euo pipefail

REPO="${GITHUB_REPO:-DekuWorks/241RunnersAwareness}"
APP_NAME="${AZURE_WEBAPP_NAME:-241runners-api-v2}"

echo "=== 241 Runners — Azure API restore ==="

if ! az account show >/dev/null 2>&1; then
  echo "Run: az login"
  exit 1
fi

RG="$(az webapp list --query "[?name=='$APP_NAME'].resourceGroup | [0]" -o tsv)"
if [[ -z "$RG" || "$RG" == "null" ]]; then
  echo "Could not find App Service: $APP_NAME"
  exit 1
fi

echo "App: $APP_NAME  Resource group: $RG"

STATE="$(az webapp show --name "$APP_NAME" --resource-group "$RG" --query state -o tsv)"
echo "Current state: $STATE"

if [[ "$STATE" != "Running" ]]; then
  echo "Starting App Service..."
  az webapp start --name "$APP_NAME" --resource-group "$RG"
fi

if [[ -n "${AZURE_SQL_CONNECTION_STRING:-}" ]]; then
  CS="$AZURE_SQL_CONNECTION_STRING"
else
  echo "Set AZURE_SQL_CONNECTION_STRING before running (GitHub secret value)."
  CS=""
fi

if [[ -n "${CS:-}" ]]; then
  SQL_HOST=$(echo "$CS" | sed -nE 's/.*[Ss]erver=(tcp:)?([^,;]+).*/\2/p' | head -1)
  if [[ -z "$SQL_HOST" ]] || ! host "$SQL_HOST" >/dev/null 2>&1; then
    echo "Connection string server does not resolve: ${SQL_HOST:-<missing>}"
    exit 1
  fi
  echo "Applying connection string (server: $SQL_HOST) and JWT settings..."
  if [[ -z "${JWT_KEY:-}" ]]; then
    echo "Warning: JWT_KEY not set; skipping JWT app setting."
  fi
  az webapp config appsettings delete \
    --name "$APP_NAME" \
    --resource-group "$RG" \
    --setting-names DefaultConnection 2>/dev/null || true
  az webapp config appsettings set \
    --name "$APP_NAME" \
    --resource-group "$RG" \
    --settings \
      "ConnectionStrings__DefaultConnection=$CS" \
      "JWT_KEY=${JWT_KEY:-}" \
      "JWT_ISSUER=241RunnersAwareness" \
      "JWT_AUDIENCE=241RunnersAwareness" \
      "ASPNETCORE_ENVIRONMENT=Production"
fi

az webapp config set --name "$APP_NAME" --resource-group "$RG" --always-on true

echo ""
echo "Deploy latest API:"
echo "  gh workflow run api-deploy.yml -R $REPO"
echo ""
echo "Health:"
echo "  curl https://${APP_NAME}.azurewebsites.net/api/v1.0/auth/health"
