# Production API deployment

**Target URL:** `https://241runners-api.onrender.com`

Hosting blockers encountered during setup:
- **Fly.io** — free trial ended (add payment method at [fly.io](https://fly.io))
- **Azure** — subscription read-only/disabled (`241runners-api-v2` stopped)

## Recommended: Render (free tier)

1. Sign in at [render.com](https://render.com) and connect the `DekuWorks/241RunnersAwareness` GitHub repo.
2. **New → Blueprint** → select this repo (`render.yaml` is at the root).
3. Set these secret env vars in the Render dashboard when prompted:

| Variable | Source |
|----------|--------|
| `JWT_KEY` | GitHub secret `JWT_KEY` or generate 32+ char string |
| `SUPABASE_URL` | `https://hylwpwauxlnrqvbkmcln.supabase.co` |
| `SUPABASE_SERVICE_ROLE_KEY` | GitHub secret `SUPABASE_SERVICE_ROLE_KEY` |
| `ConnectionStrings__DefaultConnection` | GitHub secret `SUPABASE_CONNECTION_STRING` |

4. After deploy, verify: `curl https://241runners-api.onrender.com/health`
5. `config.json` already points at this host — GitHub Pages will work after Pages rebuild.

One-click (after Render login):  
[Deploy to Render](https://render.com/deploy?repo=https://github.com/DekuWorks/241RunnersAwareness)

## Alternative: Fly.io

```bash
# After adding a card at fly.io
export FLY_API_TOKEN=$(fly auth token)
bash scripts/deploy-api-fly.sh
```

## Smoke test

```bash
API_BASE=https://241runners-api.onrender.com bash scripts/smoke-test-api.sh
```
