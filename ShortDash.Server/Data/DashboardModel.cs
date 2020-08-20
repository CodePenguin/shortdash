using System.Collections.Generic;

namespace ShortDash.Server.Data
{
    public enum GridCellType
    {
        None,
        Action,
        DashLink
    }

    public class GridCell 
    {
        public string Title { get; set; }
        public GridCellType CellType { get; set; } = GridCellType.None;
        public string BackgroundColor { get; set; } = "";
        public string Parameters { get; set; } = "{}";
    }

    public class GridRow
    {
        public readonly List<GridCell> Cells;

        public GridRow()
        {
            Cells = new List<GridCell>();
        }
    }
    public class DashboardModel
    {
        public string Title { get; set; }

        public readonly List<GridRow> Rows;

        public DashboardModel()
        {
            Rows = new List<GridRow>();
        }
    }
}
