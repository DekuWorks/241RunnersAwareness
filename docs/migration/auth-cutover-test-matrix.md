# Authentication Cutover Test Matrix

**Purpose:** Gate production authentication migration. **All tests must pass in staging** before cutover. Use synthetic accounts only in automated CI; use designated test accounts in staging (never production PII in logs).

**Status:** NOT STARTED — run after Phase 2 staging Supabase + Phase 7 API dual-provider

---

## Test environment requirements

- Staging Supabase project (isolated)
- Staging API pointed at Supabase PostgreSQL
- Email delivery redirected to test inbox / disabled capture
- OAuth callbacks configured for staging URLs only
- `AUTH_ACCEPT_SUPABASE_JWT` flag available for dual-mode testing

---

## 1. Email / password users

| ID | Scenario | Steps | Expected | Pass |
|----|----------|-------|----------|------|
| E1 | Existing user login (Phase A) | Migrate user with bcrypt hash; login via `POST /api/v1/auth/login` | 200, JWT returned, `user.id` unchanged | ☐ |
| E2 | Wrong password | Login with bad password | 401 `INVALID_CREDENTIALS` | ☐ |
| E3 | Disabled account | `IsActive=false` | 401 `ACCOUNT_DISABLED` | ☐ |
| E4 | Change password | `POST change-password` with valid current password | 200; old password fails, new works | ☐ |
| E5 | Password reset flow | Request reset → receive email → set new password | New password works; reset token single-use | ☐ |
| E6 | Email verification | Unverified user restrictions (if any) | Matches pre-migration behavior | ☐ |
| E7 | Bcrypt import (Phase B only) | `signInWithPassword` via Supabase client after hash import | Success without password change | ☐ |
| E8 | Password reset required (Phase B fallback) | User with `password_reset_required` | Invitation email; first login sets password | ☐ |

---

## 2. Google OAuth users

| ID | Scenario | Steps | Expected | Pass |
|----|----------|-------|----------|------|
| G1 | Existing Google user login | `POST /api/v1/auth/oauth/login` with valid Google token | 200, JWT, correct `user.id` | ☐ |
| G2 | Google user not registered | OAuth login without prior `oauth/register` | 400 `requiresRegistration` | ☐ |
| G3 | New Google registration | `oauth/register` then login | User created, `IsEmailVerified=true` | ☐ |
| G4 | Supabase Google (Phase B) | Sign in via Supabase Google provider | Maps to same `source_user_id` | ☐ |
| G5 | Cancelled Google login | Abort OAuth flow | No user created | ☐ |
| G6 | Email case mismatch | `User@Email.com` vs `user@email.com` | Single account, no duplicate | ☐ |

---

## 3. Apple / Microsoft OAuth

| ID | Scenario | Steps | Expected | Pass |
|----|----------|-------|----------|------|
| O1 | Microsoft login | Valid Microsoft token | 200 if user exists | ☐ |
| O2 | Apple login | Valid Apple token | **BLOCKED** until `VerifyAppleToken` implemented | ☐ |
| O3 | Invalid provider token | Tampered token | 401 | ☐ |

---

## 4. Administrators

| ID | Scenario | Steps | Expected | Pass |
|----|----------|-------|----------|------|
| A1 | Admin login | Admin email/password | 200, `role=admin` | ☐ |
| A2 | Admin dashboard | `admin/admindash.html` with token | Loads, admin API calls succeed | ☐ |
| A3 | Admin-only API | `GET /api/v1/Admin/stats` | 200 for admin, 403 for user | ☐ |
| A4 | Dual-role admin (admin + parent) | User with `AdditionalRoles` | Admin routes + parent runner access | ☐ |
| A5 | Non-admin denied | User token on admin route | 403 | ☐ |
| A6 | Role elevation attempt | User tries to set `role=admin` via API | Rejected | ☐ |

---

## 5. Caregivers, parents, general users

| ID | Scenario | Steps | Expected | Pass |
|----|----------|-------|----------|------|
| R1 | Parent owns runner | `GET /api/v1/Runner/{id}` own runner | 200 with full profile | ☐ |
| R2 | Parent denied other runner | Another user's runner ID | 403/404 | ☐ |
| R3 | Caregiver role login | `role=caregiver` user login | 200, correct role in JWT | ☐ |
| R4 | Therapist role | `role=therapist` | Appropriate case/runner access per app rules | ☐ |
| R5 | General user | `role=user` | Public + own data only | ☐ |
| R6 | My cases | `GET /api/v1/cases/my-cases` | Only own cases | ☐ |

---

## 6. Emergency contacts & sensitive data

| ID | Scenario | Steps | Expected | Pass |
|----|----------|-------|----------|------|
| S1 | Emergency contact on own profile | Authenticated owner `GET /api/v1/auth/me` or profile | Fields visible to owner | ☐ |
| S2 | Other user emergency contact | User A requests User B emergency fields | Denied | ☐ |
| S3 | Public case API | `GET /api/public/cases` | No emergency/medical/private address fields | ☐ |
| S4 | Public map | `GET /api/map/points` | No exact private coordinates beyond policy | ☐ |
| S5 | Admin emergency access | Admin views runner detail | Allowed per admin policy | ☐ |

---

## 7. Tokens & security

| ID | Scenario | Steps | Expected | Pass |
|----|----------|-------|----------|------|
| T1 | Expired JWT | Use token after `exp` | 401 | ☐ |
| T2 | Tampered JWT | Modify payload | 401 | ☐ |
| T3 | SignalR auth | Connect to `/hubs/admin` with admin token | Connected |
| T4 | SignalR denied | User token to admin hub | Denied | ☐ |
| T5 | Service role absent from web | Build frontend/mobile bundles | No `service_role` string in artifacts | ☐ |
| T6 | Logout | `POST logout` + discard token | Subsequent requests 401 | ☐ |
| T7 | Supabase JWT mapping (Phase B) | API accepts Supabase token | Resolves to correct `Users.Id` | ☐ |

---

## 8. Mobile (MANUAL CONFIRMATION REQUIRED — separate repo)

| ID | Scenario | Expected | Pass |
|----|----------|----------|------|
| M1 | iOS email login | Same as E1 | ☐ |
| M2 | Android email login | Same as E1 | ☐ |
| M3 | Mobile Google OAuth | Same as G1/G4 | ☐ |
| M4 | Token secure storage | Token in secure storage, not plain logs | ☐ |
| M5 | Deep link password reset | Staging reset link opens app | ☐ |

---

## Sign-off

| Role | Name | Date | All critical tests pass |
|------|------|------|-------------------------|
| Engineering | | | ☐ |
| Security | | | ☐ |
| Product | | | ☐ |

**Critical tests:** E1, E5, G1 (if OAuth used), A1–A3, R1–R2, S1–S4, T5, T7 (if Phase B)

**Production auth cutover is BLOCKED until this matrix is signed off.**
