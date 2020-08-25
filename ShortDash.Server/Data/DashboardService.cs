using System;
using System.IO;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class DashboardService
    {
        const string DarkBlue = "#769ECB";
        const string WildBlue = "#9DBAD5";
        const string Gray = "#C8D6B9";
        const string EtonBlue = "#8FC1A9";
        const string GreenSheen= "#7CAA98";

        public Task<Dashboard> GetDashboardAsync(int id)
        {            
            var result = new Dashboard();
            if (id == 1)
            {
                // Row 1
                result.Cells.Add(new DashboardCell() { Title = "Twitter", CellType = DashboardCellType.Action, Icon = "Twitter.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://twitter.com\"}" });
                result.Cells.Add(new DashboardCell() { Title = "Discord", CellType = DashboardCellType.Action, Icon = "Phone.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://discord.com\"}" });
                result.Cells.Add(new DashboardCell() { Title = "Slack", CellType = DashboardCellType.Action, BackgroundColor = Gray, Icon = "Mail.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://slack.com\"}" });
                result.Cells.Add(new DashboardCell() { Title = "Dash 2", CellType = DashboardCellType.DashLink, BackgroundColor = GreenSheen, Icon = "open-iconic/svg/grid-three-up.svg", Parameters = "{\"DashboardId\":2}" });

                result.Cells.Add(new DashboardCell() { Title = "Mute", CellType = DashboardCellType.Action, BackgroundColor = EtonBlue, Icon = "open-iconic/svg/ban.svg", Parameters = "{\"IsToggle\":true}" });
                result.Cells.Add(new DashboardCell() { Title = "Prev", CellType = DashboardCellType.Action, BackgroundColor = Gray });
                result.Cells.Add(new DashboardCell() { Title = "Play", CellType = DashboardCellType.Action, BackgroundColor = DarkBlue });
                result.Cells.Add(new DashboardCell() { Title = "Next", CellType = DashboardCellType.Action, BackgroundColor = WildBlue });

                result.Cells.Add(new DashboardCell() { Title = "Notepad", CellType = DashboardCellType.Action, Icon = "Settings.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"c:\\\\windows\\\\Notepad.exe\",\"Arguments\":\"d:\\\\temp\\\\temp.txt\",\"WorkingDirectory\":\"c:\\\\windows\\\\\"}" });
                result.Cells.Add(new DashboardCell());
                result.Cells.Add(new DashboardCell());
                result.Cells.Add(new DashboardCell() { Title = "Batch", CellType = DashboardCellType.Action, Icon = "Maps.png" });
            } 
            else if (id == 2)
            {
                // Row 1
                result.Cells.Add(new DashboardCell() { Title = "Twitter", CellType = DashboardCellType.Action, Icon = "Twitter.png" });
                result.Cells.Add(new DashboardCell() { Title = "Discord", CellType = DashboardCellType.Action, Icon = "Phone.png" });
                result.Cells.Add(new DashboardCell() { Title = "Dash 1", CellType = DashboardCellType.DashLink, BackgroundColor = GreenSheen, Icon = "open-iconic/svg/grid-three-up.svg", Parameters = "{\"DashboardId\":1}" });

                result.Cells.Add(new DashboardCell() { Title = "GMail", CellType = DashboardCellType.Action, Icon = "Mail.png" });
                result.Cells.Add(new DashboardCell());
                result.Cells.Add(new DashboardCell() { Title = "Batch", CellType = DashboardCellType.Action, BackgroundColor = EtonBlue, Icon = "Podcast.png" });
            }
            else
            {
                throw new FileNotFoundException($"Dashboard not found {id}");
            }

            return Task.FromResult(result);
        }

        public static string RandomIcon()
        {
            var Icons = new[]
            {
                "Settings", "Clock", "Phone", "Photos", "Mail", "Podcast", "Maps"
            };
            var rng = new Random();
            return Icons[rng.Next(Icons.Length)] + ".png";
        }

    }
}
