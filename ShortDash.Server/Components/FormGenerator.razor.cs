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
    public class FormGeneratorComponent : OwningComponentBase
    {
        public PropertyInfo[] Properties = Array.Empty<PropertyInfo>();

        private FormGeneratorComponentsRepository _repo;

        public FormGeneratorComponent()
        {
        }

        [Parameter]
        public EditContext EditContext { get; set; }

        [Parameter]
        public EventCallback<EditContext> OnValidSubmit { get; set; }

        public RenderFragment RenderFormElement(PropertyInfo propInfo) => builder =>
        {
            builder.OpenComponent(0, _repo.FormElementComponent);
            builder.AddAttribute(1, nameof(FormElement.FieldIdentifier), propInfo);
            builder.AddAttribute(2, nameof(FormElement.InputFieldClasses), "form-control");
            builder.AddAttribute(3, nameof(FormElement.DescriptionFieldClasses), "form-text text-muted");
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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _repo = new FormGeneratorComponentsRepository();
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Properties = GenerateProperties();
        }
    }
}