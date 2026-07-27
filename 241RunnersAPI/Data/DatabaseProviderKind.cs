namespace _241RunnersAPI.Data
{
    /// <summary>
    /// Database provider selection for dual Azure SQL / Supabase PostgreSQL support.
    /// Set DATABASE_PROVIDER=SqlServer (default) or Postgres.
    /// </summary>
    public static class DatabaseProviderKind
    {
        public const string SqlServer = "SqlServer";
        public const string Postgres = "Postgres";

        public static string Current =>
            Environment.GetEnvironmentVariable("DATABASE_PROVIDER")?.Trim() ?? SqlServer;

        public static bool IsPostgres =>
            string.Equals(Current, Postgres, StringComparison.OrdinalIgnoreCase);

        public static bool IsSqlServer => !IsPostgres;

        public static string UtcNowSql =>
            IsPostgres ? "timezone('utc', now())" : "GETUTCDATE()";

        public static string FilterColumnNotNull(string columnName) =>
            IsPostgres
                ? $"\"{columnName}\" IS NOT NULL"
                : $"[{columnName}] IS NOT NULL";
    }
}
