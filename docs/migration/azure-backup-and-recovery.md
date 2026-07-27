# Azure Backup and Recovery Plan

**Project:** 241RunnersAwareness.org  
**Last updated:** 2026-07-26  
**Status:** Documented procedure — **live backup not executed in this phase** (requires subscription access approval)

---

## Purpose

Define how to back up production Azure resources **before** any Supabase migration cutover. Backups support rollback and forensic comparison. **No backup files belong in git.**

---

## Production Resource Inventory

| Resource | Name | Resource group | Notes |
|----------|------|----------------|-------|
| App Service (API) | `241runners-api-v2` | `241raLinux_group` | State as of 2026-07-26: **AdminDisabled** (returns HTTP 403) |
| SQL Server | `241runners-sql-2025` | `241runnersawareness-rg` | Location: `centralus` |
| SQL Database | `241RunnersAwarenessDB` | MANUAL CONFIRMATION REQUIRED | Name inferred from connection string patterns in codebase |
| Storage account | `241runnersstorage` | `241raLinux_group` | Kind: StorageV2 |
| Blob container (app) | `images` | — | From `BlobImageStorageService.cs` |
| Frontend | GitHub Pages | `DekuWorks/241RunnersAwareness` | Custom domain: `www.241runnersawareness.org` |
| Azure subscription | `Azure Cloud Sub` | `21864b8e-adc5-4f22-9f39-0f936aae95d4` | **Disabled / read-only** as of 2026-07-26 |

---

## Pre-Backup Checklist

- [ ] Confirm Azure subscription is **active** (currently BLOCKED — re-enable before backup)
- [ ] Notify stakeholders of backup window (read-only impact if using copy-only backup)
- [ ] Record current git commit SHA: `386dd3f274aa` (main, 2026-07-16) — verify latest before cutover
- [ ] Record EF migration head from `__EFMigrationsHistory` after audit
- [ ] Verify API health endpoints respond (when App Service is running)
- [ ] Assign backup operator and approver (names: MANUAL CONFIRMATION REQUIRED)

---

## 1. Azure SQL — BACPAC Export

### When to run

- Before initial Supabase data migration rehearsal
- Immediately before production cutover (final backup)
- After any production schema change

### Procedure (Portal)

1. Azure Portal → SQL databases → select `241RunnersAwarenessDB`
2. **Export** → choose storage account (dedicated backup account recommended, not `241runnersstorage` app account)
3. Use SQL admin credentials from Key Vault — **never store in repo**
4. Name file: `241RunnersAwarenessDB-pre-migration-YYYYMMDD-HHMM.bacpac`
5. Enable encryption at rest on destination storage (Microsoft-managed keys minimum)

### Procedure (CLI)

```bash
# MANUAL CONFIRMATION REQUIRED — verify names and subscription
export RESOURCE_GROUP="241runnersawareness-rg"
export SERVER_NAME="241runners-sql-2025"
export DATABASE_NAME="241RunnersAwarenessDB"
export STORAGE_URI="https://<backup-storage>.blob.core.windows.net/backups/241RunnersAwarenessDB-$(date -u +%Y%m%dT%H%M%Z).bacpac"
export SQL_ADMIN_USER="..."   # from Key Vault
export SQL_ADMIN_PASSWORD="..." # prompt / Key Vault

az sql db export \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SERVER_NAME" \
  --name "$DATABASE_NAME" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASSWORD" \
  --storage-key-type StorageAccessKey \
  --storage-key "<storage-account-key>" \
  --storage-uri "$STORAGE_URI"
```

### Export metadata to record

| Field | Value |
|-------|-------|
| Export timestamp (UTC) | |
| BACPAC path | |
| File size (bytes) | |
| Operator | |
| EF migration head | |
| Row counts checksum | From `tools/database-audit` |

### Backup verification

1. Import BACPAC to a **new** temporary database on a dev server
2. Run `tools/database-audit/run-audit.sh` against the restored copy
3. Compare row counts to production audit
4. Delete temp database after validation (retain BACPAC)

### Retention

| Tier | Duration | Location |
|------|----------|----------|
| Pre-migration | Minimum **90 days** after successful Supabase cutover | Encrypted blob storage, separate RG |
| Pre-cutover final | Minimum **180 days** | Same |
| Deletion | Requires written approval from backup approver | See `azure-decommission-plan.md` |

---

## 2. Azure Blob Storage Backup

### Scope

- Container: `images` (runner/case photos)
- Any additional containers discovered via `tools/database-audit/inventory-azure-blob.sh`

### Procedure

