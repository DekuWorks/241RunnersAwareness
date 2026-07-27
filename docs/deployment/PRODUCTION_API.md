# Production API deployment

**Stack:** Supabase (PostgreSQL + Storage) + Render (ASP.NET Core API host)

**Live API:** `https://two41runners-api.onrender.com`

The API is a thin compute layer — all data and files live in Supabase. Render only runs the Docker container with Supabase env vars.

## Supabase (data layer)

| Service | Project |
|---------|---------|
| PostgreSQL | `hylwpwauxlnrqvbkmcln` |
| Storage | `images` bucket |
| Dashboard | https://supabase.com/dashboard/project/hylwpwauxlnrqvbkmcln |

Required env vars on the API host:

| Variable | Source |
|----------|--------|
| `DATABASE_PROVIDER` | `Postgres` |
| `STORAGE_PROVIDER` | `Supabase` |
| `ConnectionStrings__DefaultConnection` | Supabase pooler URL (Dashboard → Database → Connection string) |
| `SUPABASE_URL` | `https://hylwpwauxlnrqvbkmcln.supabase.co` |
| `SUPABASE_SERVICE_ROLE_KEY` | Dashboard → API → service_role (server only) |
| `JWT_KEY` | 32+ char secret |
| `JWT_ISSUER` / `JWT_AUDIENCE` | `241RunnersAwareness` |
| `PUBLIC_API_BASE_URL` | `https://two41runners-api.onrender.com` |

Use the **Supavisor pooler** host (`aws-0-us-east-1.pooler.supabase.com`) for IPv4 compatibility.

## Render (API host)

**Dashboard:** https://dashboard.render.com/web/srv-d9jb6l7avr4c73c6bcrg

Deploy from `render.yaml` at repo root (auto-deploys on push to `main`).

One-click setup: [Deploy to Render](https://render.com/deploy?repo=https://github.com/DekuWorks/241RunnersAwareness)

> **Note:** Render free tier spins down after ~15 min idle. Upgrade to Starter ($7/mo) for always-on production, or use an uptime pinger on `/health`.

## Verify

```bash
curl https://two41runners-api.onrender.com/health
API_BASE=https://two41runners-api.onrender.com bash scripts/smoke-test-api.sh
```

`config.json` points at this host — GitHub Pages loads it at runtime.

## Local development

```bash
cp .env.example .env   # fill Supabase credentials
bash scripts/run-api-supabase.sh
```

See also: [`docs/migration/SUPABASE_FIRST.md`](../migration/SUPABASE_FIRST.md)
