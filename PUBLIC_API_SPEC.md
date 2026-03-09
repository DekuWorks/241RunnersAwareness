# Public API Contract — Cases & Missing Map

This document defines the **public** (unauthenticated) API contract for the 241 Runners Awareness frontend. The backend must implement these endpoints and DTOs. No sensitive data may be exposed.

---

## Phase 0 — PublicCaseDto (safe fields only)

Backend MUST return **only** these fields for public endpoints. All other fields are **excluded** for privacy.

### PublicCaseDto shape

| Field | Type | Notes |
|-------|------|--------|
| `id` | string (GUID) or int | Case identifier |
| `publicDisplayName` | string | e.g. first name + last initial, or "Runner #123" — **never** full name if sensitive |
| `ageRange` | string (optional) | e.g. "25-35" — OR `age` only if policy allows |
| `age` | number (optional) | Only if allowed by policy; otherwise use ageRange |
| `status` | string | One of: `Missing`, `Found`, `Safe`, `Urgent`, `Resolved` (and any backend enum values) |
| `lastSeenCity` | string | City only — **no street address** |
| `lastSeenState` | string | State/region |
| `lastSeenAt` | string (ISO date, optional) | When last seen |
| `photoUrl` | string (optional) | Public photo URL |
| `descriptionShort` | string (optional) | Short description; **limit length** (e.g. 200–500 chars) |
| `latitude` | number (optional) | **Fuzzed/rounded** recommended — e.g. 3 decimals (~110 m) |
| `longitude` | number (optional) | Same as latitude |
| `updatedAt` | string (ISO date) | Last update time |

### Explicitly EXCLUDE (never in PublicCaseDto)

- Full address / street address
- Phone numbers
- Email
- Emergency contacts
- Medical info
- Internal / admin notes
- Private identifiers (SSN, etc.)

### Map-specific DTO (Missing map)

For `GET /api/public/map/missing`, a minimal shape is acceptable:

- `id`, `publicDisplayName` (or `displayName`), `status`, `latitude`, `longitude`, `photoUrl`, `lastSeenCity`, `lastSeenState`, `updatedAt`

Coordinates should be **fuzzed** (e.g. rounded to 3 decimals) with a note in UI: "Map location is approximate."

---

## Phase 1 — Backend endpoints (no auth)

### 1. Public cases

- **GET** `/api/public/cases`
  - Returns: array of **PublicCaseDto** (or paged `{ items: PublicCaseDto[], totalCount: number }`).
  - Query params (all optional):
    - `status` — e.g. `?status=Missing`
    - `q` — search by name/city
    - `page`, `pageSize` — pagination
    - `sort` — e.g. `newest`, `updated`, `status`
  - **No** `Authorization` header required.
  - CORS must allow frontend origin (Netlify + localhost).

- **GET** `/api/public/cases/{id}`
  - Returns: single **PublicCaseDto**.
  - 404 if not found.

### 2. Public missing map

- **GET** `/api/public/map/missing`
  - Returns: **only** cases where `status == Missing` **and** have coordinates.
  - Response: array of objects with at least: `id`, `displayName` or `publicDisplayName`, `status`, `lat`/`latitude`, `lng`/`longitude`, `photoUrl`, `lastSeenCity`, `lastSeenState`, `updatedAt`.

### 3. Implementation notes (backend)

- Use **EF Core projection**: `.Select(c => new PublicCaseDto { ... })` so sensitive columns are never returned.
- Do **not** return full entities; always map to DTOs.
- Optional: `visibility` or `isPublic` field — public endpoints return only records where `visibility == Public`.
- **City/state**: Parse `Location` / `LastSeenLocation` into `lastSeenCity` and `lastSeenState` (e.g. "Houston, TX" → city "Houston", state "TX"). No street address.
- **Age range**: Prefer exposing `ageRange` (e.g. "0-12", "13-17", "18-25", "26-40", "41-65", "65+") for privacy; exact `age` is optional.
- **Legacy**: `GET /api/v1.0/cases/publiccases` is deprecated (response headers `X-Deprecated`, `Deprecation`, `Link` with successor `/api/public/cases`). Use `/api/public/cases` instead.

---

## Phase 4 — Security checklist

- [ ] All public endpoints use DTO projection only (no raw entity serialization).
- [ ] No `.Include()` of sensitive relations that end up in the response.
- [ ] Coordinate fuzzing (e.g. round to 3 decimals) for map if exact location is sensitive.
- [ ] CORS allows frontend; public routes do not require `[Authorize]`.
- [ ] Rate limiting (per IP) and short response caching (e.g. 30–120 s) recommended.

---

## Frontend usage

- **Cases page**: `GET /api/public/cases` with optional `status`, `q`, `page`, `pageSize`, `sort`.
- **Case detail page**: `GET /api/public/cases/{id}`.
- **Missing map page**: `GET /api/public/map/missing` — only Missing cases with coordinates; no auth.

If the backend still exposes the legacy `publiccases` or `publiccases/stats/houston` endpoints, the frontend can fall back until the new public API is deployed (see `publicCasesApi.js`).
