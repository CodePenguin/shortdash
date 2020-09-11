using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using ShortDash.Server.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Dashboard_View : ComponentBase
    {
        [Parameter]
        public int? DashboardId { get; set; }

        [Inject]
        public DashboardService DashboardService { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        public string TextClass { get; set; } = "light";
        protected Dashboard Dashboard { get; private set; }
        protected Dictionary<string, object> DashboardAttributes { get; private set; } = new Dictionary<string, object>();
        protected EditContext DashboardEditContext { get; private set; } = null;
        protected bool EditMode { get; set; }

        private List<DashboardCell> DashboardCells { get; } = new List<DashboardCell>();

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected void CancelChanges()
        {
            LoadDashboardCells();
            StateHasChanged();
        }

        protected async void ConfirmDelete()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Delete Dashboard",
                message: "Are you sure you want to delete this dashboard?",
                confirmLabel: "Delete",
                confirmClass: "btn-danger");
            if (!confirmed)
            {
                return;
            }
            await DashboardService.DeleteDashboardAsync(Dashboard);
            NavigationManagerService.NavigateTo($"/");
        }

        protected void LoadDashboardCells()
        {
            DashboardCells.Clear();
            DashboardCells.AddRange(Dashboard.DashboardCells.OrderBy(c => c.Sequence).ThenBy(c => c.DashboardCellId).ToList());
            EditMode = DashboardCells.Count == 0;
        }

        protected override async Task OnParametersSetAsync()
        {
            DashboardId ??= 1;
            Dashboard = await DashboardService.GetDashboardAsync(DashboardId.Value);
            DashboardEditContext = new EditContext(Dashboard);
            LoadDashboardCells();

            DashboardAttributes.Clear();
            RefreshStyles();
        }

        protected void RefreshStyles()
        {
            if (Dashboard.BackgroundColor != null)
            {
                DashboardAttributes["style"] = "background-color: " + Dashboard.BackgroundColor?.ToHtmlString();
                TextClass = Dashboard.BackgroundColor?.TextClass();
            }
            else
            {
                DashboardAttributes.Remove("style");
                TextClass = "light";
            }
        }

        protected async void RemoveCell(DashboardCell cell)
        {
            Dashboard.DashboardCells.Remove(cell);
            await DashboardService.DeleteDashboardCellAsync(cell);
            StateHasChanged();
        }

        protected async void SaveChanges()
        {
            if (!DashboardEditContext.Validate())
            {
                return;
            }
            // Update sequences and add new cells to the main list
            for (var i = 0; i < DashboardCells.Count; i++)
            {
                var cell = DashboardCells[i];
                cell.Sequence = i;
                if (!Dashboard.DashboardCells.Contains(cell))
                {
                    Dashboard.DashboardCells.Add(cell);
                }
            }
            // Create a list of cells that have been removed
            var removalList = new List<DashboardCell>();
            foreach (var cell in Dashboard.DashboardCells)
            {
                if (!DashboardCells.Contains(cell))
                {
                    removalList.Add(cell);
                }
            }
            await DashboardService.UpdateDashboardAsync(Dashboard, removalList);
            EditMode = false;
            RefreshStyles();
            StateHasChanged();
        }

        protected void ToggleEditMode()
        {
            EditMode = !EditMode;
        }
    }
}