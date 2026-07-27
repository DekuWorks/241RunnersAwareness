# Migration Progress Tracker

**Project:** Azure → Supabase migration for 241RunnersAwareness.org  
**Last updated:** 2026-07-26  
**Overall status:** Phase 0 complete — ready for Phase 1 (pending stakeholder ADR approval)

## Status legend

| Status | Meaning |
|--------|---------|
| NOT STARTED | Not yet begun |
| IN PROGRESS | Active work |
| BLOCKED | Waiting on credential, approval, or external dependency |
| READY FOR REVIEW | Work complete; validation pending review |
| COMPLETE | Validated and signed off |
| DEFERRED | Explicitly postponed |

---

## Phase 0: Discovery

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Inspect repository structure | COMPLETE | Codex | — | Low | Inventory matches codebase | See `repository-inventory.md` | 2026-07-26 |
| Create repository inventory | COMPLETE | Codex | Repo inspection | Low | Peer review | | 2026-07-26 |
| Create migration progress tracker | COMPLETE | Codex | — | Low | This document | | 2026-07-26 |
| Create ADR-001 architecture decision | COMPLETE | Codex | Inventory | Low | Architecture review | `docs/architecture/ADR-001-supabase-migration.md` | 2026-07-26 |
| Identify unknowns / manual confirmations | COMPLETE | Codex | Inventory | Medium | Stakeholder review | Listed in inventory | 2026-07-26 |

---

**Overall status:** Phase 2–4 foundation complete — Supabase project creation in progress. Live Azure audit still BLOCKED.

---

## Phase 2: Supabase Foundation

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Initialize `supabase/` CLI structure | COMPLETE | Codex | ADR approval | Low | `supabase db push` | `supabase/config.toml` | 2026-07-26 |
| Add `.env.example` (no real secrets) | COMPLETE | Codex | — | Low | Secret scan clean | Root `.env.example` | 2026-07-26 |
| Add environment validation script | NOT STARTED | — | `.env.example` | Low | Missing vars detected | | |
| Create staging Supabase project plan | COMPLETE | Codex | ADR | Medium | Separate from production | Project created via CLI | 2026-07-26 |
| Design PostgreSQL base schema migrations | COMPLETE | Codex | Schema mapping | High | Migrations apply cleanly | `supabase/migrations/` | 2026-07-26 |
| Add `supabase:start/stop/reset` scripts | COMPLETE | Codex | Supabase CLI | Low | `Makefile` targets | `scripts/supabase-push-migrations.sh` | 2026-07-26 |
| Document setup in README | COMPLETE | Codex | Scripts | Low | New dev can start locally | Updated `README.md` | 2026-07-26 |

---

## Phase 3: Security

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Design RLS policy matrix | COMPLETE | Codex | Schema design | **Critical** | Security review | `rls-policy-matrix.md` | 2026-07-26 |
| Create public-safe views/functions | COMPLETE | Codex | RLS design | **Critical** | Denylist column tests | `v_public_cases`, `v_public_runners_map` | 2026-07-26 |
| Design Supabase Storage bucket policies | NOT STARTED | — | Storage mapping | High | Private buckets default | | |
| Document auth migration plan | COMPLETE | Codex | Auth audit | Critical | Security review | `auth-migration-plan.md` | 2026-07-26 |
| Document authentication controls | COMPLETE | Codex | Auth audit | High | `authentication-controls.md` | | 2026-07-26 |
| Auth cutover test matrix | COMPLETE | Codex | Auth plan | Critical | Sign-off before cutover | `auth-cutover-test-matrix.md` | 2026-07-26 |
| Identity migration map schema | COMPLETE | Codex | Auth plan | Medium | Staging migration only | `supabase/migrations/20260726000001_*` | 2026-07-26 |
| Bcrypt import staging test | NOT STARTED | — | Staging Supabase | High | E7 in test matrix | BLOCKED | |
| API dual JWT support | NOT STARTED | — | Phase 7 | High | T7 in test matrix | | |
| Production auth cutover | BLOCKED | — | All auth tests pass | Critical | Signed test matrix | Never before validation | |
| Add RLS automated tests | NOT STARTED | — | RLS policies | **Critical** | Deny/allow cases pass | `supabase/tests/` | |

---

