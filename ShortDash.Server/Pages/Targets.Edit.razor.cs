using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;

namespace ShortDash.Server.Pages
{
    public partial class Targets_Edit : ComponentBase
    {
        [Parameter]
        public int DashboardActionTargetId { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        protected DashboardActionTarget DashboardActionTarget { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected void CancelChanges()
        {
            NavigationManagerService.NavigateTo($"/targets");
        }

        protected async void ConfirmDelete()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Delete Target",
                message: "Are you sure you want to delete this target?",
                confirmLabel: "Delete",
                confirmClass: "btn-danger");
            if (!confirmed)
            {
                return;
            }
            await DashboardService.DeleteDashboardActionTargetAsync(DashboardActionTarget);
            NavigationManagerService.NavigateTo($"/targets");
        }

        protected override async Task OnParametersSetAsync()
        {
            if (DashboardActionTargetId > 0)
            {
                await LoadDashboardActionTarget();
            }
            else
            {
                await Task.Run(() => NewDashboardActionTarget());
            }
        }

        protected async void SaveChanges()
        {
            if (DashboardActionTarget.DashboardActionTargetId == 0)
            {
                await DashboardService.AddDashboardActionTargetAsync(DashboardActionTarget);
            }
            else
            {
                await DashboardService.UpdateDashboardActionTargetAsync(DashboardActionTarget);
            }
            NavigationManagerService.NavigateTo($"/targets");
        }

        private async Task LoadDashboardActionTarget()
        {
            DashboardActionTarget = await DashboardService.GetDashboardActionTargetAsync(DashboardActionTargetId);
        }

        private void NewDashboardActionTarget()
        {
            DashboardActionTarget = new DashboardActionTarget();
        }
    }
}
