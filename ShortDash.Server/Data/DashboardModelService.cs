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
                var row = new GridRow();
                row.Cells.Add(new GridCell() { Title = "Twitter", CellType = GridCellType.Action, BackgroundColor = DarkBlue, Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://twitter.com\"}" });
                row.Cells.Add(new GridCell() { Title = "Discord", CellType = GridCellType.Action, BackgroundColor = WildBlue, Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://discord.com\"}" });
                row.Cells.Add(new GridCell() { Title = "Slack", CellType = GridCellType.Action, BackgroundColor = Gray, Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"https://slack.com\"}" });
                row.Cells.Add(new GridCell() { Title = "Dash 2", CellType = GridCellType.DashLink, Parameters = "{\"DashboardId\":2}" });
                result.Rows.Add(row);

                // Row 2
                row = new GridRow();
                row.Cells.Add(new GridCell() { Title = "Mute", CellType = GridCellType.Action, Parameters = "{\"IsToggle\":true}" });
                row.Cells.Add(new GridCell() { Title = "Prev", CellType = GridCellType.Action, BackgroundColor = Gray });
                row.Cells.Add(new GridCell() { Title = "Play", CellType = GridCellType.Action });
                row.Cells.Add(new GridCell() { Title = "Next", CellType = GridCellType.Action });
                result.Rows.Add(row);

                // Row 3
                row = new GridRow();
                row.Cells.Add(new GridCell() { Title = "Notepad", CellType = GridCellType.Action, BackgroundColor = GreenSheen, Parameters = "{\"ActionType\":\"ExecuteProcess\",\"FileName\":\"c:\\\\windows\\\\Notepad.exe\",\"Arguments\":\"d:\\\\temp\\\\temp.txt\",\"WorkingDirectory\":\"c:\\\\windows\\\\\"}" });
                row.Cells.Add(new GridCell());
                row.Cells.Add(new GridCell());
                row.Cells.Add(new GridCell() { Title = "Batch", CellType = GridCellType.Action, BackgroundColor = EtonBlue });
                result.Rows.Add(row);
            } 
            else if (id == 2)
            {
                // Row 1
                var row = new GridRow();
                row.Cells.Add(new GridCell() { Title = "Twitter", CellType = GridCellType.Action, BackgroundColor = DarkBlue });
                row.Cells.Add(new GridCell() { Title = "Discord", CellType = GridCellType.Action, BackgroundColor = WildBlue });
                row.Cells.Add(new GridCell() { Title = "Dash 1", CellType = GridCellType.DashLink, Parameters = "{\"DashboardId\":1}" });
                result.Rows.Add(row);

                // Row 2
                row = new GridRow();
                row.Cells.Add(new GridCell() { Title = "GMail", CellType = GridCellType.Action, BackgroundColor = GreenSheen });
                row.Cells.Add(new GridCell());
                row.Cells.Add(new GridCell() { Title = "Batch", CellType = GridCellType.Action, BackgroundColor = EtonBlue });
                result.Rows.Add(row);
            }
            else
            {
                throw new FileNotFoundException($"Dashboard not found {id}");
            }

            return Task.FromResult(result);
        }

    }
}
