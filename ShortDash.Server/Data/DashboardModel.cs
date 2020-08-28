using Microsoft.EntityFrameworkCore;
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
        public int? DashboardActionId { get; set; }
        public string Title { get; set; }
        public DashboardCellType CellType { get; set; } = DashboardCellType.None;
        public string BackgroundColor { get; set; } = "";
        public string Icon { get; set; } = "";
        public int Sequence { get; set; }

        public virtual DashboardAction DashboardAction { get; set; }
    }

    public enum DashboardCellType
    {
        None = 0,
        Action = 1,
        DashLink = 2
    }

    public enum DashboardActionType
    {
        Action = 0,
        CompositeAction = 1
    }

    public class DashboardAction
    {
        public int DashboardActionId { get; set; }
        public DashboardActionType ActionType { get; set; }
        public int DashboardActionTargetId { get; set; }
        public string ActionClass { get; set; }
        public string Title { get; set; }
        public string BackgroundColor { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Parameters { get; set; } = "{}";

        public virtual DashboardActionTarget DashboardActionTarget { get; set; }
        public virtual List<DashboardSubAction> DashboardSubActionChildren { get; set; } = new List<DashboardSubAction>();
        public virtual List<DashboardSubAction> DashboardSubActionParents { get; set; } = new List<DashboardSubAction>();
    }

    public class DashboardSubAction
    {
        public int DashboardActionParentId { get; set; }
        public int DashboardActionChildId { get; set; }
        public int Sequence { get; set; }

        public virtual DashboardAction DashboardActionParent { get; set; }
        public virtual DashboardAction DashboardActionChild { get; set; }
    }

    public class DashboardActionTarget
    {
        public int DashboardActionTargetId { get; set; }
        public string Title { get; set; }
    }
}
