using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashGroupActionDialog : ComponentBase
    {
        [Parameter]
        public DashboardAction DashboardAction { get; set; }

        private string TextClass => DashboardAction.BackgroundColor.TextClass();
        private string BackgroundColorCode => DashboardAction.BackgroundColor.ToHtmlString();

        [CascadingParameter]
        private BlazoredModalInstance BlazoredModal { get; set; }

        public static Task ShowAsync(IModalService modalService, DashboardAction dashboardAction)
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(DashboardAction), dashboardAction);
            var modal = modalService.Show<DashGroupActionDialog>("", parameters);
            return modal.Result;
        }

        private Task CloseDialog()
        {
            return BlazoredModal.Close();
        }
    }
}