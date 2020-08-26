using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<DashboardCell> DashboardCells { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dashboard>().HasData(SeedDashboards());
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

        private List<DashboardCell> SeedDashboardCells()
        {
            // TODO: REMOVE DEBUG SEEDS
            const string DarkBlue = "#769ECB";
            const string WildBlue = "#9DBAD5";
            const string Gray = "#C8D6B9";
            const string EtonBlue = "#8FC1A9";
            const string GreenSheen = "#7CAA98";
            return new List<DashboardCell>
            {
                new DashboardCell() { DashboardCellId = 1, DashboardId = 2, Title = "Twitter", CellType = DashboardCellType.Action, Icon = "Twitter.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://twitter.com\"}" },
                new DashboardCell() { DashboardCellId = 2, DashboardId = 2, Title = "Discord", CellType = DashboardCellType.Action, Icon = "Phone.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://discord.com\"}" },
                new DashboardCell() { DashboardCellId = 3, DashboardId = 2, Title = "Slack", CellType = DashboardCellType.Action, BackgroundColor = Gray, Icon = "Mail.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://slack.com\"}" },
                new DashboardCell() { DashboardCellId = 4, DashboardId = 2, Title = "Dash 2", CellType = DashboardCellType.DashLink, BackgroundColor = GreenSheen, Icon = "open-iconic/svg/grid-three-up.svg", Parameters = "{\"DashboardId\":3}" },
                new DashboardCell() { DashboardCellId = 5, DashboardId = 2, Title = "Mute", CellType = DashboardCellType.Action, BackgroundColor = EtonBlue, Icon = "open-iconic/svg/ban.svg", Parameters = "{\"IsToggle\":true}" },
                new DashboardCell() { DashboardCellId = 6, DashboardId = 2, Title = "Prev", CellType = DashboardCellType.Action, BackgroundColor = Gray },
                new DashboardCell() { DashboardCellId = 7, DashboardId = 2, Title = "Play", CellType = DashboardCellType.Action, BackgroundColor = DarkBlue },
                new DashboardCell() { DashboardCellId = 8, DashboardId = 2, Title = "Next", CellType = DashboardCellType.Action, BackgroundColor = WildBlue },
                new DashboardCell() { DashboardCellId = 9, DashboardId = 2, Title = "Notepad", CellType = DashboardCellType.Action, Icon = "Settings.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"c:\\\\windows\\\\Notepad.exe\",\"Arguments\":\"d:\\\\temp\\\\temp.txt\",\"WorkingDirectory\":\"c:\\\\windows\\\\\"}" },
                new DashboardCell() { DashboardCellId = 10, DashboardId = 2 },
                new DashboardCell() { DashboardCellId = 11, DashboardId = 2 },
                new DashboardCell() { DashboardCellId = 12, DashboardId = 2, Title = "Batch", CellType = DashboardCellType.Action, Icon = "Maps.png" },
                new DashboardCell() { DashboardCellId = 13, DashboardId = 3, Title = "Twitter", CellType = DashboardCellType.Action, Icon = "Twitter.png" },
                new DashboardCell() { DashboardCellId = 14, DashboardId = 3, Title = "Discord", CellType = DashboardCellType.Action, Icon = "Phone.png" },
                new DashboardCell() { DashboardCellId = 15, DashboardId = 3, Title = "Dash 1", CellType = DashboardCellType.DashLink, BackgroundColor = GreenSheen, Icon = "open-iconic/svg/grid-three-up.svg", Parameters = "{\"DashboardId\":2}" },
                new DashboardCell() { DashboardCellId = 16, DashboardId = 3, Title = "GMail", CellType = DashboardCellType.Action, Icon = "Mail.png" },
                new DashboardCell() { DashboardCellId = 17, DashboardId = 3 },
                new DashboardCell() { DashboardCellId = 18, DashboardId = 3, Title = "Batch", CellType = DashboardCellType.Action, BackgroundColor = EtonBlue, Icon = "Podcast.png" }
            };
        }

    }
}
