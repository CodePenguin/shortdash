using System;
using System.IO;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class DashboardModelService
    {
        const string DarkBlue = "#769ECB";
        const string WildBlue = "#9DBAD5";
        const string Gray = "#C8D6B9";
        const string EtonBlue = "#8FC1A9";
        const string GreenSheen= "#7CAA98";

        public Task<DashboardModel> GetDashModelAsync(int id)
        {            
            var result = new DashboardModel();
            if (id == 1)
            {
                // Row 1
                result.Cells.Add(new GridCell() { Title = "Twitter", CellType = GridCellType.Action, Icon = "Twitter.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://twitter.com\"}" });
                result.Cells.Add(new GridCell() { Title = "Discord", CellType = GridCellType.Action, Icon = "Phone.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://discord.com\"}" });
                result.Cells.Add(new GridCell() { Title = "Slack", CellType = GridCellType.Action, BackgroundColor = Gray, Icon = "Mail.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://slack.com\"}" });
                result.Cells.Add(new GridCell() { Title = "Dash 2", CellType = GridCellType.DashLink, BackgroundColor = GreenSheen, Parameters = "{\"DashboardId\":2}" });

                result.Cells.Add(new GridCell() { Title = "Mute", CellType = GridCellType.Action, Icon = "Photos.png", Parameters = "{\"IsToggle\":true}" });
                result.Cells.Add(new GridCell() { Title = "Prev", CellType = GridCellType.Action, BackgroundColor = Gray });
                result.Cells.Add(new GridCell() { Title = "Play", CellType = GridCellType.Action, BackgroundColor = DarkBlue });
                result.Cells.Add(new GridCell() { Title = "Next", CellType = GridCellType.Action, BackgroundColor = WildBlue });

                result.Cells.Add(new GridCell() { Title = "Notepad", CellType = GridCellType.Action, Icon = "Settings.png", Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"c:\\\\windows\\\\Notepad.exe\",\"Arguments\":\"d:\\\\temp\\\\temp.txt\",\"WorkingDirectory\":\"c:\\\\windows\\\\\"}" });
                result.Cells.Add(new GridCell());
                result.Cells.Add(new GridCell());
                result.Cells.Add(new GridCell() { Title = "Batch", CellType = GridCellType.Action, Icon = "Maps.png" });
            } 
            else if (id == 2)
            {
                // Row 1
                result.Cells.Add(new GridCell() { Title = "Twitter", CellType = GridCellType.Action, Icon = "Twitter.png" });
                result.Cells.Add(new GridCell() { Title = "Discord", CellType = GridCellType.Action, Icon = "Phone.png" });
                result.Cells.Add(new GridCell() { Title = "Dash 1", CellType = GridCellType.DashLink, BackgroundColor = GreenSheen, Parameters = "{\"DashboardId\":1}" });

                result.Cells.Add(new GridCell() { Title = "GMail", CellType = GridCellType.Action, Icon = "Mail.png" });
                result.Cells.Add(new GridCell());
                result.Cells.Add(new GridCell() { Title = "Batch", CellType = GridCellType.Action, BackgroundColor = EtonBlue, Icon = "Podcast.png" });
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
