# Azure SQL Audit Report

**Generated:** 2026-07-26  
**Source:** Entity Framework model + repository inspection + Azure resource metadata  
**Live database audit:** **BLOCKED** — Azure subscription disabled; App Service AdminDisabled. Run `tools/database-audit/run-audit.sh` against a restored BACPAC when access is restored.

---

## Executive Summary

The application uses **8 application tables** on Azure SQL Server (`241runners-sql-2025`), managed by EF Core migrations under `241RunnersAPI/Migrations/SqlServer/`. Primary keys are **integer identity** columns. No stored procedures are invoked by the application code. Sightings are stored as **JSON text** in `Cases.AdditionalInformation`.

This document contains **sanitized schema and statistical placeholders** only. Populate the "Live audit results" sections after running read-only scripts.

---

## Server and Database

| Property | Value |
|----------|-------|
| SQL Server | `241runners-sql-2025.database.windows.net` |
| Resource group | `241runnersawareness-rg` |
| Region | `centralus` |
| Database name | `241RunnersAwarenessDB` (MANUAL CONFIRMATION REQUIRED) |
| EF provider | SQL Server 8.0.11 |
| Migration count (repo) | 14 EF migrations + 1 manual SQL script |

---

## Schemas

| Schema | Tables |
|--------|--------|
| `dbo` | 8 application tables + `__EFMigrationsHistory` |

---

## Tables (from EF Core model)

| Table | PK | Sensitive data | RLS required (Supabase) |
|-------|-----|----------------|-------------------------|
| `Users` | `Id` int | **Yes** — email, phone, address, password hash, OAuth tokens | Yes |
| `Runners` | `Id` int | **Yes** — medical, location, emergency, photos | Yes |
| `Cases` | `Id` int | **Yes** — descriptions, locations, JSON sightings | Yes |
| `Devices` | `Id` int | **Yes** — FCM tokens | Yes |
| `TopicSubscriptions` | `Id` int | Low — topic names | Yes |
| `Notifications` | `Id` int | Medium — message body | Yes |
| `DataDeletionRequests` | `Id` int | Medium — user requests | Yes |
| `AccountDeletionRequests` | `Id` int | Medium — email in `UserEmail` | Yes |

---

## Foreign Key Relationships (application model)

```
Users (1) ──< Runners (UserId) CASCADE
Users (1) ──< Devices (UserId)
Users (1) ──< TopicSubscriptions (UserId)
Users (1) ──< Notifications (UserId)
Users (1) ──< DataDeletionRequests (UserId)
Users (1) ──< AccountDeletionRequests (UserId)
Runners (1) ──< Cases (RunnerId)
Users (1) ──< Cases (ReportedByUserId)
```

Additional columns without FK constraints in model:

- `Runner.CreatedByUserId` — int, no FK in snapshot (MANUAL CONFIRMATION REQUIRED in live DB)
- `Case.CreatedByUserId` — int

---

## Indexes (from EF migrations)

Notable indexes (see `05-indexes.sql` output for live list):

| Table | Index | Type |
|-------|-------|------|
| `Users` | `Email` | Unique |
| `Users` | `EmailVerificationToken`, `PasswordResetToken` | Unique filtered |
| `Runners` | `UserId`, `Status` | Non-unique |
| `Devices` | `(UserId, Platform)` | Unique |
| `TopicSubscriptions` | `(UserId, Topic)` | Unique |
| `Cases` | Performance indexes from migration `AddPerformanceIndexes` | Various |

---

## Row Counts

### Live audit results

> Run `tools/database-audit/sql/02-row-counts.sql` and paste counts here. **Do not commit raw audit output files.**

| Table | Row count | Audited (UTC) |
|-------|-----------|---------------|
| `Users` | _pending_ | |
| `Runners` | _pending_ | |
| `Cases` | _pending_ | |
| `Devices` | _pending_ | |
| `TopicSubscriptions` | _pending_ | |
| `Notifications` | _pending_ | |
| `DataDeletionRequests` | _pending_ | |
| `AccountDeletionRequests` | _pending_ | |
| `__EFMigrationsHistory` | _pending_ | |

---

## Data Quality Summary

### Live audit results

> Run `tools/database-audit/sql/03-data-quality.sql`

| Metric | Value | Notes |
|--------|-------|-------|
| Users by `Role` | _pending_ | Expected: user, parent, caregiver, therapist, adoptiveparent, admin |
| Runners by `Status` | _pending_ | Missing, Found, Resolved |
| Public cases (`IsPublic=1`) | _pending_ | |
| Cases with JSON sightings | _pending_ | `AdditionalInformation` length > 2 |
| Orphan FK counts | _pending_ | From `04-foreign-keys.sql` |
| Invalid email format count | _pending_ | Pattern-based only |
| Duplicate emails | _pending_ | Should be 0 (unique index) |

