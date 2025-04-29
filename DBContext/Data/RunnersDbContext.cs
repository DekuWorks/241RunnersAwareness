using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.Models;

namespace _241RunnersAwareness.BackendAPI.Data
{
    public class RunnersDbContext : DbContext
    {
        public RunnersDbContext(DbContextOptions<RunnersDbContext> options) : base(options)
        {
        }

        // These match your table names
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    }
    // IF _contex was null, this is where it would blow up(meaning it didn't register it or the it isn't recieving it properly)
}
