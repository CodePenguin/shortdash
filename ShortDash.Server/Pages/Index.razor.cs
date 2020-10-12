using Microsoft.AspNetCore.Components;
using ShortDash.Server.Services;
using System;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        private AdminAccessCodeService AdministratorAccessCodeService { get; set; }

        private bool IsFirstRun { get; set; }
        private bool ShowAdminDeviceLinkMessage { get; set; }

        protected void OnCompletedEvent(object sender, EventArgs eventArgs)
        {
            IsFirstRun = false;
            ShowAdminDeviceLinkMessage = true;
            StateHasChanged();
        }

        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            ShowAdminDeviceLinkMessage = false;
            IsFirstRun = !await AdministratorAccessCodeService.IsInitialized();
        }
    }
}