---

## Sensitive Column Inventory

Columns flagged for migration handling (no values):

### Users

`Email`, `PasswordHash`, `PhoneNumber`, `Address`, `City`, `State`, `ZipCode`, `ProviderAccessToken`, `ProviderRefreshToken`, `EmailVerificationToken`, `PasswordResetToken`, `ResetToken`, `EmergencyContactName`, `EmergencyContactPhone`, `Notes`, `ProfileImageUrl`, etc.

### Runners

`Name`, `PhysicalDescription`, `MedicalConditions`, `Medications`, `Allergies`, `EmergencyInstructions`, `LastKnownLocation`, `MapLatitude`, `MapLongitude`, `ProfileImageUrl`, `AdditionalImageUrls`, `AdditionalNotes`, etc.

### Cases

`Description`, `Location`, `LastSeenLocation`, `AdditionalInformation` (JSON sightings), `CaseImageUrls`, `DocumentUrls`, `LastSeenLatitude`, `LastSeenLongitude`, etc.

### Devices

`FcmToken`, `TopicsJson`

Full list: run `06-sensitive-column-inventory.sql`

---

## EF Migration History (expected from repo)

| MigrationId | Description |
|-------------|-------------|
| `20250908143434_InitialCreate` | Initial |
| `20250908173941_AddRunnerAndCaseModels` | Runner + Case |
| `20250908173952_FixDecimalPrecision` | Decimal fix |
| `20250913231044_AddResetTokenColumns` | Reset tokens |
| `20250913232725_AddPerformanceIndexes` | Indexes |
| `20250919173347_AddPushNotificationsAndTopics` | Notifications |
| `20250927154654_AddOAuthSupport` | OAuth fields |
| `20251005031030_AddRunnerStatusField` | Runner status |
| `20251005031204_AddRunnerCreatedByUserIdField` | CreatedBy |
| `20251007234420_AddEnhancedRunnerFields` | Enhanced runner |
| `20251007234507_AddRunnerHeightWeightEyeColorFields` | Physical attrs |
| `20251013225243_AddDataDeletionRequests` | GDPR requests |
| `20251014231512_AddAdditionalRolesToUser` | Additional roles JSON |
| `20260309192215_AddRunnerShowOnMapAndCoordinates` | Map coordinates |

Live applied migrations: _pending_ (run `07-ef-migrations.sql`)

---

## Database Logic Outside EF

| Object type | Count in app code | Action |
|-------------|-------------------|--------|
| Stored procedures | 0 called | None to migrate |
| Triggers | Unknown | Audit live DB |
| Views | 0 in app | Create public views in Supabase |
| Raw SQL | Minimal | Review `ef-core-provider-compatibility.md` (Phase 7) |

Manual SQL script in repo: `Migrations/SqlServer/ApplyRunnerMapColumns.sql`

---

## File / Blob URL Columns

| Table | Column | Storage backend |
|-------|--------|-----------------|
| `Users` | `ProfileImageUrl`, `AdditionalImageUrls`, `DocumentUrls` | Azure Blob URLs |
| `Runners` | `ProfileImageUrl`, `AdditionalImageUrls` | Azure Blob (`images` container) |
| `Cases` | `CaseImageUrls`, `DocumentUrls` | Azure Blob |

---

## Known Risks from Schema Review

1. **JSON sightings** in `Cases.AdditionalInformation` — not relational; migration must preserve JSON or normalize
2. **OAuth users** with null `PasswordHash` — auth migration must branch
3. **Map coordinates** on runners and cases — public exposure risk during migration
4. **No audit log table** — historical admin actions may be lost if only in logs
5. **Integer user IDs** vs future Supabase Auth UUIDs — requires `identity_migration_map`

---

## Next Steps

1. Re-enable Azure subscription and App Service
2. Export BACPAC per `azure-backup-and-recovery.md`
3. Run `tools/database-audit/run-audit.sh` on **restored copy**
4. Populate pending sections in this document (counts only, no PII)
5. Proceed to Phase 2 (Supabase local foundation)

---

## Audit Tool Reference

```bash
cd tools/database-audit
export AZURE_SQL_SERVER="241runners-sql-2025.database.windows.net"
export AZURE_SQL_DATABASE="241RunnersAwarenessDB"
export AZURE_SQL_USER="readonly_audit"
export AZURE_SQL_PASSWORD="..."
./run-audit.sh
```
