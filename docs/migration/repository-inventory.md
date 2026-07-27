# Repository Inventory — 241RunnersAwareness.org

**Generated:** 2026-07-26  
**Phase:** 0 (Discovery)  
**Status:** Based on repository inspection; production row counts and live Azure configuration require manual confirmation.

---

## Executive Summary

241RunnersAwareness is a **static HTML/JavaScript public site** plus **ASP.NET Core 8 Web API** backed by **Azure SQL Server** and **Azure Blob Storage**, deployed to **GitHub Pages** (frontend) and **Azure App Service** (API). There is **no React Native/Expo mobile app in this repository** (backend CORS and token key names indicate a separate mobile client). There is **no Supabase** configuration in the repo today.

The platform handles **missing-runner cases**, **runner profiles** (including medical and location data), **user accounts** with multiple roles, **push notifications** (Firebase), and **real-time updates** (SignalR).

---

## Applications

| Application | Location | Technology | Deployment Target |
|-------------|----------|------------|-------------------|
| Public website | Repository root (`*.html`, `js/`, `assets/`) | Static HTML/CSS/JS | GitHub Pages → `www.241runnersawareness.org` |
| Admin dashboard | `admin/` | Static HTML/JS (monolithic `admindash.html`) | GitHub Pages (same site) |
| Backend API | `241RunnersAPI/` | ASP.NET Core 8, EF Core 8 | Azure App Service `241runners-api-v2.azurewebsites.net` |
| Mobile app | **Not in repo** | Inferred: React Native/Expo (separate repo) | MANUAL CONFIRMATION REQUIRED |
| Solution file | `241RunnersAwareness.sln` | .NET solution | References `241RunnersAPI` only |

---

## Backend Projects

| Project | Path | Purpose |
|---------|------|---------|
| `241RunnersAPI` | `241RunnersAPI/241RunnersAPI.csproj` | Main Web API, EF Core, SignalR hubs, Azure Blob integration |

**No separate test projects** are present in the solution.

---

## Frontend Projects

| Component | Path | Notes |
|-----------|------|-------|
| Public pages | `index.html`, `cases.html`, `map.html`, `login.html`, etc. | 29+ HTML pages |
| Shared JS | `js/`, `assets/js/` | Multiple overlapping API clients (`api.js`, `apiClient.js`, `auth-utils.js`, `publicCasesApi.js`) |
| Admin | `admin/admindash.html`, `admin/users.html`, `admin/login.html` | ~13k-line admin dashboard |
| Config | `config.json` | `API_BASE_URL` points to Azure API |
| Partials | `partials/header.html` | Shared header fragment |

**No npm/package.json** — no frontend build pipeline.

---

## Shared Libraries

None. All backend code is in `241RunnersAPI/`.

---

## Database

### Provider

| Setting | Value |
|---------|-------|
| ORM | Entity Framework Core 8 |
| Provider | `Microsoft.EntityFrameworkCore.SqlServer` |
| Configuration | `241RunnersAPI/Data/DatabaseProvider.cs` → `UseSqlServer()` |
| Connection | `ConnectionStrings:DefaultConnection` or env `ConnectionStrings__DefaultConnection` |
| Migrations | `241RunnersAPI/Migrations/SqlServer/` (14 EF migrations + 1 manual SQL script) |
| Startup | `Program.cs` calls `MigrateAsync()` + `DbInitializer.Initialize()` on boot |

### DbContext

**`ApplicationDbContext`** (`241RunnersAPI/Data/ApplicationDbContext.cs`)

| DbSet | Entity | PK Type |
|-------|--------|---------|
| `Users` | `User` | `int` identity |
| `Runners` | `Runner` | `int` identity |
| `Cases` | `Case` | `int` identity |
| `Devices` | `Device` | `int` identity |
| `TopicSubscriptions` | `TopicSubscription` | `int` identity |
| `Notifications` | `Notification` | `int` identity |
| `DataDeletionRequests` | `DataDeletionRequest` | `int` identity |
| `AccountDeletionRequests` | `AccountDeletionRequest` | `int` identity |

### Entities NOT mapped to database

| Class | File | Notes |
|-------|------|-------|
| `EnhancedRunner` | `Models/EnhancedRunner.cs` | Rich DTO; API uses `Runner` entity |
| Auth DTOs | `Models/AuthRequests.cs`, `Models/User.cs` | Request/response shapes |

### Domain concepts absent as separate tables

| Expected concept (from spec) | Actual implementation |
|------------------------------|---------------------|
| `runner_emergency_contacts` | **Not a table.** `Runner.EmergencyInstructions`; user `PhoneNumber`, `Address` on `User` |
| `guardians`, `caregivers` | **Not tables.** Represented as `User.Role` values (`parent`, `caregiver`, `therapist`, `adoptiveparent`) |
| `sightings` | **Not a table.** JSON array stored in `Case.AdditionalInformation` |
| `case_status_history` | **Not a table.** `Case.Status`, `UpdatedAt`, `ResolvedAt` only |
| `audit_logs` | **Not a persistent table.** `SecurityAuditService` exists but is **not registered in DI** |
| `refresh_tokens` | **Not a table.** JWT issued inline in `AuthController`; no refresh token store |
| `user_roles` | **Not normalized.** `User.Role` + `User.AdditionalRoles` (JSON string) |
| `runner_photos` | **Not a table.** `Runner.ProfileImageUrl`, `AdditionalImageUrls` (URLs pointing to Azure Blob) |

