using System.IO;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class DashModelService
    {
        const string DarkBlue = "#769ECB";
        const string WildBlue = "#9DBAD5";
        const string Beige = "#FAF3DD";
        const string Gray = "#C8D6B9";
        const string EtonBlue = "#8FC1A9";
        const string GreenSheen= "#7CAA98";

        public Task<DashModel> GetDashModelAsync(int id)
        {
            if (id != 1) throw new FileNotFoundException($"Dashboard not found {id}");

            var result = new DashModel();

            // Row 1
            var row = new GridRow();
            row.Cells.Add(new GridCell() { Title = "Twitter", CellType = GridCellType.Action, BackgroundColor = DarkBlue });
            row.Cells.Add(new GridCell() { Title = "Discord", CellType = GridCellType.Action, BackgroundColor = WildBlue});
            row.Cells.Add(new GridCell() { Title = "Slack", CellType = GridCellType.Action, BackgroundColor = Beige });
            row.Cells.Add(new GridCell() { Title = "Chrome", CellType = GridCellType.Action });
            result.Rows.Add(row);

            // Row 2
            row = new GridRow();
            row.Cells.Add(new GridCell() { Title = "Mute", CellType = GridCellType.Action });
            row.Cells.Add(new GridCell() { Title = "Prev", CellType = GridCellType.Action, BackgroundColor = Gray });
            row.Cells.Add(new GridCell() { Title = "Play", CellType = GridCellType.Action });
            row.Cells.Add(new GridCell() { Title = "Next", CellType = GridCellType.Action });
            result.Rows.Add(row);

            // Row 3
            row = new GridRow();
            row.Cells.Add(new GridCell() { Title = "GMail", CellType = GridCellType.Action, BackgroundColor = GreenSheen });
            row.Cells.Add(new GridCell());
            row.Cells.Add(new GridCell());
            row.Cells.Add(new GridCell() { Title = "Batch", CellType = GridCellType.Action, BackgroundColor = EtonBlue });
            result.Rows.Add(row);

            return Task.FromResult(result);
        }

    }
}
