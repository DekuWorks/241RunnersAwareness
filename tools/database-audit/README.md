# Azure SQL Database Audit (Read-Only)

Sanitized, read-only discovery scripts for the 241RunnersAwareness Azure SQL database. **No sensitive values are selected** — only schema metadata, counts, and statistical summaries.

## Prerequisites

- Read-only SQL access to production or a **restored BACPAC copy** (preferred for audit runs)
- One of:
  - [sqlcmd](https://learn.microsoft.com/sql/tools/sqlcmd-utility) (v18+)
  - Azure Data Studio
  - `az sql db query` (read-only login)

## Safety

- Use a **read-only** database user; never run against production with write credentials unless exporting backups
- Do **not** redirect output to the repository — store reports in a secure location outside git
- Scripts never `SELECT` email, phone, address, medical text, or password fields

## Quick start

```bash
# Set connection (never commit this value)
export AZURE_SQL_SERVER="241runners-sql-2025.database.windows.net"
export AZURE_SQL_DATABASE="241RunnersAwarenessDB"   # MANUAL CONFIRMATION REQUIRED if name differs
export AZURE_SQL_USER="readonly_audit"
export AZURE_SQL_PASSWORD="..."                      # use Key Vault or prompt

cd tools/database-audit
./run-audit.sh
```

Output is written to `artifacts/database-audit/<timestamp>/` (gitignored).

## Scripts

| Script | Purpose |
|--------|---------|
| `sql/01-schema-inventory.sql` | Tables, columns, types, keys |
| `sql/02-row-counts.sql` | Row counts per table |
| `sql/03-data-quality.sql` | Null rates, duplicates (sanitized) |
| `sql/04-foreign-keys.sql` | FK relationships and orphan counts |
| `sql/05-indexes.sql` | Index inventory |
| `sql/06-sensitive-column-inventory.sql` | PII column flags (no values) |
| `sql/07-ef-migrations.sql` | Applied EF migration history |

## Azure Blob inventory

```bash
./inventory-azure-blob.sh
```

Requires `az login` with read access to storage account `241runnersstorage`.

## Report

After running, paste summarized results into `docs/migration/azure-sql-audit.md` (sanitized sections only).
