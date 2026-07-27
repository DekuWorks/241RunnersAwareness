# Supabase-First Operating Mode

**Effective:** 2026-07-27  
**Azure migration:** Paused — all Azure docs, tooling, and resources are preserved for later cutover.

## Current stack

| Layer | Primary | Notes |
|-------|---------|-------|
| Database | **Supabase PostgreSQL** | Project `hylwpwauxlnrqvbkmcln` |
| File storage | **Supabase Storage** (`images` bucket) | Via `SupabaseImageStorageService` |
| API | **ASP.NET Core 8** (local or future host) | `DATABASE_PROVIDER=Postgres`, `STORAGE_PROVIDER=Supabase` |
| Auth | **API JWT** (unchanged) | Supabase Auth deferred |
| Static site | GitHub Pages | Still points to Azure API until new API host is deployed |
| Mobile | Expo / `241RA-mobile` | Point `EXPO_PUBLIC_API_URL` at running API |

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

## What stays on Azure (paused)

- Azure SQL production data (rollback source)
- Azure Blob `images` container
- App Service `241runners-api-v2` (disabled — subscription issue)
- All migration runbooks in `docs/migration/`

## Production site gap

`config.json` still references `241runners-api-v2.azurewebsites.net`. The live site will not authenticate until the API is deployed to a new host (Fly.io, Render, etc.) with Supabase env vars. Local API + simulator work today.

## Seeded accounts (fresh Supabase DB)

Admin users are created on first API boot via `DbInitializer` (password `marcus2025` for `dekuworks1@gmail.com`, etc.). See `241RunnersAPI/Data/DbInitializer.cs`.
