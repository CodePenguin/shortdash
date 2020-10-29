using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
using ShortDash.Server.Services;
using ShortDash.Server.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Dashboard_View : PageBase
    {
        [Parameter]
        public int? DashboardId { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        private bool CanEdit { get; set; }
        private bool CanView { get; set; }
        private Dashboard Dashboard { get; set; }
        private Dictionary<string, object> DashboardAttributes { get; set; } = new Dictionary<string, object>();
        private List<DashboardCell> DashboardCells { get; } = new List<DashboardCell>();
        private EditContext DashboardEditContext { get; set; } = null;
        private bool EditMode { get; set; }
        private bool IsLoading { get; set; }
        private string TextClass { get; set; } = "light";

        protected async override Task OnParametersSetAsync()
        {
            IsLoading = true;
            DashboardId ??= 1;
            CanEdit = await SecureContext.AuthorizeAsync(Policies.EditDashboards);
            CanView = await CanViewDashboard();
            Dashboard = await DashboardService.GetDashboardAsync(DashboardId.Value);
            if (Dashboard == null)
            {
                IsLoading = false;
                return;
            }
            DashboardEditContext = new EditContext(Dashboard);
            LoadDashboardCells();

            if (CanEdit)
            {
                NavMenuManager.AddMenuButton("far fa-edit", EditButtonClickEvent);
            }

            DashboardAttributes.Clear();
            RefreshStyles();
            IsLoading = false;
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
            if (!confirmed || !await SecureContext.ValidateUserAsync())
            {
                return;
            }
            await DashboardService.DeleteDashboardAsync(Dashboard);
            NavigationManager.NavigateTo($"/");
        }

        private void EditButtonClickEvent()
        {
            if (!EditMode)
            {
                EditMode = true;
            }
            else
            {
                CancelChanges();
            }
        }

        private void LoadDashboardCells()
        {
            DashboardCells.Clear();
            DashboardCells.AddRange(Dashboard.DashboardCells.OrderBy(c => c.Sequence).ThenBy(c => c.DashboardCellId).ToList());
            EditMode = CanEdit && (DashboardCells.Count == 0);
        }

        private void RefreshStyles()
        {
            NavMenuManager.Subtitle = Dashboard.Name;
            if (Dashboard.BackgroundColor != null)
            {
                DashboardAttributes["style"] = "background-color: " + Dashboard.BackgroundColor.ToHtmlString();
                TextClass = Dashboard.BackgroundColor.TextClass();
            }
            else
            {
                DashboardAttributes.Remove("style");
                TextClass = "light";
            }
        }

        private async void SaveChanges()
        {
            if (!DashboardEditContext.Validate() || !await SecureContext.ValidateUserAsync())
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
    }
}