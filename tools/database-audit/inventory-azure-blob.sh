#!/usr/bin/env bash
# Read-only inventory of Azure Blob containers for 241runnersstorage.
# Does not download blob contents. Outputs counts and sizes only.
set -euo pipefail

STORAGE_ACCOUNT="${AZURE_STORAGE_ACCOUNT:-241runnersstorage}"
RESOURCE_GROUP="${AZURE_STORAGE_RG:-241raLinux_group}"
TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"
OUT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)/artifacts/blob-audit/$TIMESTAMP"

if ! az account show >/dev/null 2>&1; then
  echo "Run: az login"
  exit 1
fi

mkdir -p "$OUT_DIR"

echo "Storage account: $STORAGE_ACCOUNT (RG: $RESOURCE_GROUP)" | tee "$OUT_DIR/summary.txt"
echo "Generated (UTC): $TIMESTAMP" | tee -a "$OUT_DIR/summary.txt"
echo "" | tee -a "$OUT_DIR/summary.txt"

# List containers (metadata only)
az storage container list \
  --account-name "$STORAGE_ACCOUNT" \
  --auth-mode login \
  --query "[].{name:name, publicAccess:properties.publicAccess}" \
  -o table 2>"$OUT_DIR/errors.txt" | tee -a "$OUT_DIR/summary.txt" || {
  echo "Note: --auth-mode login may require Storage Blob Data Reader role."
  echo "Alternative: set AZURE_STORAGE_CONNECTION_STRING and use --connection-string."
  cat "$OUT_DIR/errors.txt"
}

# Expected application container from codebase
echo "" | tee -a "$OUT_DIR/summary.txt"
echo "Expected app container (from code): images" | tee -a "$OUT_DIR/summary.txt"

# If connection string is available (do not log the string)
if [[ -n "${AZURE_STORAGE_CONNECTION_STRING:-}" ]]; then
  for container in images; do
    echo "Container: $container" | tee -a "$OUT_DIR/summary.txt"
    az storage blob list \
      --container-name "$container" \
      --connection-string "$AZURE_STORAGE_CONNECTION_STRING" \
      --query "length(@)" -o tsv 2>/dev/null | xargs -I{} echo "  blob_count: {}" | tee -a "$OUT_DIR/summary.txt" || true
    az storage blob list \
      --container-name "$container" \
      --connection-string "$AZURE_STORAGE_CONNECTION_STRING" \
      --query "[].{size:properties.contentLength, contentType:properties.contentType}" \
      -o json 2>/dev/null > "$OUT_DIR/${container}-metadata.json" || true
  done
fi

echo ""
echo "Blob inventory written to: $OUT_DIR"
