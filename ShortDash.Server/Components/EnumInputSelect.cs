using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class EnumInputSelect<T> : InputBase<T>
    {
        [Parameter]
        public EventCallback<string> OptionSelected { get; set; }

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

        protected async override Task OnInitializedAsync()
        {
            var values = typeof(T).GetEnumValues();
            Options.Clear();
            foreach (var value in values)
            {
                Options.Add(new KeyValuePair<string, string>(value.ToString(), typeof(T).GetEnumName(value)));
            }
            Options.Sort((a, b) => a.Value.CompareTo(b.Value));
            if (string.IsNullOrWhiteSpace(CurrentValueAsString))
            {
                CurrentValueAsString = Options.FirstOrDefault().Key.ToString();
                await OptionSelected.InvokeAsync(CurrentValueAsString);
            }
        }

        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            var convertedValue = (T)Enum.Parse(typeof(T), value);
            if (!Enum.IsDefined(typeof(T), convertedValue))
            {
                result = default;
                validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
                return false;
            }

            result = convertedValue;
            validationErrorMessage = null;
            return true;
        }
    }
}
