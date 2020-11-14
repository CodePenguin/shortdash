using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashboardGridCell : ComponentBase
    {
        [Parameter]
        public DashboardCell Cell { get; set; }

        [Parameter]
        public bool EditMode { get; set; }

        [Parameter]
        public bool HideLabel { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnMoveCellLeft { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnMoveCellRight { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnRemoveCell { get; set; }

        private Task MoveCellLeft()
        {
            return OnMoveCellLeft.InvokeAsync(Cell);
        }

        private Task MoveCellRight()
        {
            return OnMoveCellRight.InvokeAsync(Cell);
        }

        private Task RemoveCell()
        {
            return OnRemoveCell.InvokeAsync(Cell);
        }
    }
}