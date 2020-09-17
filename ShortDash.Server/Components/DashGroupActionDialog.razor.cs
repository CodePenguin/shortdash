using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashGroupActionDialog : ComponentBase
    {
        public string BackgroundColorCode => DashboardAction.BackgroundColor?.ToHtmlString();

        [Parameter]
        public DashboardAction DashboardAction { get; set; }

        private string TextClass => DashboardAction.BackgroundColor?.TextClass();

        [CascadingParameter]
        protected BlazoredModalInstance BlazoredModal { get; set; }

        public static Task ShowAsync(IModalService modalService, DashboardAction dashboardAction)
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(DashboardAction), dashboardAction);
            var modal = modalService.Show<DashGroupActionDialog>("", parameters);
            return modal.Result;
        }

        protected Task CloseDialog()
        {
            return BlazoredModal.Close();
        }
    }
}