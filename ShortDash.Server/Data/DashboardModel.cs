using ShortDash.Server.Extensions;
using ShortDash.Server.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace ShortDash.Server.Data
{
    public class Dashboard
    {
        [NotMapped]
        public Color? BackgroundColor { get; set; }

        [Column("BackgroundColor")]
        public string BackgroundColorHtmlValue
        {
            get => BackgroundColor?.ToHtmlString();
            set => BackgroundColor = ColorExtensions.FromHtmlString(value);
        }

        public virtual List<DashboardCell> DashboardCells { get; set; } = new List<DashboardCell>();
        public int DashboardId { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class DashboardAction
    {
        public string ActionTypeName { get; set; }

        [NotMapped]
        public Color? BackgroundColor { get; set; }

        [Column("BackgroundColor")]
        public string BackgroundColorHtmlValue
        {
            get => BackgroundColor?.ToHtmlString();
            set => BackgroundColor = ColorExtensions.FromHtmlString(value);
        }

        public int DashboardActionId { get; set; }
        public virtual DashboardActionTarget DashboardActionTarget { get; set; }
        public int DashboardActionTargetId { get; set; }
        public virtual List<DashboardSubAction> DashboardSubActionChildren { get; set; } = new List<DashboardSubAction>();

        public virtual List<DashboardSubAction> DashboardSubActionParents { get; set; } = new List<DashboardSubAction>();

        public string Icon { get; set; } = "";

        [Required]
        public string Label { get; set; }

        public string Parameters { get; set; } = "{}";
    }

    public class DashboardActionTarget
    {
        public int DashboardActionTargetId { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class DashboardCell
    {
        public virtual Dashboard Dashboard { get; set; }
        public virtual DashboardAction DashboardAction { get; set; }
        public int DashboardActionId { get; set; }
        public int DashboardCellId { get; set; }
        public int DashboardId { get; set; }
        public int Sequence { get; set; }
    }

    public class DashboardSubAction
    {
        public virtual DashboardAction DashboardActionChild { get; set; }
        public int DashboardActionChildId { get; set; }
        public virtual DashboardAction DashboardActionParent { get; set; }
        public int DashboardActionParentId { get; set; }
        public int Sequence { get; set; }
    }
}