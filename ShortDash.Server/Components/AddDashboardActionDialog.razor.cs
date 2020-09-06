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
        protected List<DashboardAction> AvailableActions { get; } = new List<DashboardAction>();

        [Inject]
        protected DashboardService DashboardService { get; set; }

        protected SelectedItem Selected { get; } = new SelectedItem();

        public static Task<ModalResult> ShowAsync(IModalService modalService)
        {
            var modal = modalService.Show<AddDashboardActionDialog>("Add an action");
            return modal.Result;
        }

        protected override async Task OnParametersSetAsync()
        {
            AvailableActions.AddRange(await DashboardService.GetDashboardActionsAsync());
        }

        protected int ParseResult()
        {
            return BindConverter.TryConvertToInt(Selected.Value, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        protected class SelectedItem
        {
            public string Value { get; set; }
        }
    }
}