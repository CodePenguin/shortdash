using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;

namespace ShortDash.Server.Pages
{
    public partial class Targets_Edit : PageBase
    {
        [Parameter]
        public string DashboardActionTargetId { get; set; }

        private DashboardActionTarget DashboardActionTarget { get; set; }

        private bool IsLoading => DashboardActionTarget == null;

        protected async override Task OnParametersSetAsync()
        {
            DashboardActionTarget = null;
            if (!string.IsNullOrWhiteSpace(DashboardActionTargetId))
            {
                if (DashboardActionTargetId == DashboardActionTarget.ServerTargetId)
                {
                    NavigationManager.NavigateTo("/targets");
                    return;
                }

                await LoadDashboardActionTarget();
            }
            else
            {
                await Task.Run(() => NewDashboardActionTarget());
            }
        }

        private void CancelChanges()
        {
            NavigationManager.NavigateTo("/targets");
        }

        private async void ConfirmDelete()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Delete Target",
                message: "Are you sure you want to delete this target?",
                confirmLabel: "Delete",
                confirmClass: "btn-danger");
            if (!confirmed || !await SecureContext.ValidateUserAsync())
            {
                return;
            }
            await DashboardService.DeleteDashboardActionTargetAsync(DashboardActionTarget);
            NavigationManager.NavigateTo("/targets");
        }

        private async Task LoadDashboardActionTarget()
        {
            DashboardActionTarget = await DashboardService.GetDashboardActionTargetAsync(DashboardActionTargetId);
        }

        private void NewDashboardActionTarget()
        {
            DashboardActionTarget = new DashboardActionTarget();
        }

        private async void SaveChanges()
        {
            if (!await SecureContext.ValidateUserAsync())
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(DashboardActionTarget.DashboardActionTargetId))
            {
                await DashboardService.AddDashboardActionTargetAsync(DashboardActionTarget);
            }
            else
            {
                await DashboardService.UpdateDashboardActionTargetAsync(DashboardActionTarget);
            }
            NavigationManager.NavigateTo("/targets");
        }
    }
}
