using Microsoft.EntityFrameworkCore;

namespace ShortDash.Core.Data
{
    public abstract class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ConfigurationSection> ConfigurationSections { get; set; }
    }
}
