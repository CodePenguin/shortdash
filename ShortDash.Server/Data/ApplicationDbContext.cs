using Microsoft.EntityFrameworkCore;

namespace ShortDash.Server.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

    }
}
