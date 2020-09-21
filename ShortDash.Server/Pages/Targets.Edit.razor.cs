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
    public partial class Targets_Edit : ComponentBase
    {
        [Parameter]
        public string DashboardActionTargetId { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        protected DashboardActionTarget DashboardActionTarget { get; set; }

        protected bool IsLoading => TargetEditContext == null;

        protected EditContext TargetEditContext { get; private set; } = null;

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected void CancelChanges()
        {
            NavigationManagerService.NavigateTo("/targets");
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
            NavigationManagerService.NavigateTo("/targets");
        }

        protected override async Task OnParametersSetAsync()
        {
            TargetEditContext = null;
            if (!string.IsNullOrWhiteSpace(DashboardActionTargetId))
            {
                if (DashboardActionTargetId == DashboardActionTarget.ServerTargetId)
                {
                    NavigationManagerService.NavigateTo("/targets");
                    return;
                }

                await LoadDashboardActionTarget();
            }
            else
            {
                await Task.Run(() => NewDashboardActionTarget());
            }
            TargetEditContext = new EditContext(DashboardActionTarget);
        }

        protected async void SaveChanges()
        {
            if (!TargetEditContext.Validate())
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
            NavigationManagerService.NavigateTo("/targets");
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