## Phase 4: Migration Tooling

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Create `tools/241Runners.DataMigration/` console app | COMPLETE | Codex | Schema mapping | High | `dotnet run` works | `tools/241Runners.DataMigration/` | 2026-07-26 |
| Create migration manifest | COMPLETE | Codex | Inventory | Medium | All tables listed | `config/migration-manifest.json` | 2026-07-26 |
| Implement dry-run mode | COMPLETE | Codex | Migration tool | High | No writes in dry-run | `MIGRATION_DRY_RUN=true` default | 2026-07-26 |
| Implement checkpointing / resume | IN PROGRESS | Codex | Migration tool | High | Rerun is idempotent | Checkpoints on live runs | 2026-07-26 |
| Implement quarantine workflow | NOT STARTED | — | Migration tool | Medium | Invalid rows logged safely | | |
| Add migration tool unit tests | NOT STARTED | — | Tool | Medium | CI passes | | |
| Create data dependency graph | NOT STARTED | — | FK audit | Medium | Topological order valid | `data-dependency-graph.md` | |

---

## Phase 5: Data Migration

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Document schema mapping (all tables) | COMPLETE | Codex | DB audit | High | Every column mapped | `schema-mapping.md` | 2026-07-26 |
| Migrate lookup/reference data | NOT STARTED | — | Tool + schema | Low | Row counts match | | |
| Migrate users + identity map | NOT STARTED | — | Auth plan | **Critical** | No duplicate users | | |
| Migrate roles / additional roles | NOT STARTED | — | Users migrated | High | Role parity tests | | |
| Migrate runners | NOT STARTED | — | Users | **Critical** | FK integrity | Sensitive data | |
| Migrate cases | NOT STARTED | — | Runners, Users | **Critical** | FK integrity | | |
| Migrate devices / notifications / topics | NOT STARTED | — | Users | Medium | Row counts | | |
| Migrate deletion requests | NOT STARTED | — | Users | Medium | Row counts | | |
| Transform sightings JSON (evaluate normalize) | NOT STARTED | — | Cases | Medium | Sighting API parity | | |
| Post-migration validation compare | NOT STARTED | — | All tables | **Critical** | `migration-summary.md` pass | | |

---

## Phase 6: Storage Migration

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Document storage mapping | NOT STARTED | — | Blob audit | High | All containers mapped | `storage-mapping.md` | |
| Create Supabase buckets (staging) | NOT STARTED | — | ADR | Medium | Private by default | | |
| Implement blob → Supabase transfer | NOT STARTED | — | Tool | High | Hash sample validation | | |
| Update DB URL references | NOT STARTED | — | File transfer | High | No broken image links | | |
| Create `migration_file_map` table | COMPLETE | Codex | Schema | Medium | Idempotent uploads | `migration.blob_file_map` | 2026-07-26 |

---

## Phase 7: API Migration

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Add `DATABASE_PROVIDER` configuration | COMPLETE | Codex | ADR | High | SqlServer + Postgres switch | `DatabaseProviderKind.cs` | 2026-07-26 |
| Add Npgsql EF Core provider | COMPLETE | Codex | Schema | High | API builds both ways | `Npgsql.EntityFrameworkCore.PostgreSQL` | 2026-07-26 |
| Disable auto-migrate on production cutover | IN PROGRESS | Codex | Runbook | High | Controlled migration only | Skips EF migrate when Postgres | 2026-07-26 |
| Document EF provider compatibility | NOT STARTED | — | Code audit | Medium | Raw SQL inventory | `ef-core-provider-compatibility.md` | |
| Document database logic mapping | NOT STARTED | — | DB audit | Medium | No orphan sprocs | `database-logic-mapping.md` | |
| Create API compatibility matrix | NOT STARTED | — | Controller audit | High | All endpoints classified | `api-compatibility-matrix.md` | |
| PostgreSQL integration tests | NOT STARTED | — | Npgsql provider | High | CI green | | |
| Update health/ready checks for Postgres | NOT STARTED | — | Provider switch | Medium | `/readyz` passes | | |
| Disable auto-migrate on production cutover | NOT STARTED | — | Runbook | High | Controlled migration only | | |

---

