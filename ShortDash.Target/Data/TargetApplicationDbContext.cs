using Microsoft.EntityFrameworkCore;
using ShortDash.Core.Data;

namespace ShortDash.Target.Data
{
    public class TargetApplicationDbContext : ApplicationDbContext
    {
        public TargetApplicationDbContext(DbContextOptions<TargetApplicationDbContext> options) : base(options)
        {
        }
    }
}
