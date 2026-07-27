# Authentication Migration Plan

**Project:** 241RunnersAwareness.org — Azure SQL → Supabase  
**Last updated:** 2026-07-26  
**Status:** Proposed — **do not switch production authentication until staging validation passes**

---

## Executive Summary

The platform does **not** use ASP.NET Core Identity. Authentication is implemented as:

- **Custom JWT** (HMAC-SHA256) issued by `AuthController` / `OAuthController`
- **BCrypt** password hashing via `BCrypt.Net-Next` (`$2a$` format, work factor 11)
- **Custom OAuth** verification for Google, Apple, Microsoft (not ASP.NET OAuth middleware)
- **Application roles** stored on `Users.Role` + `Users.AdditionalRoles` (JSON), not a normalized `user_roles` table

### Recommended strategy: **Phased hybrid (minimum disruption)**

| Phase | Auth behavior | User impact |
|-------|---------------|-------------|
| **A — Database cutover** | Keep existing API login + JWT; move `Users` table to Supabase PostgreSQL unchanged | **None** — same email/password, same tokens |
| **B — Supabase Auth (optional)** | Import or invite users; API validates Supabase JWT **or** legacy JWT during transition | Low if bcrypt import works; medium if password reset required |
| **C — Production auth cutover** | Only after all tests in `auth-cutover-test-matrix.md` pass | Requires communication plan |

**Do not enable Supabase Auth in production until Phase C approval.**

---

## Current Authentication Architecture

### Components

| Component | Location | Notes |
|-----------|----------|-------|
| Registration / login | `AuthController.cs` | `POST api/v1/auth/register`, `login` |
| OAuth | `OAuthController.cs` | `POST api/v1/auth/oauth/login`, `oauth/register` |
| JWT config | `Program.cs` | `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE` |
| Token claims | `AuthController.GenerateJwtToken` | `NameIdentifier` (int user id), `Email`, `Role`, `Name` |
| Password hash | `Users.PasswordHash` | BCrypt; nullable for OAuth-only users |
| OAuth fields | `AuthProvider`, `ProviderUserId`, tokens on `User` | Tokens stored **without real encryption** (see risks) |
| Authorization | `[Authorize(Roles = "...")]`, `BaseController.IsAdmin()` | Role from JWT `ClaimTypes.Role` |
| Client storage | `localStorage` | `ra_auth`, `jwtToken`, `ra_admin_token`, etc. |

### JWT claims issued (email/password login)

```csharp
ClaimTypes.NameIdentifier → user.Id (integer string)
ClaimTypes.Email          → user.Email
ClaimTypes.Role           → user.Role  // user|parent|caregiver|therapist|adoptiveparent|admin
ClaimTypes.Name           → "FirstName LastName"
```

**Not in JWT today:** `AdditionalRoles`, `IsAdminUser`, `PrimaryUserRole` (computed in login response body only). `BaseController.GetAllUserRoles()` expects `AllRoles` claim that is **not** set in `GenerateJwtToken` — dual-role admins may be under-authorized in some code paths.

**Token lifetime:** 1 hour (`AuthController`) vs 24 hours stated in login response `expiresAt` — inconsistency to fix before cutover.

### Roles (application)

| Role | Purpose |
|------|---------|
| `user` | General account |
| `parent` | Parent/guardian persona |
| `caregiver` | Caregiver access |
| `therapist` | ABA therapist |
| `adoptiveparent` | Adoptive parent |
| `admin` | Administrative access |

`AdditionalRoles` JSON supports dual roles (e.g., admin + parent). Authorization policies use **primary `Role` column** in most `[Authorize(Roles=...)]` attributes.

### User relationships (authorization, not separate auth identities)

| Relationship | Implementation |
|--------------|----------------|
| Runner ownership | `Runners.UserId` — owner can CRUD own runner; admin override |
| Case reporting | `Cases.ReportedByUserId`, `Cases.RunnerId` |
| Caregiver/guardian | **Role on `Users`**, not a link table |
| Emergency contacts | `Users.EmergencyContact*` and `Runners.EmergencyInstructions` — authorized via runner/user ownership + admin |
| Devices / notifications | `Devices.UserId`, `Notifications.UserId` |

Preserving **`Users.Id` (integer)** through database migration is **required** for all relationship FKs.

---

## Password Hash Compatibility

### Current format

- Library: `BCrypt.Net.BCrypt.HashPassword(password)` (default cost **11**)
- Stored prefix: `$2a$11$...` (confirmed in EF seed migrations)
- Verification: `BCrypt.Net.BCrypt.Verify(plain, hash)`

### Supabase Auth compatibility

| Factor | Assessment |
|--------|------------|
| Algorithm | Both use **bcrypt** — theoretically compatible |
| Prefix `$2a$` | Supported by Supabase GoTrue in many versions |
| Work factor 11 | Within Supabase defaults |
| Import API | `auth.admin.createUser({ email, password_hash, email_confirm })` — server-side only |

### Verdict

