using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Drawing;

namespace ShortDash.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ConfigurationSection> ConfigurationSections { get; set; }
        public DbSet<DashboardAction> DashboardActions { get; set; }
        public DbSet<DashboardActionTarget> DashboardActionTargets { get; set; }
        public DbSet<DashboardCell> DashboardCells { get; set; }
        public DbSet<DashboardDevice> DashboardDevices { get; set; }
        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<DashboardSubAction> DashboardSubActions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DashboardSubAction>()
                .HasKey(u => new { u.DashboardActionChildId, u.DashboardActionParentId });

            modelBuilder.Entity<DashboardAction>()
                .HasMany(u => u.DashboardSubActionChildren)
                .WithOne(f => f.DashboardActionParent)
                .HasForeignKey(f => f.DashboardActionParentId);

            modelBuilder.Entity<DashboardAction>()
                .HasMany(u => u.DashboardSubActionParents)
                .WithOne(f => f.DashboardActionChild)
                .HasForeignKey(f => f.DashboardActionChildId);

            modelBuilder.Entity<Dashboard>().HasData(SeedDashboards());

            modelBuilder.Entity<DashboardActionTarget>().HasData(SeedDashboardActionTargets());

            base.OnModelCreating(modelBuilder);
        }

        private List<DashboardActionTarget> SeedDashboardActionTargets()
        {
            return new List<DashboardActionTarget>
            {
                new DashboardActionTarget()
                {
                    DashboardActionTargetId = DashboardActionTarget.ServerTargetId,
                    Name = "ShortDash Server",
                    PublicKey = "----"
                }
            };
        }

        private List<Dashboard> SeedDashboards()
        {
            return new List<Dashboard>
            {
                new Dashboard
                {
                    DashboardId = 1,
                    Name = "Main"
                }
            };
        }
    }
}