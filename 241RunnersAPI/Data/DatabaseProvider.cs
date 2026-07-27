using Microsoft.EntityFrameworkCore;

namespace _241RunnersAPI.Data
{
    public static class DbContextConfiguration
    {
        public static void ConfigureProvider(DbContextOptionsBuilder options, string connectionString)
        {
            if (DatabaseProviderKind.IsPostgres)
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(3);
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                });
            }
            else
            {
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.EnableRetryOnFailure(3);
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                });
            }
        }
    }
}
