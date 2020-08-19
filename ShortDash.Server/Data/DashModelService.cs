using System.IO;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class DashModelService
    {

        public Task<DashModel> GetDashModelAsync(int id)
        {
            if (id != 1) throw new FileNotFoundException($"Dashboard not found {id}");

            var result = new DashModel();

            // Row 1
            var row = new GridRow();
            row.Cells.Add(new GridCell() { Title = "Twitter" });
            row.Cells.Add(new GridCell() { Title = "Discord" });
            row.Cells.Add(new GridCell() { Title = "Slack" });
            row.Cells.Add(new GridCell() { Title = "Chrome" });
            result.Rows.Add(row);

            // Row 2
            row = new GridRow();
            row.Cells.Add(new GridCell() { Title = "Mute" });
            row.Cells.Add(new GridCell() { Title = "Prev" });
            row.Cells.Add(new GridCell() { Title = "Play" });
            row.Cells.Add(new GridCell() { Title = "Next" });
            result.Rows.Add(row);

            // Row 3
            row = new GridRow();
            row.Cells.Add(new GridCell() { Title = "GMail" });
            row.Cells.Add(new GridCell() { Title = "Seperator" });
            row.Cells.Add(new GridCell() { Title = "Seperator" });
            row.Cells.Add(new GridCell() { Title = "Batch" });
            result.Rows.Add(row);

            return Task.FromResult(result);
        }

    }
}
