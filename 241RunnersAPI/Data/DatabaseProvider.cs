using Microsoft.EntityFrameworkCore;

namespace _241RunnersAPI.Data
{
    public static class DbContextConfiguration
    {
        public static void ConfigureProvider(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(3);
            });
        }
    }
}
