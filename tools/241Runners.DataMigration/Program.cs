using System.Text.Json;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace _241Runners.DataMigration;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var dryRun = GetEnvBool("MIGRATION_DRY_RUN", defaultValue: true);
        var azureConn = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING");
        var postgresConn = Environment.GetEnvironmentVariable("SUPABASE_DB_URL")
            ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(azureConn) || string.IsNullOrWhiteSpace(postgresConn))
        {
            Console.Error.WriteLine("Set AZURE_SQL_CONNECTION_STRING and SUPABASE_DB_URL before running.");
            return 1;
        }

        var manifestPath = Environment.GetEnvironmentVariable("MIGRATION_MANIFEST_PATH")
            ?? "config/migration-manifest.json";
        if (!File.Exists(manifestPath))
        {
            Console.Error.WriteLine($"Manifest not found: {manifestPath}");
            return 1;
        }

        var manifest = JsonSerializer.Deserialize<MigrationManifest>(
            await File.ReadAllTextAsync(manifestPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (manifest?.Tables == null || manifest.Tables.Count == 0)
        {
            Console.Error.WriteLine("Manifest has no tables.");
            return 1;
        }

        Console.WriteLine($"241 Runners data migration — dryRun={dryRun}");
        Console.WriteLine("Source: Azure SQL | Target: Supabase PostgreSQL");
        Console.WriteLine("WARNING: Run against STAGING only. Do not touch production without approval.");

        await using var pg = new NpgsqlConnection(postgresConn);
        await pg.OpenAsync();

        Guid runId;
        if (!dryRun)
        {
            runId = await CreateRunAsync(pg, manifest.Version);
        }
        else
        {
            runId = Guid.Empty;
            Console.WriteLine("[dry-run] Skipping migration.runs insert");
        }

        await using var sql = new SqlConnection(azureConn);
        await sql.OpenAsync();

        foreach (var table in manifest.Tables.OrderBy(t => t.Order))
        {
            Console.WriteLine($"--- {table.Name} ---");
            try
            {
                var copied = await CopyTableAsync(sql, pg, table, dryRun);
                Console.WriteLine($"  rows: {copied}");

                if (!dryRun)
                {
                    await UpsertCheckpointAsync(pg, runId, table.Name, copied, "completed");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  FAILED: {ex.Message}");
                if (!dryRun)
                {
                    await UpsertCheckpointAsync(pg, runId, table.Name, 0, "failed", ex.Message);
                    await MarkRunFailedAsync(pg, runId, ex.Message);
                }
                return 2;
            }
        }

        if (!dryRun)
        {
            await MarkRunCompletedAsync(pg, runId);
        }

        Console.WriteLine("Migration finished successfully.");
        return 0;
    }

    private static async Task<long> CopyTableAsync(
        SqlConnection sql,
        NpgsqlConnection pg,
        TableSpec table,
        bool dryRun)
    {
        var columns = await GetSqlColumnsAsync(sql, table.Name);
        if (columns.Count == 0)
        {
            throw new InvalidOperationException($"No columns found for {table.Name}");
        }

        var quotedCols = columns.Select(c => $"\"{c}\"").ToList();
        var selectSql = $"SELECT {string.Join(", ", columns.Select(c => $"[{c}]"))} FROM [{table.Name}] ORDER BY [{table.PrimaryKey}]";
        var insertSql = $"INSERT INTO \"{table.Name}\" ({string.Join(", ", quotedCols)}) VALUES ({string.Join(", ", columns.Select((_, i) => $"@p{i}"))})";

        long count = 0;
        await using var selectCmd = new SqlCommand(selectSql, sql);
        await using var reader = await selectCmd.ExecuteReaderAsync();

        await using var tx = dryRun ? null : await pg.BeginTransactionAsync();

        while (await reader.ReadAsync())
        {
            if (dryRun)
            {
                count++;
                continue;
            }

            await using var insertCmd = new NpgsqlCommand(insertSql, pg, tx);
            for (var i = 0; i < columns.Count; i++)
            {
                var value = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                insertCmd.Parameters.AddWithValue($"p{i}", value);
            }

            await insertCmd.ExecuteNonQueryAsync();
            count++;
        }

        if (tx != null)
        {
            await tx.CommitAsync();
        }

        if (!dryRun && table.IdentityInsert && count > 0)
        {
            await using var identityCmd = new NpgsqlCommand(
                $"""
                SELECT setval(
                    pg_get_serial_sequence('"{table.Name}"', '{table.PrimaryKey}'),
                    COALESCE((SELECT MAX("{table.PrimaryKey}") FROM "{table.Name}"), 1),
                    true)
                """,
                pg);
            await identityCmd.ExecuteNonQueryAsync();
        }

        return count;
    }

    private static async Task<List<string>> GetSqlColumnsAsync(SqlConnection sql, string tableName)
    {
        const string query = """
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @table
            ORDER BY ORDINAL_POSITION
            """;

        var columns = new List<string>();
        await using var cmd = new SqlCommand(query, sql);
        cmd.Parameters.AddWithValue("@table", tableName);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }

    private static async Task<Guid> CreateRunAsync(NpgsqlConnection pg, string? version)
    {
        const string sql = """
            INSERT INTO migration.runs (manifest_version, status)
            VALUES (@v, 'running')
            RETURNING id
            """;
        await using var cmd = new NpgsqlCommand(sql, pg);
        cmd.Parameters.AddWithValue("v", (object?)version ?? DBNull.Value);
        var id = (Guid)(await cmd.ExecuteScalarAsync() ?? throw new InvalidOperationException("No run id"));
        return id;
    }

    private static async Task UpsertCheckpointAsync(
        NpgsqlConnection pg,
        Guid runId,
        string tableName,
        long rows,
        string status,
        string? error = null)
    {
        const string sql = """
            INSERT INTO migration.checkpoints (run_id, table_name, rows_copied, status, error_message)
            VALUES (@run, @table, @rows, @status, @err)
            ON CONFLICT (run_id, table_name) DO UPDATE
            SET rows_copied = EXCLUDED.rows_copied,
                status = EXCLUDED.status,
                error_message = EXCLUDED.error_message,
                updated_at = NOW()
            """;
        await using var cmd = new NpgsqlCommand(sql, pg);
        cmd.Parameters.AddWithValue("run", runId);
        cmd.Parameters.AddWithValue("table", tableName);
        cmd.Parameters.AddWithValue("rows", rows);
        cmd.Parameters.AddWithValue("status", status);
        cmd.Parameters.AddWithValue("err", (object?)error ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task MarkRunCompletedAsync(NpgsqlConnection pg, Guid runId)
    {
        await using var cmd = new NpgsqlCommand(
            "UPDATE migration.runs SET status = 'completed', completed_at = NOW() WHERE id = @id", pg);
        cmd.Parameters.AddWithValue("id", runId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task MarkRunFailedAsync(NpgsqlConnection pg, Guid runId, string message)
    {
        await using var cmd = new NpgsqlCommand(
            "UPDATE migration.runs SET status = 'failed', completed_at = NOW(), notes = @notes WHERE id = @id", pg);
        cmd.Parameters.AddWithValue("id", runId);
        cmd.Parameters.AddWithValue("notes", message);
        await cmd.ExecuteNonQueryAsync();
    }

    private static bool GetEnvBool(string name, bool defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(raw)
            ? defaultValue
            : bool.TryParse(raw, out var parsed) && parsed;
    }
}

internal sealed class MigrationManifest
{
    public string? Version { get; set; }
    public List<TableSpec> Tables { get; set; } = new();
}

internal sealed class TableSpec
{
    public string Name { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = "Id";
    public int Order { get; set; }
    public bool IdentityInsert { get; set; }
    public List<string>? DependsOn { get; set; }
}
