using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
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

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        private bool CanView { get; set; }

        private Dashboard Dashboard { get; set; }

        private Dictionary<string, object> DashboardAttributes { get; set; } = new Dictionary<string, object>();

        private List<DashboardCell> DashboardCells { get; } = new List<DashboardCell>();

        private EditContext DashboardEditContext { get; set; } = null;

        [Inject]
        private DashboardService DashboardService { get; set; }

        private bool EditMode { get; set; }

        [CascadingParameter]
        private IModalService ModalService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [CascadingParameter]
        private ISecureContext SecureContext { get; set; }

        private string TextClass { get; set; } = "light";

        protected async override Task OnParametersSetAsync()
        {
            DashboardId ??= 1;
            CanView = await CanViewDashboard();
            Dashboard = await DashboardService.GetDashboardAsync(DashboardId.Value);
            DashboardEditContext = new EditContext(Dashboard);
            LoadDashboardCells();

            DashboardAttributes.Clear();
            RefreshStyles();
        }

        private void CancelChanges()
        {
            LoadDashboardCells();
            StateHasChanged();
        }

        private async Task<bool> CanViewDashboard()
        {
            return (await AuthenticationStateTask).User.CanAccessDashboard(DashboardId.GetValueOrDefault());
        }

        private async void ConfirmDelete()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Delete Dashboard",
                message: "Are you sure you want to delete this dashboard?",
                confirmLabel: "Delete",
                confirmClass: "btn-danger");
            if (!confirmed || !await SecureContext.ValidateUser())
            {
                return;
            }
            await DashboardService.DeleteDashboardAsync(Dashboard);
            NavigationManager.NavigateTo($"/");
        }

        private void LoadDashboardCells()
        {
            DashboardCells.Clear();
            DashboardCells.AddRange(Dashboard.DashboardCells.OrderBy(c => c.Sequence).ThenBy(c => c.DashboardCellId).ToList());
            EditMode = DashboardCells.Count == 0;
        }

        private void RefreshStyles()
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

        private async void SaveChanges()
        {
            if (!DashboardEditContext.Validate() || !await SecureContext.ValidateUser())
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

        private void ToggleEditMode()
        {
            EditMode = !EditMode;
        }
    }
}