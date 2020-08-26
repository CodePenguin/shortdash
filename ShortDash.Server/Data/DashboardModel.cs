using System.Collections.Generic;

namespace ShortDash.Server.Data
{
    public class Dashboard
    {
        public int DashboardId { get; set; }
        public string Title { get; set; }

        public virtual List<DashboardCell> DashboardCells { get; set; } = new List<DashboardCell>();
    }

    public class DashboardCell 
    {
        public int DashboardCellId { get; set; }
        public int DashboardId { get; set; }
        public virtual Dashboard Dashboard { get; set; }

        public string Title { get; set; }
        public DashboardCellType CellType { get; set; } = DashboardCellType.None;
        public string BackgroundColor { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Parameters { get; set; } = "{}";
    }

    public enum DashboardCellType
    {
        None = 0,
        Action = 1,
        DashLink = 2
    }
}
