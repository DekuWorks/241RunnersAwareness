# Staging Supabase Environment

## Project

| Field | Value |
|-------|-------|
| Name | 241RunnersAwareness |
| Project ref | `tmglqsdxplpdfexavihn` |
| Region | us-east-1 |
| Dashboard | https://supabase.com/dashboard/project/tmglqsdxplpdfexavihn |
| API URL | https://tmglqsdxplpdfexavihn.supabase.co |

Metadata is also in `config/supabase-project.json` (no secrets).

## Link CLI

```bash
# Reset database password in Dashboard → Project Settings → Database first if needed
supabase link --project-ref tmglqsdxplpdfexavihn
supabase db push
```

## Environment variables

Copy `.env.example` to `.env` and set:

- `SUPABASE_PROJECT_REF=tmglqsdxplpdfexavihn`
- `SUPABASE_URL=https://tmglqsdxplpdfexavihn.supabase.co`
- `SUPABASE_DB_URL=postgresql://postgres:YOUR_DB_PASSWORD@db.tmglqsdxplpdfexavihn.supabase.co:5432/postgres`
- `SUPABASE_ANON_KEY` — from Dashboard → API or `supabase projects api-keys`
- `SUPABASE_SERVICE_ROLE_KEY` — server-side only; set in GitHub Actions secrets

## GitHub Actions secrets (recommended)

| Secret | Purpose |
|--------|---------|
| `SUPABASE_PROJECT_REF` | CLI link / deploy |
| `SUPABASE_URL` | API client config |
| `SUPABASE_ANON_KEY` | Public client (still treat as secret in CI) |
| `SUPABASE_SERVICE_ROLE_KEY` | Migration tooling / API server only |
| `SUPABASE_DB_PASSWORD` | `db push` and data migration |

## Storage

Create private bucket `images` in Dashboard → Storage (matches Azure `images` container). Policies: service role write, public read via signed URLs or API proxy until cutover.

## Safety

- This project is **staging** — do not point production traffic here until cutover approval
- Do not copy production PII until Azure subscription is restored and rehearsal is approved
- Azure production remains rollback source per ADR-001
