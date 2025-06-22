using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace _241RunnersAwareness.BackendAPI.Data
{
    public class RunnersDbContextFactory : IDesignTimeDbContextFactory<RunnersDbContext>
    {
        public RunnersDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // base folder
                .AddJsonFile("appsettings.json")              // load appsettings
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<RunnersDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new RunnersDbContext(optionsBuilder.Options);
        }
    }
}