**Conditionally compatible** — bcrypt hashes **may** be importable via Supabase Admin API, but this is **not guaranteed** without staging proof.

**Mandatory staging test (before any import decision):**

1. Export 3–5 **synthetic** bcrypt hashes (never production) with same `BCrypt.Net` settings
2. Import into staging Supabase via Admin API
3. Attempt `signInWithPassword` for each
4. Document pass/fail per hash version

### If import fails (fallback)

1. Create Supabase Auth user with `email_confirm: true` only when `Users.IsEmailVerified == true`
2. Set `identity_migration_map.migration_status = 'password_reset_required'`
3. Send **one-time** secure password reset / invitation email via Supabase (staging: disabled redirect)
4. Keep `Users.PasswordHash` in app DB during transition for **API-only** login (Phase A) until user completes reset

**Never decrypt or reverse password hashes.**

---

## OAuth Providers

| Provider | Implementation status | Lookup key | Notes |
|----------|----------------------|------------|-------|
| **Google** | **Functional** — `userinfo` API with bearer token | `Email` + `AuthProvider == 'google'` | Frontend login page is email/password only; mobile may use OAuth |
| **Apple** | **Stub** — placeholder email in `VerifyAppleToken` | Same pattern | Not production-ready |
| **Microsoft** | **Functional** — Graph `/me` | Same pattern | |

### OAuth migration approach

**Phase A (recommended for cutover):** No change — API continues custom OAuth verification and issues same JWT.

**Phase B (Supabase Auth):**

| Provider | Action |
|----------|--------|
| Google | Enable in Supabase Auth; configure web/iOS/Android client IDs (MANUAL CONFIRMATION REQUIRED); link identities by verified email with duplicate detection |
| Apple | Complete real token verification **before** migration; configure Apple provider in Supabase |
| Microsoft | Enable Azure AD provider in Supabase or keep API-side verification |

### Account linking rules

1. Normalize email to lowercase before match
2. If email exists with `AuthProvider = 'email'` and user signs in with Google:
   - **Do not auto-merge** without explicit user consent
   - Queue for manual review or offer "link accounts" flow
3. OAuth login requires **pre-registration** via `oauth/register` today — document for mobile clients

### Duplicate email scenarios

| Scenario | Current behavior | Migration action |
|----------|------------------|------------------|
| Same email, email + Google | OAuth register rejects existing email | Map to single Supabase user with multiple identities |
| Same email, different Google accounts | Unlikely | Block; audit log |
| OAuth user, null `PasswordHash` | Email login calls `BCrypt.Verify` on null — **may throw/fail** | Fix: reject password login when `AuthProvider != email` |

---

## Email Verification & Password Reset

| Feature | Current state | Migration requirement |
|---------|---------------|----------------------|
| Email verification | `IsEmailVerified`, `EmailVerificationToken`; `verify-email` endpoint uses weak token lookup | Map to Supabase `email_confirmed_at` when importing |
| Password reset | Generates `ResetToken` GUID; **logs token** (no email integration) | Implement Supabase reset emails before auth cutover |
| Change password | BCrypt verify + rehash | Same logic or Supabase `updateUser` |
| Phone verification | `IsPhoneVerified` field; **no SMS provider** | Defer or keep app-level flags |
| MFA / TOTP | **Not implemented** | Do not weaken; document gap |
| Account lockout | `FailedLoginAttempts`, `LockedUntil` on `User` | Preserve columns; sync policy to Supabase if using Auth |
| Refresh tokens | Client UUID; **not stored server-side** | Supabase refresh tokens replace client flow when cut over |

---

## Identity Mapping Table

Required for Supabase Auth migration (restricted schema):

```sql
-- See supabase/migrations/20260726000001_identity_migration_map.sql
identity_migration_map (
  source_user_id        INT PRIMARY KEY,      -- preserved Users.Id
  supabase_user_id      UUID UNIQUE,          -- auth.users.id
  source_provider       TEXT,                 -- email, google, apple, microsoft
  email_normalized      TEXT NOT NULL,
  migration_status      TEXT NOT NULL,        -- pending|imported|password_reset_required|linked|failed
  migrated_at           TIMESTAMPTZ,
  failure_reason        TEXT,                 -- sanitized, no PII
  requires_password_reset BOOLEAN DEFAULT false
);
```

**RLS:** deny all except `service_role` and migration tooling.

Application `Users` table retains `Id` as canonical FK for runners, cases, devices.

---

## Chosen Migration Strategy (Detailed)

### Strategy name: **Preserve-then-optional-Auth**

#### Step 1 — Data migration (no auth change)

1. Migrate `Users` table to PostgreSQL with **all columns** including `PasswordHash`, OAuth fields, roles
2. API continues `BCrypt.Verify` + `GenerateJwtToken` against PostgreSQL
3. Create `identity_migration_map` rows with `migration_status = 'pending'`, `supabase_user_id = NULL`
4. **Zero user-facing auth change**

#### Step 2 — Staging Supabase Auth evaluation

