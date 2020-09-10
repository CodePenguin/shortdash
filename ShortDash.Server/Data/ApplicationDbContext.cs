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

        public DbSet<DashboardAction> DashboardActions { get; set; }
        public DbSet<DashboardActionTarget> DashboardActionTargets { get; set; }
        public DbSet<DashboardCell> DashboardCells { get; set; }
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
            modelBuilder.Entity<DashboardAction>().HasData(SeedDashboardActions());
            modelBuilder.Entity<DashboardSubAction>().HasData(SeedDashboardSubActions());
            modelBuilder.Entity<DashboardCell>().HasData(SeedDashboardCells());
            base.OnModelCreating(modelBuilder);
        }

        private List<DashboardAction> SeedDashboardActions()
        {
            // TODO: REMOVE DEBUG SEEDS
            return new List<DashboardAction>
            {
                new DashboardAction() { DashboardActionId = 1, DashboardActionTargetId = 1, Label = "Twitter", Icon = "Twitter.png", ActionTypeName = "ShortDash.Plugins.Core.Windows.ExecuteProcessAction", Parameters = "{\"FileName\":\"https://twitter.com\"}" },
                new DashboardAction() { DashboardActionId = 2, DashboardActionTargetId = 1, Label = "Discord", Icon = "Phone.png", ActionTypeName = "ShortDash.Plugins.Core.Windows.ExecuteProcessAction", Parameters = "{\"FileName\":\"https://discord.com\"}" },
                new DashboardAction() { DashboardActionId = 3, DashboardActionTargetId = 1, Label = "Slack", BackgroundColor = Color.Gray, Icon = "Mail.png", ActionTypeName = "ShortDash.Plugins.Core.Windows.ExecuteProcessAction", Parameters = "{\"FileName\":\"https://slack.com\"}" },
                new DashboardAction() { DashboardActionId = 4, DashboardActionTargetId = 1, Label = "Mute", BackgroundColor = Color.CornflowerBlue, Icon = "oi-ban", ActionTypeName = "ShortDash.Plugins.Core.Windows.MediaMuteAction", Parameters = "{\"IsToggle\":true}" },
                new DashboardAction() { DashboardActionId = 5, DashboardActionTargetId = 1, Label = "Prev", BackgroundColor = Color.Gray, ActionTypeName = "ShortDash.Plugins.Core.Windows.MediaPreviousTrackAction" },
                new DashboardAction() { DashboardActionId = 6, DashboardActionTargetId = 1, Label = "Play", BackgroundColor = Color.DarkBlue, Icon = "oi-media-play", ActionTypeName = "ShortDash.Plugins.Core.Windows.MediaPlayPauseAction" },
                new DashboardAction() { DashboardActionId = 7, DashboardActionTargetId = 1, Label = "Next", BackgroundColor = Color.SeaGreen, ActionTypeName = "ShortDash.Plugins.Core.Windows.MediaNextTrackAction" },
                new DashboardAction() { DashboardActionId = 8, DashboardActionTargetId = 1, Label = "Notepad", Icon = "Settings.png", ActionTypeName = "ShortDash.Plugins.Core.Windows.ExecuteProcessAction", Parameters = "{\"FileName\":\"c:\\\\windows\\\\Notepad.exe\",\"Arguments\":\"d:\\\\temp\\\\temp.txt\",\"WorkingDirectory\":\"c:\\\\windows\\\\\"}" },
                new DashboardAction() { DashboardActionId = 9, DashboardActionTargetId = 1, Label = "Batch", BackgroundColor = Color.LavenderBlush, Icon = "Maps.png", ActionTypeName = "ShortDash.Plugins.Core.Windows.ExecuteProcessAction", Parameters = "{\"FileName\":\"test.batch\"}" },
                new DashboardAction() { DashboardActionId = 10, DashboardActionTargetId = 1, Label = "Multi", Icon = "Mail.png", ActionTypeName = "Composite" },
                new DashboardAction() { DashboardActionId = 11, DashboardActionTargetId = 1, Label = "Dash 2", BackgroundColor = Color.LightSeaGreen, Icon = "oi-grid-three-up", ActionTypeName = "ShortDash.Server.Actions.DashLinkAction", Parameters = "{\"DashboardId\":2}" },
                new DashboardAction() { DashboardActionId = 12, DashboardActionTargetId = 1, Label = "Dash 3", BackgroundColor = Color.SpringGreen, Icon = "oi-grid-three-up", ActionTypeName = "ShortDash.Server.Actions.DashLinkAction", Parameters = "{\"DashboardId\":3}" },
                new DashboardAction() { DashboardActionId = 13, DashboardActionTargetId = 1, Label = "Separator", ActionTypeName = "ShortDash.Server.Actions.DashSeparatorAction" },
            };
        }

        private List<DashboardActionTarget> SeedDashboardActionTargets()
        {
            return new List<DashboardActionTarget>
            {
                new DashboardActionTarget() { DashboardActionTargetId = 1, Name = "Main" },
            };
        }

        private List<DashboardCell> SeedDashboardCells()
        {
            // TODO: REMOVE DEBUG SEEDS
            return new List<DashboardCell>
            {
                new DashboardCell() { DashboardCellId = 1, DashboardId = 2, DashboardActionId = 1 },
                new DashboardCell() { DashboardCellId = 2, DashboardId = 2, DashboardActionId = 2 },
                new DashboardCell() { DashboardCellId = 3, DashboardId = 2, DashboardActionId = 3 },
                new DashboardCell() { DashboardCellId = 4, DashboardId = 2, DashboardActionId = 12 },
                new DashboardCell() { DashboardCellId = 5, DashboardId = 2, DashboardActionId = 4 },
                new DashboardCell() { DashboardCellId = 6, DashboardId = 2, DashboardActionId = 5 },
                new DashboardCell() { DashboardCellId = 7, DashboardId = 2, DashboardActionId = 6 },
                new DashboardCell() { DashboardCellId = 8, DashboardId = 2, DashboardActionId = 7 },
                new DashboardCell() { DashboardCellId = 9, DashboardId = 2, DashboardActionId = 8 },
                new DashboardCell() { DashboardCellId = 10, DashboardId = 2, DashboardActionId = 13 },
                new DashboardCell() { DashboardCellId = 11, DashboardId = 2, DashboardActionId = 13 },
                new DashboardCell() { DashboardCellId = 12, DashboardId = 2, DashboardActionId = 9 },
                new DashboardCell() { DashboardCellId = 13, DashboardId = 3, DashboardActionId = 1 },
                new DashboardCell() { DashboardCellId = 14, DashboardId = 3, DashboardActionId = 2 },
                new DashboardCell() { DashboardCellId = 15, DashboardId = 3, DashboardActionId = 11 },
                new DashboardCell() { DashboardCellId = 16, DashboardId = 3, DashboardActionId = 10 },
                new DashboardCell() { DashboardCellId = 17, DashboardId = 3, DashboardActionId = 13 },
                new DashboardCell() { DashboardCellId = 18, DashboardId = 3, DashboardActionId = 9 }
            };
        }

        private List<Dashboard> SeedDashboards()
        {
            return new List<Dashboard>
            {
                new Dashboard { DashboardId = 1, Name = "Main" },
                // TODO: REMOVE DEBUG SEEDS
                new Dashboard { DashboardId = 2, Name = "Dash 2" },
                new Dashboard { DashboardId = 3, Name = "Dash 3" }
            };
        }

        private List<DashboardSubAction> SeedDashboardSubActions()
        {
            // TODO: REMOVE DEBUG SEEDS
            return new List<DashboardSubAction>
            {
                new DashboardSubAction() { DashboardActionParentId = 10, DashboardActionChildId = 1 },
                new DashboardSubAction() { DashboardActionParentId = 10, DashboardActionChildId = 2 },
            };
        }
    }
}