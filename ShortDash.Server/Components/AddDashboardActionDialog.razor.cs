using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class AddDashboardActionDialog : ComponentBase
    {
        private List<DashboardAction> AvailableActions { get; } = new List<DashboardAction>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        private SelectedItem Selected { get; } = new SelectedItem();

        public static Task<ModalResult> ShowAsync(IModalService modalService)
        {
            var modal = modalService.Show<AddDashboardActionDialog>("Add an action");
            return modal.Result;
        }

        protected async override Task OnParametersSetAsync()
        {
            AvailableActions.AddRange(await DashboardService.GetDashboardActionsAsync());
            Selected.Value = AvailableActions.FirstOrDefault()?.DashboardActionId.ToString();
        }

        private int ParseResult()
        {
            return BindConverter.TryConvertToInt(Selected.Value, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        private class SelectedItem
        {
            public string Value { get; set; }
        }
    }
}