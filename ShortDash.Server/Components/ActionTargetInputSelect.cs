using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class ActionTargetInputSelect : InputBase<string>
    {
        [Parameter]
        public EventCallback<string> OptionSelected { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        private List<KeyValuePair<string, string>> Options { get; } = new List<KeyValuePair<string, string>>();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "select");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "class", "form-control " + CssClass);
            builder.AddAttribute(3, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(4, "onchange", EventCallback.Factory.CreateBinder<string>(this,
                value =>
                {
                    CurrentValueAsString = value;
                    OptionSelected.InvokeAsync(value);
                }, CurrentValueAsString, null));
            var sequence = 5;
            foreach (var pair in Options)
            {
                builder.OpenElement(sequence++, "option");
                builder.AddAttribute(sequence++, "value", pair.Key);
                builder.AddContent(sequence++, pair.Value);
                builder.CloseElement();
            }
            builder.CloseElement();
        }

        protected async override Task OnParametersSetAsync()
        {
            var targets = await DashboardService.GetDashboardActionTargetsAsync();
            Options.Clear();
            foreach (var target in targets)
            {
                Options.Add(new KeyValuePair<string, string>(target.DashboardActionTargetId, $"{target.Name} ({target.Platform})"));
            }
            Options.Sort((a, b) => a.Value.CompareTo(b.Value));
            if (string.IsNullOrWhiteSpace(CurrentValueAsString))
            {
                CurrentValueAsString = Options.FirstOrDefault().Key.ToString();
                await OptionSelected.InvokeAsync(CurrentValueAsString);
            }
        }

        protected override bool TryParseValueFromString(string value, out string result, out string validationErrorMessage)
        {
            if (Options.FirstOrDefault(p => p.Key.Equals(value)).Equals(default(KeyValuePair<string, string>)))
            {
                result = default;
                validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
                return false;
            }

            result = value;
            validationErrorMessage = null;
            return true;
        }
    }
}