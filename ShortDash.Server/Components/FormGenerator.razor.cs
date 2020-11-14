using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ShortDash.Server.Components
{
    public partial class FormGenerator : ComponentBase
    {
        private string baseId;
        private PropertyInfo[] properties = Array.Empty<PropertyInfo>();

        [Parameter]
        public EditContext EditContext { get; set; }

        [Parameter]
        public Type FormElementType { get; set; } = typeof(FormElement);

        [Parameter]
        public EventCallback<EditContext> OnValidSubmit { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            baseId = EditContext.Model.GetType().Name;
            properties = GenerateProperties();
        }

        private static int GetDisplayOrder(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
            return displayAttribute?.GetOrder() ?? 10000;
        }

        private PropertyInfo[] GenerateProperties()
        {
            return EditContext.Model
                .GetType()
                .GetProperties()
                .OrderBy(p => GetDisplayOrder(p))
                .ToArray();
        }

        private RenderFragment RenderFormElement(PropertyInfo propInfo)
        {
            return builder =>
                {
                    builder.OpenComponent(0, FormElementType);
                    builder.AddAttribute(1, nameof(FormElement.BaseId), baseId);
                    builder.AddAttribute(2, nameof(FormElement.Model), EditContext.Model);
                    builder.AddAttribute(3, nameof(FormElement.ModelProperty), propInfo);
                    builder.CloseComponent();
                };
        }
    }
}