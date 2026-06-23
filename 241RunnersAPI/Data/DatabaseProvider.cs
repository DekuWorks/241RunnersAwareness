using Microsoft.EntityFrameworkCore;

namespace _241RunnersAPI.Data
{
    public static class DbContextConfiguration
    {
        public static void ConfigureProvider(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(3);
            });
        }
    }
}
