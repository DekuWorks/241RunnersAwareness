-- Identity migration mapping for Azure Users.Id ↔ Supabase Auth UUID
-- Apply in STAGING first. Restrict access via RLS (service role only).
-- Does not modify production until explicit cutover approval.

CREATE SCHEMA IF NOT EXISTS migration;

CREATE TABLE IF NOT EXISTS migration.identity_migration_map (
    source_user_id          INTEGER PRIMARY KEY,
    supabase_user_id        UUID UNIQUE,
    source_provider         TEXT NOT NULL DEFAULT 'email',
    email_normalized        TEXT NOT NULL,
    migration_status        TEXT NOT NULL DEFAULT 'pending'
        CHECK (migration_status IN (
            'pending',
            'imported',
            'password_reset_required',
            'linked',
            'failed',
            'skipped'
        )),
    requires_password_reset BOOLEAN NOT NULL DEFAULT FALSE,
    migrated_at             TIMESTAMPTZ,
    failure_reason          TEXT,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_identity_migration_map_email
    ON migration.identity_migration_map (email_normalized);

CREATE INDEX IF NOT EXISTS idx_identity_migration_map_status
    ON migration.identity_migration_map (migration_status);

CREATE INDEX IF NOT EXISTS idx_identity_migration_map_supabase_id
    ON migration.identity_migration_map (supabase_user_id)
    WHERE supabase_user_id IS NOT NULL;

COMMENT ON TABLE migration.identity_migration_map IS
    'Maps legacy Users.Id to Supabase auth.users.id. No PII beyond normalized email.';

-- Deny public access; migration tooling uses service role
ALTER TABLE migration.identity_migration_map ENABLE ROW LEVEL SECURITY;

CREATE POLICY identity_migration_map_deny_all ON migration.identity_migration_map
    FOR ALL
    USING (FALSE);

-- Application users table will be migrated separately in schema mapping phase.
-- Users.Id INTEGER must remain the FK target for Runners, Cases, Devices, etc.
