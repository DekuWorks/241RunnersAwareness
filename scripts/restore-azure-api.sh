#!/usr/bin/env bash
# Restore 241runners-api-v2 on Azure and refresh GitHub deploy secrets.
# Run from repo root after: az login --use-device-code
set -euo pipefail

WEBAPP_NAME="${WEBAPP_NAME:-241runners-api-v2}"
REPO="${GITHUB_REPO:-DekuWorks/241RunnersAwareness}"

echo "=== 241 Runners API restore ==="

if ! az account show >/dev/null 2>&1 || ! az webapp list --query "[0].name" -o tsv >/dev/null 2>&1; then
  echo "Azure CLI not logged in (or token expired). Opening device login..."
  az logout 2>/dev/null || true
  az login --use-device-code --tenant "e41153b5-1d65-4b0a-aa82-cf7a2d000346"
fi

echo "Subscription: $(az account show --query name -o tsv)"

RESOURCE_GROUP="$(az webapp list --query "[?name=='${WEBAPP_NAME}'].resourceGroup | [0]" -o tsv)"
if [[ -z "${RESOURCE_GROUP}" || "${RESOURCE_GROUP}" == "null" ]]; then
  echo "ERROR: Web app '${WEBAPP_NAME}' not found in this subscription."
  az webapp list --query "[].{name:name, rg:resourceGroup, state:state}" -o table
  exit 1
fi

echo "Web app: ${WEBAPP_NAME}"
echo "Resource group: ${RESOURCE_GROUP}"

STATE="$(az webapp show --name "${WEBAPP_NAME}" --resource-group "${RESOURCE_GROUP}" --query state -o tsv)"
echo "Current state: ${STATE}"

if [[ "${STATE}" != "Running" ]]; then
  echo "Starting web app..."
  az webapp start --name "${WEBAPP_NAME}" --resource-group "${RESOURCE_GROUP}"
  sleep 10
fi

echo "Downloading fresh publish profile..."
PROFILE_FILE="$(mktemp)"
az webapp deployment list-publishing-profiles \
  --name "${WEBAPP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --xml > "${PROFILE_FILE}"

if ! grep -q "publishUrl" "${PROFILE_FILE}"; then
  echo "ERROR: Publish profile looks invalid (missing publishUrl)."
  exit 1
fi

echo "Updating GitHub secrets..."
gh secret set AZURE_WEBAPP_PUBLISH_PROFILE < "${PROFILE_FILE}" -R "${REPO}"
gh secret set AZURE_WEBAPP_NAME -b "${WEBAPP_NAME}" -R "${REPO}"
rm -f "${PROFILE_FILE}"

echo "Re-linking GitHub Actions OIDC (optional but recommended)..."
echo "  Azure Portal -> ${WEBAPP_NAME} -> Deployment Center -> GitHub Actions -> Reconnect"
echo "  This refreshes AZUREAPPSERVICE_* federated credentials if OIDC deploy is used."

echo "Triggering API Deploy workflow..."
gh workflow run api-deploy.yml -R "${REPO}" --ref main

echo ""
echo "Done. Watch deploy:"
echo "  gh run watch -R ${REPO} \$(gh run list --workflow=api-deploy.yml -L 1 --json databaseId -q '.[0].databaseId')"
echo ""
echo "Verify API:"
echo "  curl https://${WEBAPP_NAME}.azurewebsites.net/healthz"
