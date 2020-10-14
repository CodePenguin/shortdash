using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Components;
using ShortDash.Server.Services;

namespace ShortDash.Server.Pages
{
    public abstract class PageBase : ComponentBase
    {
        [Inject]
        protected DashboardService DashboardService { get; set; }

        [CascadingParameter]
        protected IModalService ModalService { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected NavMenuManager NavMenuManager { get; set; }

        [CascadingParameter]
        protected ISecureContext SecureContext { get; set; }

        [Inject]
        protected IToastService ToastService { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            NavMenuManager.Reset();
        }
    }
}
