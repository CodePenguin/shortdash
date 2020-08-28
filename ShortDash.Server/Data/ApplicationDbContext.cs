using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<DashboardCell> DashboardCells { get; set; }
        public DbSet<DashboardAction> DashboardActions { get; set; }
        public DbSet<DashboardActionTarget> DashboardActionTargets { get; set; }
        public DbSet<DashboardSubAction> DashboardSubActions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

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
            modelBuilder.Entity<DashboardAction>().HasData(SeedDashboardActions());
            modelBuilder.Entity<DashboardSubAction>().HasData(SeedDashboardSubActions());
            modelBuilder.Entity<DashboardCell>().HasData(SeedDashboardCells());
            base.OnModelCreating(modelBuilder);
        }

        private List<Dashboard> SeedDashboards()
        {
            return new List<Dashboard> {
                new Dashboard { DashboardId = 1, Title = "Main" },
                // TODO: REMOVE DEBUG SEEDS
                new Dashboard { DashboardId = 2, Title = "Dash 2" },
                new Dashboard { DashboardId = 3, Title = "Dash 3" }
            };
        }

        private List<DashboardAction> SeedDashboardActions()
        {
            return new List<DashboardAction> {
                new DashboardAction() { DashboardActionId = 1, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Twitter", Icon = "Twitter.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://twitter.com\"}" },
                new DashboardAction() { DashboardActionId = 2, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Discord", Icon = "Phone.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://discord.com\"}" },
                new DashboardAction() { DashboardActionId = 3, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Slack", BackgroundColor = Gray, Icon = "Mail.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://slack.com\"}" },
                new DashboardAction() { DashboardActionId = 4, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Mute", BackgroundColor = EtonBlue, Icon = "open-iconic/svg/ban.svg", Parameters = "{\"IsToggle\":true}" },
                new DashboardAction() { DashboardActionId = 5, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Prev", BackgroundColor = Gray },
                new DashboardAction() { DashboardActionId = 6, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Play", BackgroundColor = DarkBlue },
                new DashboardAction() { DashboardActionId = 7, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Next", BackgroundColor = WildBlue },
                new DashboardAction() { DashboardActionId = 8, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Notepad", Icon = "Settings.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"c:\\\\windows\\\\Notepad.exe\",\"Arguments\":\"d:\\\\temp\\\\temp.txt\",\"WorkingDirectory\":\"c:\\\\windows\\\\\"}" },
                new DashboardAction() { DashboardActionId = 9, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Batch", BackgroundColor = EtonBlue, Icon = "Maps.png" },
                new DashboardAction() { DashboardActionId = 10, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Multi", Icon = "Mail.png" },
                new DashboardAction() { DashboardActionId = 11, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Dash 2", ActionClass="DashLink", Parameters = "{\"DashboardId\":2}" },
                new DashboardAction() { DashboardActionId = 12, DashboardActionTargetId = 1, ActionType = DashboardActionType.Action, Title = "Dash 3", ActionClass="DashLink", Parameters = "{\"DashboardId\":3}" },
            };
        }

        private List<DashboardSubAction> SeedDashboardSubActions()
        {
            return new List<DashboardSubAction> {
                new DashboardSubAction() { DashboardActionParentId = 10, DashboardActionChildId = 1  },
                new DashboardSubAction() { DashboardActionParentId = 10, DashboardActionChildId = 2 },
            };
        }

        private List<DashboardActionTarget> SeedDashboardActionTargets()
        {
            return new List<DashboardActionTarget> {
                new DashboardActionTarget() { DashboardActionTargetId = 1, Title = "Main" },
            };
        }

        private List<DashboardCell> SeedDashboardCells()
        {
            // TODO: REMOVE DEBUG SEEDS
            return new List<DashboardCell>
            {
                new DashboardCell() { DashboardCellId = 1, DashboardId = 2, Title = "Twitter", CellType = DashboardCellType.Action, DashboardActionId = 1, Icon = "Twitter.png" },
                new DashboardCell() { DashboardCellId = 2, DashboardId = 2, Title = "Discord", CellType = DashboardCellType.Action, DashboardActionId = 2, Icon = "Phone.png" },
                new DashboardCell() { DashboardCellId = 3, DashboardId = 2, Title = "Slack", CellType = DashboardCellType.Action, DashboardActionId = 3, BackgroundColor = Gray, Icon = "Mail.png"},
                new DashboardCell() { DashboardCellId = 4, DashboardId = 2, Title = "Dash 3", CellType = DashboardCellType.DashLink, DashboardActionId = 12, BackgroundColor = GreenSheen, Icon = "open-iconic/svg/grid-three-up.svg" },
                new DashboardCell() { DashboardCellId = 5, DashboardId = 2, Title = "Mute", CellType = DashboardCellType.Action, DashboardActionId = 4, BackgroundColor = EtonBlue, Icon = "open-iconic/svg/ban.svg" },
                new DashboardCell() { DashboardCellId = 6, DashboardId = 2, Title = "Prev", CellType = DashboardCellType.Action, DashboardActionId = 5, BackgroundColor = Gray },
                new DashboardCell() { DashboardCellId = 7, DashboardId = 2, Title = "Play", CellType = DashboardCellType.Action, DashboardActionId = 6, BackgroundColor = DarkBlue },
                new DashboardCell() { DashboardCellId = 8, DashboardId = 2, Title = "Next", CellType = DashboardCellType.Action, DashboardActionId = 7, BackgroundColor = WildBlue },
                new DashboardCell() { DashboardCellId = 9, DashboardId = 2, Title = "Notepad", CellType = DashboardCellType.Action, DashboardActionId = 8, Icon = "Settings.png" },
                new DashboardCell() { DashboardCellId = 10, DashboardId = 2 },
                new DashboardCell() { DashboardCellId = 11, DashboardId = 2 },
                new DashboardCell() { DashboardCellId = 12, DashboardId = 2, Title = "Batch", CellType = DashboardCellType.Action, DashboardActionId = 9, Icon = "Maps.png" },
                new DashboardCell() { DashboardCellId = 13, DashboardId = 3, Title = "Twitter", CellType = DashboardCellType.Action, DashboardActionId = 1, Icon = "Twitter.png" },
                new DashboardCell() { DashboardCellId = 14, DashboardId = 3, Title = "Discord", CellType = DashboardCellType.Action, DashboardActionId = 2, Icon = "Phone.png" },
                new DashboardCell() { DashboardCellId = 15, DashboardId = 3, Title = "Dash 2", CellType = DashboardCellType.DashLink, DashboardActionId = 11, BackgroundColor = GreenSheen, Icon = "open-iconic/svg/grid-three-up.svg" },
                new DashboardCell() { DashboardCellId = 16, DashboardId = 3, Title = "GMail", CellType = DashboardCellType.Action, DashboardActionId = 10, Icon = "Mail.png" },
                new DashboardCell() { DashboardCellId = 17, DashboardId = 3 },
                new DashboardCell() { DashboardCellId = 18, DashboardId = 3, Title = "Batch", CellType = DashboardCellType.Action, DashboardActionId = 9, BackgroundColor = EtonBlue, Icon = "Podcast.png" }
            };
        }

        // TODO: REMOVE DEBUG COLORS
        const string DarkBlue = "#769ECB";
        const string WildBlue = "#9DBAD5";
        const string Gray = "#C8D6B9";
        const string EtonBlue = "#8FC1A9";
        const string GreenSheen = "#7CAA98";
    }
}
