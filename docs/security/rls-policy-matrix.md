# RLS Policy Matrix

## Design principle

Direct Supabase client access (`anon` / `authenticated` JWT) must **not** read or write application tables. All business logic stays in the ASP.NET API, which connects with a privileged database role.

## Application tables

| Table | RLS | anon | authenticated | API (postgres role) |
|-------|-----|------|---------------|---------------------|
| Users | ON | DENY | DENY | FULL (bypasses RLS) |
| Runners | ON | DENY | DENY | FULL |
| Cases | ON | DENY | DENY | FULL |
| Devices | ON | DENY | DENY | FULL |
| TopicSubscriptions | ON | DENY | DENY | FULL |
| Notifications | ON | DENY | DENY | FULL |
| DataDeletionRequests | ON | DENY | DENY | FULL |
| AccountDeletionRequests | ON | DENY | DENY | FULL |

Policies: `*_deny_all` with `USING (FALSE)` on all operations.

## Public views (read-only)

| View | anon SELECT | authenticated SELECT | Exposed fields |
|------|-------------|----------------------|----------------|
| `v_public_cases` | GRANT | GRANT | Approved public cases, no contact PII |
| `v_public_runners_map` | GRANT | GRANT | Map-opted runners, coordinates only |

## Migration tables (`migration` schema)

| Table | RLS | Client access |
|-------|-----|---------------|
| identity_migration_map | ON | DENY (service role only) |
| runs, checkpoints, failures | ON | DENY |
| blob_file_map | ON | DENY |

## Future: Supabase Auth (Phase B)

If/when Supabase Auth replaces API JWT:

1. Add policies keyed on `auth.uid()` via `migration.identity_migration_map`
2. User-owned rows: `UserId` matches mapped legacy ID
3. Admin role via custom JWT claim or `service_role` only
4. Re-run auth cutover test matrix before production

## Verification

```sql
-- As anon (should return 0 rows from Users, rows only from views)
SET ROLE anon;
SELECT COUNT(*) FROM "Users";
SELECT COUNT(*) FROM v_public_cases;
RESET ROLE;
```
