using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class FormGenerator : ComponentBase
    {
        private PropertyInfo[] properties = Array.Empty<PropertyInfo>();

        [Parameter]
        public EditContext EditContext { get; set; }

        [Parameter]
        public Type FormElementType { get; set; } = typeof(FormElementComponent);

        [Parameter]
        public EventCallback<EditContext> OnValidSubmit { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
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
                    builder.AddAttribute(1, nameof(FormElementComponent.Model), EditContext.Model);
                    builder.AddAttribute(2, nameof(FormElementComponent.ModelProperty), propInfo);
                    builder.CloseComponent();
                };
        }
    }
}