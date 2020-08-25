using Microsoft.EntityFrameworkCore;

namespace ShortDash.Server.Data
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<Dashboard> Dashboards { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

    }
}
