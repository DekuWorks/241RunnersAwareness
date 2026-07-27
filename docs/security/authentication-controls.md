# Authentication Controls

**Last updated:** 2026-07-26  
**Scope:** Current production behavior + target controls during Supabase migration

---

## Current Controls (ASP.NET Core API)

| Control | Implementation | Status |
|---------|----------------|--------|
| Password hashing | BCrypt (`BCrypt.Net-Next`), cost factor 11 | Active |
| Password policy | Min 8 chars, upper, lower, number | Active |
| JWT signing | HMAC-SHA256 symmetric (`JWT_KEY`) | Active |
| JWT validation | Issuer, audience, lifetime, signing key | Active |
| Account disable | `Users.IsActive` checked at login | Active |
| Account lockout fields | `FailedLoginAttempts`, `LockedUntil` | Present — verify enforcement in login path |
| Email verification flag | `IsEmailVerified` | Partial — weak verify endpoint |
| Phone verification flag | `IsPhoneVerified` | No SMS backend |
| OAuth token verification | HTTP calls to Google/Microsoft | Google/Microsoft active; Apple stub |
| Rate limiting | `RateLimitingMiddleware` — auth endpoints throttled | Active |
| Input sanitization | `InputSanitizationService` on register | Active |
| CSRF middleware | `CsrfProtectionMiddleware` | Active |
| Role-based authorization | `[Authorize(Roles=...)]`, ownership checks | Active |
| Admin endpoints | `admin` role required | Active |
| Runner ownership | `Runner.UserId == current user` or admin | Active |
| Logout | Client-side JWT discard | Stateless — no server revocation list |
| Refresh tokens | GUID to client, **not persisted** | Weak — not true refresh |
| MFA / TOTP | — | **Not implemented** |
| Password reset email | Token logged to server log | **Insecure — must fix before cutover** |
| OAuth token storage | Plain text in DB (`EncryptToken` noop) | **Insecure — must fix** |

---

## Target Controls (Migration)

### Phase A — Database only (no Supabase Auth)

| Control | Action |
|---------|--------|
| All current controls | Preserve identical API behavior against PostgreSQL |
| Password hashes | Migrate column as-is |
| JWT | Same signing key rotation policy |
| Audit | Log auth failures without passwords/tokens |

### Phase B — Supabase Auth (optional)

| Control | Action |
|---------|--------|
| Service role key | Migration tooling + API backend only; env/Key Vault |
| Anon key | Client SDK only where required; no elevated DB access |
| Supabase JWT validation | API validates `iss`, `exp`, signature; map UUID → `Users.Id` |
| Roles | Load from `Users` table — **never** trust client metadata |
| RLS | Enabled on sensitive tables if direct client DB access exists |
| Email confirmation | Import only when `IsEmailVerified` evidence exists |
| Password reset | Supabase hosted flow; disable token logging |
| Brute force | Supabase Auth rate limits + API rate limits |
| Session invalidation | Plan JWT key rotation at cutover |

---

## Controls That Must Not Weaken

- Minimum password complexity
- Admin-only administrative routes
- Runner/case ownership checks
- Public API field filtering (no emergency contacts / medical data)
- No service-role key in frontend, mobile, or git
- No production PII in migration logs or test fixtures

---

## Logging Rules (Auth)

**Do not log:**

- Passwords or password hashes
- JWT access/refresh tokens (full strings)
- Password reset tokens
- OAuth access/refresh tokens
- Complete request bodies on auth endpoints

**May log (masked):**

- User id (integer)
- Email: `j***@example.com` format
- Auth event type (login_success, login_failed)
- Provider name (google, email)
- Correlation / request id

---

## Gaps to Close Before Production Auth Cutover

1. Remove password reset token from application logs (`AuthController.ResetPassword`)
2. Implement real email delivery for reset (Supabase SMTP or SendGrid)
3. Guard email/password login when `PasswordHash` is null (OAuth-only users)
4. Implement or disable Apple OAuth (remove stub)
5. Encrypt OAuth provider tokens at rest
6. Align JWT `expiresIn` response with actual token `exp`
7. Include `AdditionalRoles` in JWT or load roles from DB on every authorized request
8. Add server-side refresh token store OR document stateless-only model

---

## Related Documents

- [`auth-migration-plan.md`](../migration/auth-migration-plan.md)
- [`auth-cutover-test-matrix.md`](../migration/auth-cutover-test-matrix.md)
- [`rls-policy-matrix.md`](./rls-policy-matrix.md) (Phase 3)