1. For each user category (see test matrix), attempt bcrypt import OR invitation flow
2. Record results in `identity_migration_map`
3. Configure Google OAuth in staging Supabase project only

#### Step 3 — Dual validation period (staging)

API accepts:

- **Legacy JWT** (existing signing key) — always during transition
- **Supabase JWT** (optional flag `AUTH_ACCEPT_SUPABASE_JWT=true`) — validate `iss`, `aud`, `exp`, map `sub` UUID → `source_user_id` via `identity_migration_map`

Role resolution for Supabase JWT:

- **Never trust** `user_metadata.role` from client
- Load roles from `Users` table by `source_user_id`

#### Step 4 — Production cutover gate

**All must pass:**

- Email/password users sign in
- Google OAuth users sign in (if used in production)
- Admin dashboard + admin API routes
- Caregiver/parent runner access boundaries
- Emergency contact fields not exposed to unauthorized users
- Password reset email delivered (test inbox)
- No service-role key in client bundles

**Then** flip production auth mode with maintenance window + rollback owner on standby.

#### Step 5 — Session invalidation

On auth cutover:

- Invalidate existing JWTs by rotating `JWT_KEY` **or** shortening overlap window
- Force re-login for all clients (communicate in advance)
- Supabase sessions use new refresh token flow

---

## Role & Permission Migration

### Do not store authoritative roles in Supabase `user_metadata`

| Source | Target |
|--------|--------|
| `Users.Role` | Preserved in `public.users` (or renamed `app_users`) |
| `Users.AdditionalRoles` JSON | Preserved; consider normalize to `user_role_assignments` post-migration |
| Admin privileges | `Role == 'admin'` + `AdminController` authorization |

Optional `user_role_assignments` table (Phase B+):

```text
user_id → users.id
role    → enum text
granted_at, granted_by
```

JWT / API must load roles from database on each request for Supabase Auth users.

---

## What We Preserve

| Asset | Preservation method |
|-------|---------------------|
| All user accounts | Row migration with same `Id` |
| Passwords (if bcrypt import works) | `password_hash` import OR Phase A unchanged hashes in `Users` |
| Roles & additional roles | Column migration |
| Runner/caregiver relationships | FK on `Users.Id` unchanged |
| Admin privileges | `Role` + admin endpoints unchanged |
| OAuth provider linkage | `AuthProvider`, `ProviderUserId` columns + Supabase `identities` when Phase B |
| Email verification state | `IsEmailVerified` → `email_confirm` on import |
| Disabled accounts | `IsActive == false` → block login in API and Supabase ban flag |

---

## Rollback (Auth-Specific)

1. Set `DATABASE_PROVIDER=SqlServer` (if DB was switched)
2. Disable `AUTH_ACCEPT_SUPABASE_JWT`
3. Redeploy API version that only issues legacy JWT
4. Revert frontend/mobile Supabase SDK config
5. Users continue signing in with email/password against Azure `Users` table
6. Preserve `identity_migration_map` and Supabase users for analysis — do not delete

---

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Bcrypt import fails | Phase A keeps API auth; password reset campaign |
| OAuth Apple stub | Do not migrate Apple users until verified |
| OAuth tokens stored in plain text | Rotate tokens post-migration; encrypt at rest in future |
| JWT role claim incomplete for dual-role admins | Fix `GenerateJwtToken` to include `AdditionalRoles` before cutover |
| Email/password login on OAuth-only users | Guard `PasswordHash == null` |
| Password reset logs tokens | Remove log line; integrate real email before cutover |
| Service role key exposure | Server-side migration tool only; never in web/mobile |
| Forced re-login backlash | Phase A avoids; communicate if Phase B |

---

## Manual Confirmations Required

- [ ] Which OAuth providers are used in **production** (mobile vs web)?
- [ ] Google Cloud OAuth client IDs for web, iOS, Android
- [ ] Apple Sign-In production status
- [ ] Email delivery provider for password reset (Supabase SMTP vs SendGrid)
- [ ] Stakeholder approval for forced re-login (if Phase B)
- [ ] Legal/comms for migration email to users

---

## Related Documents

- [`auth-cutover-test-matrix.md`](./auth-cutover-test-matrix.md)
- [`../security/authentication-controls.md`](../security/authentication-controls.md)
- [`../security/rls-policy-matrix.md`](../security/rls-policy-matrix.md) (Phase 3)
- [`../architecture/ADR-001-supabase-migration.md`](../architecture/ADR-001-supabase-migration.md)
- [`config/identity-migration-manifest.json`](../../config/identity-migration-manifest.json)

---

## Implementation Status

| Item | Status |
|------|--------|
| Auth audit | COMPLETE |
| Strategy documented | COMPLETE |
| `identity_migration_map` SQL | COMPLETE (staging schema) |
| Bcrypt staging test | NOT STARTED — blocked on staging Supabase |
| API dual-JWT support | NOT STARTED |
| Production auth cutover | **BLOCKED** until tests pass |