### Roles (application-level)

Validated in `User.Role` regex: `user`, `parent`, `caregiver`, `therapist`, `adoptiveparent`, `admin`.  
`AdditionalRoles` holds JSON array for dual-role users (e.g., admin + parent).

---

## Storage Systems

| System | Usage | Code locations |
|--------|-------|----------------|
| **Azure Blob Storage** | Runner/case images | `Services/BlobImageStorageService.cs`, `Controllers/ImageUploadController.cs`, `Program.cs` (conditional `BlobServiceClient` DI) |
| Config key | `ConnectionStrings:AzureStorageConnectionString` or `AzureStorageConnectionString` | |
| Container | `images` (default in `BlobImageStorageService`) | |
| Access | Upload via API; SAS token generation in `ImageUploadController` | |

**No local file storage** for production uploads.

---

## Authentication Systems

| Component | Implementation |
|-----------|----------------|
| Primary auth | Custom JWT (HMAC-SHA256) in `Program.cs` + `AuthController` |
| Password hashing | BCrypt (`BCrypt.Net-Next`) |
| ASP.NET Identity | **Not used** |
| OAuth | Custom `OAuthController` — Google, Apple, Microsoft via manual token verification |
| Google OAuth | `VerifyGoogleToken()` calls Google userinfo API; fields on `User`: `AuthProvider`, `ProviderUserId`, tokens |
| Email verification | `IsEmailVerified`, `EmailVerificationToken` on `User` |
| Phone verification | `IsPhoneVerified`, `PhoneNumber` on `User` — **no Twilio integration in code** |
| MFA / TOTP | **Not implemented** in codebase |
| Account lockout | `FailedLoginAttempts`, `LockedUntil` on `User` |
| Token storage (clients) | Browser `localStorage` (fragmented keys: `ra_auth`, `jwtToken`, `ra_admin_token`, etc.) |

### Services present but not wired in DI

- `JwtService`, `SecureTokenService`, `SecurityAuditService` — exist, not registered in `Program.cs`

---

## External Integrations

| Integration | Status | Location |
|-------------|--------|----------|
| Azure SQL | **Production DB** | EF Core |
| Azure Blob Storage | **Production file storage** | `Azure.Storage.Blobs` |
| Azure App Service | **API hosting** | `api-deploy.yml` |
| Firebase Cloud Messaging | **Push notifications** | `FirebaseNotificationService.cs`, env `FIREBASE_SERVICE_ACCOUNT_JSON` |
| Google OAuth | **Implemented** (API-side) | `OAuthController.cs` |
| Google Maps | **Frontend** | `config.json` `EXPO_PUBLIC_GOOGLE_MAPS_API_KEY` placeholder |
| Application Insights | **Optional telemetry** | `appsettings.Staging.json`, env `APPLICATIONINSIGHTS_CONNECTION_STRING` |
| SendGrid | **Not implemented** — stub `EmailService` logs only | `PhotoUpdateNotificationService.cs` |
| Twilio | **Not implemented** — mentioned in privacy policy and admin mock UI | |
| SignalR | **Real-time** | Hubs: `NotificationHub`, `AlertsHub`, `UserHub`, `AdminHub` |
| Supabase | **Not present** | — |

---

## API Surface (Controllers)

18 controllers + minimal APIs in `Program.cs`. Key route groups:

| Route prefix | Controller | Auth |
|--------------|------------|------|
| `api/v1/auth` | `AuthController`, `OAuthController` | Mixed |
| `api/v1/Admin` | `AdminController` | Admin |
| `api/v1/cases` | `CasesController` | Mixed |
| `api/v1/Runner` | `RunnerController` | Mixed |
| `api/public/cases` | `PublicCasesController` | Anonymous (public-safe DTOs) |
| `api/map` | `MapController` | Mixed |
| `api/ImageUpload` | `ImageUploadController` | Authenticated |
| `api/notifications`, `api/Devices`, `api/Topics` | Various | Authenticated |

Contract reference: `PUBLIC_API_SPEC.md`

Health endpoints: `/healthz`, `/readyz`, `/health`, `/health/ready`, `/health/live`, plus controller-level health actions.

---

## CI/CD Workflows

| Workflow | File | Status | Purpose |
|----------|------|--------|---------|
| Deploy to GitHub Pages | `.github/workflows/pages.yml` | Active | Static site on `main` push |
| API Deploy | `.github/workflows/api-deploy.yml` | Active | Build/publish API → Azure (`AZURE_CREDENTIALS` OIDC) |
| Restore Azure API | `.github/workflows/azure-restore.yml` | Manual | Start app, apply settings, trigger deploy |
| Static Site Deploy | `.github/workflows/static-deploy.yml` | Disabled | Legacy |
| Deploy (peaceiris) | `.github/workflows/deploy.yml` | Disabled | Stale (references npm) |

