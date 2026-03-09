# API configuration (241RunnersAPI)

All frontend API calls use **241RunnersAPI** (this project). The base URL is controlled in one place:

- **`/config.json`** in the site root: set `API_BASE_URL` (e.g. `https://241runners-api-v2.azurewebsites.net/api`).
- **`/assets/js/config.js`** loads `config.json` and sets `window.APP_CONFIG.API_BASE_URL`.

When you deploy 241RunnersAPI elsewhere, update `config.json` (or the deployed `config.json`) so `API_BASE_URL` points to your API base (including `/api`). No other file changes are required.

## Routes (241RunnersAPI)

- Auth: `api/v1.0/auth` (login, register, health, etc.)
- Cases: `api/v1.0/cases` (list, get, publiccases, admin)
- Runner: `api/v1.0/runner`
- Users: `api/v1.0/users`
- ImageUpload: `api/ImageUpload`
- Notifications: `api/notifications`
- Map: `api/map`

Frontend builds URLs as `API_BASE_URL + '/v1.0/...'` or `API_BASE_URL + '/ImageUpload/...'`.
