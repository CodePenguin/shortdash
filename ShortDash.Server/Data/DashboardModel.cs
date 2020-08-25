using System.Collections.Generic;

namespace ShortDash.Server.Data
{
    public enum DashboardCellType
    {
        None = 0,
        Action = 1,
        DashLink = 2
    }

    public class DashboardCell 
    {
        public string Title { get; set; }
        public DashboardCellType CellType { get; set; } = DashboardCellType.None;
        public string BackgroundColor { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Parameters { get; set; } = "{}";
    }

    public class Dashboard
    {
        public int DashboardId { get; set; }
        public string Title { get; set; }

        public List<DashboardCell> Cells { get; } = new List<DashboardCell>();
    }
}