### GitHub Secrets (names only — no values)

`AZURE_SQL_CONNECTION_STRING`, `AZURE_WEBAPP_PUBLISH_PROFILE`, `AZURE_WEBAPP_NAME`, `JWT_KEY`, `AZURE_CREDENTIALS`, legacy `AZUREAPPSERVICE_*` OIDC secrets.

---

## Sensitive Data Locations

| Data type | Storage | Exposure risk |
|-----------|---------|---------------|
| User email, phone, address | `Users` table | Authenticated API; admin endpoints |
| Password hashes | `Users.PasswordHash` (BCrypt) | Server only |
| OAuth tokens | `Users.ProviderAccessToken`, `ProviderRefreshToken` | Server only |
| Medical conditions, medications, allergies | `Runners` columns | Runner profile API; **must not appear in public endpoints** |
| Emergency instructions | `Runners.EmergencyInstructions` | Runner API |
| Exact map coordinates | `Runners.MapLatitude/Longitude`, `Cases.LastSeenLatitude/Longitude` | Map API has public and raw endpoints — **review `MapController`** |
| Case descriptions, locations | `Cases` | Public cases filtered by `IsPublic` + DTO projection |
| Sightings (reporter notes) | `Cases.AdditionalInformation` JSON | Case sighting endpoints |
| Profile/case images | Azure Blob URLs in DB | Public if URL known |
| Deletion requests | `DataDeletionRequests`, `AccountDeletionRequests` | Admin/user scoped |
| FCM device tokens | `Devices.FcmToken` | User scoped |

---

## Middleware and Security

| Component | File |
|-----------|------|
| Rate limiting | `Middleware/RateLimitingMiddleware.cs`, `IpRateLimiting` in appsettings |
| CSRF | `Middleware/CsrfProtectionMiddleware.cs` |
| Performance monitoring | `Middleware/PerformanceMonitoringMiddleware.cs` |
| Input sanitization | `Services/InputSanitizationService.cs` |
| IP validation | `Services/IpValidationService.cs` |
| CORS | `Program.cs` — allows production domain, localhost, Expo dev URLs |

---

## Scripts and Tooling

| Script | Path | Purpose |
|--------|------|---------|
| `kudu_deploy.py` | `scripts/kudu_deploy.py` | Kudu configure/deploy (SCM basic auth often disabled on App Service) |
| `restore-azure-api.sh` | `scripts/restore-azure-api.sh` | Local Azure CLI restore |

**No migration console app, no Supabase CLI, no test projects.**

---

## Known Risks

1. **Single DbContext, SQL Server–specific migrations** — PostgreSQL migration requires schema translation and provider abstraction.
2. **Integer PKs** — Not UUIDs; preservation is straightforward but differs from Supabase Auth UUID users.
3. **Sightings in JSON column** — Harder to query, migrate, and secure with RLS; may need normalization in Postgres.
4. **No dedicated audit log table** — Compliance/forensics gap; migration opportunity.
5. **Fragmented frontend auth keys** — Client migration complexity.
6. **BCrypt passwords incompatible with Supabase Auth import** — Password reset flow required for auth migration.
7. **Azure Blob URLs embedded in DB** — Storage migration must update all URL references atomically.
8. **Public map endpoints** — Must verify coordinate precision before any direct Supabase exposure.
9. **`JwtService` / `SecurityAuditService` not registered** — Dead code or incomplete wiring.
10. **Auto-migrate on API startup** — Risky for production Postgres cutover; needs controlled migration process.
11. **No automated tests** — Migration validation burden is high.
12. **Mobile app in separate repo** — MANUAL CONFIRMATION REQUIRED for mobile migration scope.

---

## Unknowns Requiring Manual Confirmation

| Item | Action needed |
|------|---------------|
| Production Azure SQL row counts and size | Run read-only audit (Phase 1) |
| Azure Blob container names, object count, total size | Azure Portal or CLI inventory |
| Whether SCM/Kudu basic auth is disabled on App Service | Confirmed likely disabled; deploy uses `az webapp deploy` |
| Mobile app repository URL and release status | Locate separate Expo/RN repo |
| Whether Twilio/SendGrid are used outside codebase | Check Azure app settings / vendor accounts |
| Current Supabase project (if any exists from prior work) | Confirm org projects; do not attach to production without approval |
| DNS full configuration (apex + www) | `CNAME` → `www.241runnersawareness.org` |
| Production backup/restore last verified date | Phase 1 backup doc |
| Legal/consent requirements for data migration | Stakeholder review |

---

## File Reference Index

```
241RunnersAwareness/
├── 241RunnersAPI/           # ASP.NET Core 8 API
├── admin/                   # Admin dashboard (static)
├── assets/, js/             # Public frontend assets
├── config.json              # API base URL
├── CNAME                    # www.241runnersawareness.org
├── .github/workflows/       # CI/CD
├── scripts/                 # Deploy helpers
├── PUBLIC_API_SPEC.md       # Public API contract
└── docs/migration/          # Migration documentation (this effort)
```
