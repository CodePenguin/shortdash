using ShortDash.Server.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

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
        public int? DashboardActionId { get; set; }
        public int Sequence { get; set; }

        public virtual Dashboard Dashboard { get; set; }
        public virtual DashboardAction DashboardAction { get; set; }
    }

    public class DashboardAction
    {
        public int DashboardActionId { get; set; }
        public int DashboardActionTargetId { get; set; }
        public string ActionClass { get; set; }
        [Required]
        public string Title { get; set; }
        [Column("BackgroundColor")]
        public string BackgroundColorHtmlValue
        {
            get => BackgroundColor?.ToHtmlString();
            set
            {
                if (ColorExtensions.TryParse(value, out var color))
                {
                    BackgroundColor = color;
                }
                else
                {
                    BackgroundColor = null;
                }
            }
        }

        [NotMapped]
        public Color? BackgroundColor { get; set; } = Color.Black;
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