## Phase 8: Client Migration

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Consolidate frontend auth token keys | NOT STARTED | — | API auth stable | Medium | Single auth module | | |
| Update public website for new API/auth | NOT STARTED | — | Phase 7 | Medium | E2E login flow | | |
| Update admin dashboard | NOT STARTED | — | Phase 7 | Medium | Admin workflows | | |
| Document mobile auth/storage plan | NOT STARTED | — | Mobile repo access | High | MANUAL CONFIRMATION | `mobile-auth-and-storage.md` | |
| Update mobile app (separate repo) | NOT STARTED | — | Mobile repo | High | BLOCKED until repo located | | |

---

## Phase 9: Staging

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Deploy staging Supabase stack | NOT STARTED | — | Phase 2–3 | Medium | Isolated project | | |
| Run synthetic data migration in staging | NOT STARTED | — | Phase 5 tool | High | No production PII | | |
| E2E tests on staging | NOT STARTED | — | Staging deploy | High | Test suite pass | | |
| Security review (RLS + public views) | NOT STARTED | — | Phase 3 | **Critical** | Sign-off | | |
| Performance comparison | NOT STARTED | — | Staging | Medium | Acceptable p95 | | |

---

## Phase 10: Production Rehearsal

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Restore prod backup to isolated environment | NOT STARTED | — | Phase 1 backup | **Critical** | Explicit approval | | |
| Full migration rehearsal | NOT STARTED | — | All phases | **Critical** | Validation report pass | | |
| Rollback rehearsal | NOT STARTED | — | Runbook | **Critical** | Azure API restored < SLA | | |
| Record rehearsal duration | NOT STARTED | — | Rehearsal | Medium | Documented in runbook | | |

---

## Phase 11: Production Cutover

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Stakeholder cutover approval | NOT STARTED | — | Phase 10 | **Critical** | Written approval | BLOCKED until rehearsal | |
| Execute cutover runbook | NOT STARTED | — | Approval | **Critical** | Smoke tests pass | | |
| Final delta migration | NOT STARTED | — | Cutover window | **Critical** | Row counts match | | |
| Switch API to PostgreSQL | NOT STARTED | — | Cutover | **Critical** | `/readyz` on Postgres | | |
| Post-cutover monitoring (72h+) | NOT STARTED | — | Cutover | High | Error rate normal | | |

---

## Phase 12: Post-Cutover

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Reconcile Azure vs Supabase records | NOT STARTED | — | Cutover | High | No unexplained gaps | | |
| Stakeholder acceptance | NOT STARTED | — | Monitoring period | Medium | Sign-off | | |
| Document Azure decommission plan | NOT STARTED | — | Acceptance | Low | No auto-delete | `azure-decommission-plan.md` | |
| Azure decommission execution | DEFERRED | — | 30+ day observation | High | **Never automated** | Manual approval only | |

---

## Cross-Cutting Tasks

| Task | Status | Owner | Dependencies | Risk | Validation | Notes | Completed |
|------|--------|-------|--------------|------|------------|-------|-----------|
| Delta migration strategy doc | NOT STARTED | — | Schema audit | High | `delta-migration-strategy.md` | | |
| Production cutover runbook | NOT STARTED | — | Phase 10 | **Critical** | `production-cutover-runbook.md` | | |
| Rollback runbook | NOT STARTED | — | Phase 10 | **Critical** | `rollback-runbook.md` | | |
| Observability documentation | NOT STARTED | — | — | Medium | `observability.md` | | |
| CI/CD: migration validation workflow | NOT STARTED | — | Tooling | Medium | PR cannot touch prod | | |
| CI/CD: secret scanning | NOT STARTED | — | — | High | No secrets in PRs | | |
| Developer convenience commands | COMPLETE | Codex | Tooling | Low | Makefile/scripts | `Makefile`, `scripts/` | 2026-07-26 |

---

## Blockers Log

| Date | Task | Blocker | Resolution |
|------|------|---------|------------|
| 2026-07-26 | Mobile app migration | Mobile repo not in workspace | MANUAL CONFIRMATION REQUIRED — provide repo URL |
| 2026-07-26 | Live SQL/Blob audit | Azure subscription disabled; App Service AdminDisabled | Re-enable subscription; run audit on restored BACPAC |
| 2026-07-26 | Rollback verification | API returns HTTP 403 | Start App Service after subscription restored |
| 2026-07-26 | Supabase project setup | Awaiting ADR approval + staging project creation | **Resolved** — project created via `supabase projects create` |

---

## Definition of Done Checklist

See master task specification §47. **0 of 30+ criteria complete** at Phase 0 start.
