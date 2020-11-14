using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class DashboardGridCellEditControlsComponent<TCellType> : ComponentBase
    {
        [Parameter]
        public TCellType Cell { get; set; }

        [Parameter]
        public EventCallback<TCellType> OnMoveCellLeft { get; set; }

        [Parameter]
        public EventCallback<TCellType> OnMoveCellRight { get; set; }

        [Parameter]
        public EventCallback<TCellType> OnRemoveCell { get; set; }

        protected Task MoveCellLeft()
        {
            return OnMoveCellLeft.InvokeAsync(Cell);
        }

        protected Task MoveCellRight()
        {
            return OnMoveCellRight.InvokeAsync(Cell);
        }

        protected Task RemoveCell()
        {
            return OnRemoveCell.InvokeAsync(Cell);
        }
    }
}
