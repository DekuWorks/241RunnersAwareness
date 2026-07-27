# Azure → Supabase Migration

This directory contains planning, runbooks, and validation artifacts for migrating 241RunnersAwareness.org from Azure SQL / Azure Blob Storage to Supabase PostgreSQL / Supabase Storage.

## Status

**Phases 0–4 foundation complete.** Supabase staging project created; schema migrations ready to push. Live Azure data migration blocked until subscription is re-enabled.

Production Azure resources are **not modified** during implementation.

## Quick start (staging)

```bash
# 1. Copy environment template
cp .env.example .env
# Edit .env with Supabase credentials (never commit .env)

# 2. Link CLI to Supabase project (after project creation)
supabase link --project-ref YOUR_PROJECT_REF

# 3. Push schema migrations
make supabase-push

# 4. Dry-run data migration (no writes)
make migrate-dry
```

## Key documents

| Document | Purpose |
|----------|---------|
| [repository-inventory.md](./repository-inventory.md) | What exists in the repo today |
| [MIGRATION_PROGRESS.md](./MIGRATION_PROGRESS.md) | Task tracker with status |
| [schema-mapping.md](./schema-mapping.md) | Azure SQL ↔ PostgreSQL column mapping |
| [azure-backup-and-recovery.md](./azure-backup-and-recovery.md) | BACPAC, blob, config backup procedures |
| [azure-sql-audit.md](./azure-sql-audit.md) | Sanitized schema audit (live counts pending) |
| [production-dependencies.md](./production-dependencies.md) | Production service dependency map |
| [auth-migration-plan.md](./auth-migration-plan.md) | Preserve sign-in strategy (phased hybrid) |
| [auth-cutover-test-matrix.md](./auth-cutover-test-matrix.md) | Pre-cutover auth test gate |
| [../security/rls-policy-matrix.md](../security/rls-policy-matrix.md) | RLS and public view policies |
| [../security/authentication-controls.md](../security/authentication-controls.md) | Current and target auth controls |
| [../architecture/ADR-001-supabase-migration.md](../architecture/ADR-001-supabase-migration.md) | Architecture decision record |

## Supabase migrations

Located in `supabase/migrations/`:

1. `20260726000000_initial_schema.sql` — 8 application tables
2. `20260726000001_identity_migration_map.sql` — auth identity map
3. `20260726000002_rls_policies.sql` — deny-all RLS on app tables
4. `20260726000003_public_views.sql` — public cases / map views
5. `20260726000004_migration_tooling.sql` — migration run tracking

## Audit tools

```bash
cd tools/database-audit
./run-audit.sh
./inventory-azure-blob.sh
```

## Safety rules

- No production PII in fixtures, logs, or commits
- No secrets in source control
- Azure remains rollback source until explicit cutover approval
- Service-role keys are server-side only
- `MIGRATION_DRY_RUN=true` by default for data migration tool