```bash
# Option A: AzCopy sync to backup account (preferred)
azcopy copy \
  "https://241runnersstorage.blob.core.windows.net/images?<SAS_OR_AUTH>" \
  "https://<backup-account>.blob.core.windows.net/241runners-images-backup-YYYYMMDD?<SAS>" \
  --recursive

# Option B: Enable soft delete + versioning on production account (defense in depth)
az storage account blob-service-properties update \
  --account-name 241runnersstorage \
  --resource-group 241raLinux_group \
  --enable-delete-retention true \
  --delete-retention-days 30 \
  --enable-versioning true
```

Record:

- Container names and blob counts
- Total size (GB)
- Sample hash verification (10 random blobs) — compare after Supabase storage migration

**Do not** download blob contents into the repository.

---

## 3. App Service Configuration Backup

Export app settings and connection strings (values stored securely, not in git):

```bash
az webapp config appsettings list \
  --name 241runners-api-v2 \
  --resource-group 241raLinux_group \
  -o json > app-settings-backup-YYYYMMDD.json
# Store file in encrypted backup vault — REDACT before any sharing
```

### Settings to inventory (names only)

| Setting | Purpose |
|---------|---------|
| `ConnectionStrings__DefaultConnection` | Azure SQL |
| `JWT_KEY` | API JWT signing |
| `JWT_ISSUER` / `JWT_AUDIENCE` | Token validation |
| `ASPNETCORE_ENVIRONMENT` | Production |
| `ConnectionStrings__AzureStorageConnectionString` or `AzureStorageConnectionString` | Blob storage |
| `FIREBASE_SERVICE_ACCOUNT_JSON` | Push notifications |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Telemetry (if set) |

GitHub Actions secrets (names only): `AZURE_SQL_CONNECTION_STRING`, `JWT_KEY`, `AZURE_WEBAPP_NAME`, `AZURE_CREDENTIALS`, `AZURE_WEBAPP_PUBLISH_PROFILE`.

---

## 4. DNS Configuration Backup

Document current DNS (export from registrar):

| Host | Type | Target |
|------|------|--------|
| `www.241runnersawareness.org` | CNAME | `dekuworks.github.io` |
| `241runnersawareness.org` | A | GitHub Pages IPs (see GitHub docs) |

Repo `CNAME` file: `www.241runnersawareness.org`

---

## 5. Git / Deployment Tags

Before cutover, tag the production API and site:

```bash
git fetch origin main
git tag -a production-pre-supabase-YYYYMMDD -m "Last known good before Supabase cutover" <sha>
git push origin production-pre-supabase-YYYYMMDD
```

Record:

- API deploy workflow run ID
- GitHub Pages deployment run ID
- Container image / publish artifact hash if applicable

---

## 6. Restore Test Procedure

**Frequency:** At least once before production cutover rehearsal.

| Step | Action | Success criteria |
|------|--------|------------------|
| 1 | Restore BACPAC to isolated SQL server | Database online |
| 2 | Point **staging** API at restored DB | `/readyz` returns 200 |
| 3 | Smoke test auth, public cases, map | Contract tests pass |
| 4 | Restore blob copy to test container | Image URLs resolve |
| 5 | Document restore duration | Record in runbook |

---

## 7. Rollback Using Backups

If cutover fails:

1. Stop writes to Supabase
2. Reconfigure App Service `ConnectionStrings__DefaultConnection` to Azure SQL (unchanged if never switched)
3. Redeploy API tag `production-pre-supabase-*`
4. Revert `config.json` / frontend if changed
5. Validate `/api/v1/auth/health` and public site
6. Preserve Supabase data and migration logs for analysis

Full steps: `docs/migration/rollback-runbook.md` (Phase 10).

---

## 8. Access and Encryption

- Backup storage account: **private**, no public access
- RBAC: backup operator + approver only
- Encryption: Microsoft-managed or customer-managed keys (MANUAL CONFIRMATION REQUIRED)
- No BACPAC or app settings files in source control

---

## 9. Approval for Backup Deletion

| Role | Name | Signature / date |
|------|------|------------------|
| Backup approver | MANUAL CONFIRMATION REQUIRED | |
| Technical lead | MANUAL CONFIRMATION REQUIRED | |

Deletion must not be automated. Minimum retention per §1.

---

## Current Blockers (2026-07-26)

1. **Azure subscription disabled** — re-enable before BACPAC export or App Service start
2. **App Service AdminDisabled** — API returns 403; restore test blocked until subscription/app restored
3. **Live SQL audit not run** — use `tools/database-audit/` when read-only access available

---

## Related Documents

- [`azure-sql-audit.md`](./azure-sql-audit.md)
- [`production-dependencies.md`](./production-dependencies.md)
- [`../architecture/ADR-001-supabase-migration.md`](../architecture/ADR-001-supabase-migration.md)
