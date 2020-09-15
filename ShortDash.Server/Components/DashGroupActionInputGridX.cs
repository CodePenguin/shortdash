using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using ShortDash.Core.Plugins;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class DashGroupActionInputGridX : InputBase<List<DashboardAction>>
    {
        [Parameter]
        public List<DashboardAction> Actions { get; set; } = new List<DashboardAction>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "div");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "class", "form-control " + CssClass);
            var sequence = 3;
            foreach (var action in Actions)
            {
                builder.OpenElement(sequence++, "span");
                builder.AddContent(sequence++, action.Label);
                builder.CloseElement();
            }
            builder.CloseElement();
        }

        protected override async Task OnParametersSetAsync()
        {
            var dashboardActionId = 10;
            var dashboardAction = await DashboardService.GetDashboardActionAsync(dashboardActionId);
            Actions.Clear();
            foreach (var action in dashboardAction.DashboardSubActionChildren)
            {
                Actions.Add(action.DashboardActionChild);
            }
            Actions.Sort((a, b) => a.Label.CompareTo(b.Label));
        }

        protected override bool TryParseValueFromString(string value, out List<DashboardAction> result, out string validationErrorMessage)
        {
            result = Actions;
            validationErrorMessage = null;
            return true;
        }
    }
}