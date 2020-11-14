using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using ShortDash.Server.Services;
using System.Collections.Generic;
using System.Linq;

namespace ShortDash.Server.Components
{
    public class ActionTypeInputSelect : InputBase<string>
    {
        [Parameter]
        public EventCallback<string> OptionSelected { get; set; }

        [Inject]
        private DashboardActionService DashboardActionService { get; set; }

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

        protected override void OnParametersSet()
        {
            var actionTypes = DashboardActionService.GetActionTypes();
            Options.Clear();
            foreach (var actionType in actionTypes)
            {
                var actionAttribute = DashboardActionService.GetActionAttribute(actionType);
                var title = !string.IsNullOrWhiteSpace(actionAttribute.Title) ? actionAttribute.Title : actionType.Name;
                Options.Add(new KeyValuePair<string, string>(actionType.FullName, title));
            }
            Options.Sort((a, b) => a.Value.CompareTo(b.Value));
            if (string.IsNullOrWhiteSpace(CurrentValueAsString))
            {
                CurrentValueAsString = Options.FirstOrDefault().Key;
                OptionSelected.InvokeAsync(CurrentValueAsString);
            }
        }

        protected override bool TryParseValueFromString(string value, out string result, out string validationErrorMessage)
        {
            var selectedItem = Options.FirstOrDefault(p => p.Key == value);
            if (selectedItem.Key != null)
            {
                result = value;
                validationErrorMessage = null;
                return true;
            }

            result = default;
            validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
            return false;
        }
    }
}