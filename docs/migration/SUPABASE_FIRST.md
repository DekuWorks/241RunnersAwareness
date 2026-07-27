# Supabase-First Operating Mode

**Effective:** 2026-07-27  
**Azure migration:** Paused — Azure docs and tooling preserved for rollback only.

## Current stack

| Layer | Provider | Notes |
|-------|----------|-------|
| Database | **Supabase PostgreSQL** | Project `hylwpwauxlnrqvbkmcln` |
| File storage | **Supabase Storage** (`images` bucket) | `SupabaseImageStorageService` |
| API compute | **Render** (Docker) | `https://two41runners-api.onrender.com` |
| Auth | **API JWT** (unchanged) | Supabase Auth deferred to Phase 2 |
| Static site | GitHub Pages | Loads `config.json` → Render API |
| Mobile | Expo / `241RA-mobile` | `EXPO_PUBLIC_API_URL` → Render API |

**Removed hosting options:** Fly.io, Railway (not used).

## Local development

```bash
# 1. Copy and fill .env (see .env.example)
cp .env.example .env

# 2. Start API against Supabase
bash scripts/run-api-supabase.sh

# 3. Mobile (separate repo)
cd ../241RA-mobile
# EXPO_PUBLIC_API_URL=http://127.0.0.1:5051 in .env
npx expo run:ios
```

## Environment variables

| Variable | Default | Purpose |
|----------|---------|---------|
| `DATABASE_PROVIDER` | `Postgres` | Use Supabase DB |
| `STORAGE_PROVIDER` | `Supabase` | Use Supabase Storage for images |
| `ConnectionStrings__DefaultConnection` | pooler URL | See `.env.example` |
| `SUPABASE_URL` | project URL | Storage + future client features |
| `SUPABASE_SERVICE_ROLE_KEY` | server only | Image uploads |
| `PUBLIC_API_BASE_URL` | `http://localhost:5051` | Image serve URLs |

Use the **Supavisor pooler** host (`aws-0-us-east-1.pooler.supabase.com`) to avoid IPv6 connection issues on macOS.

## Migrations

```bash
supabase link --project-ref hylwpwauxlnrqvbkmcln
bash scripts/supabase-push-migrations.sh
```

Schema lives in `supabase/migrations/`.

## What stays on Azure (paused)

- Azure SQL production data (rollback source)
- Azure Blob `images` container
- App Service `241runners-api-v2` (disabled — subscription issue)
- Migration runbooks in `docs/migration/`

## Production

Live site authenticates via Render API → Supabase PostgreSQL. See [`docs/deployment/PRODUCTION_API.md`](../deployment/PRODUCTION_API.md).

## Seeded accounts (fresh Supabase DB)

Admin users are created on first API boot via `DbInitializer` (password `marcus2025` for `dekuworks1@gmail.com`, etc.). See `241RunnersAPI/Data/DbInitializer.cs`.
