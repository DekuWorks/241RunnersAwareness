-- Migration tooling tables for Azure SQL → Supabase data copy (staging only)

CREATE SCHEMA IF NOT EXISTS migration;

CREATE TABLE IF NOT EXISTS migration.runs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    started_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at    TIMESTAMPTZ,
    status          TEXT NOT NULL DEFAULT 'running'
        CHECK (status IN ('running', 'completed', 'failed', 'cancelled')),
    source          TEXT NOT NULL DEFAULT 'azure_sql',
    target          TEXT NOT NULL DEFAULT 'supabase_postgres',
    manifest_version TEXT,
    notes           TEXT
);

CREATE TABLE IF NOT EXISTS migration.checkpoints (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    run_id          UUID NOT NULL REFERENCES migration.runs(id) ON DELETE CASCADE,
    table_name      TEXT NOT NULL,
    last_id         INTEGER,
    rows_copied     BIGINT NOT NULL DEFAULT 0,
    status          TEXT NOT NULL DEFAULT 'pending'
        CHECK (status IN ('pending', 'in_progress', 'completed', 'failed', 'skipped')),
    error_message   TEXT,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (run_id, table_name)
);

CREATE TABLE IF NOT EXISTS migration.failures (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    run_id          UUID NOT NULL REFERENCES migration.runs(id) ON DELETE CASCADE,
    table_name      TEXT NOT NULL,
    source_id       TEXT,
    error_message   TEXT NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS migration.blob_file_map (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_container    TEXT NOT NULL DEFAULT 'images',
    source_blob_name    TEXT NOT NULL,
    target_bucket       TEXT NOT NULL DEFAULT 'images',
    target_path         TEXT NOT NULL,
    migration_status    TEXT NOT NULL DEFAULT 'pending'
        CHECK (migration_status IN ('pending', 'copied', 'failed', 'skipped')),
    migrated_at         TIMESTAMPTZ,
    failure_reason      TEXT,
    UNIQUE (source_container, source_blob_name)
);

ALTER TABLE migration.runs ENABLE ROW LEVEL SECURITY;
ALTER TABLE migration.checkpoints ENABLE ROW LEVEL SECURITY;
ALTER TABLE migration.failures ENABLE ROW LEVEL SECURITY;
ALTER TABLE migration.blob_file_map ENABLE ROW LEVEL SECURITY;

CREATE POLICY migration_runs_deny_all ON migration.runs FOR ALL USING (FALSE);
CREATE POLICY migration_checkpoints_deny_all ON migration.checkpoints FOR ALL USING (FALSE);
CREATE POLICY migration_failures_deny_all ON migration.failures FOR ALL USING (FALSE);
CREATE POLICY migration_blob_file_map_deny_all ON migration.blob_file_map FOR ALL USING (FALSE);
