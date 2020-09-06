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
        public PropertyInfo[] Properties = Array.Empty<PropertyInfo>();

        [Parameter]
        public EditContext EditContext { get; set; }

        [Parameter]
        public Type FormElementType { get; set; } = typeof(FormElementComponent);

        [Parameter]
        public EventCallback<EditContext> OnValidSubmit { get; set; }

        public RenderFragment RenderFormElement(PropertyInfo propInfo) => builder =>
        {
            builder.OpenComponent(0, FormElementType);
            builder.AddAttribute(1, nameof(FormElementComponent.Model), EditContext.Model);
            builder.AddAttribute(2, nameof(FormElementComponent.ModelProperty), propInfo);
            builder.CloseComponent();
        };

        protected static int GetDisplayOrder(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
            return displayAttribute?.GetOrder() ?? 10000;
        }

        protected PropertyInfo[] GenerateProperties()
        {
            return EditContext.Model
                .GetType()
                .GetProperties()
                .OrderBy(p => GetDisplayOrder(p))
                .ToArray();
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Properties = GenerateProperties();
        }
    }
}