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
            builder.AddAttribute(2, nameof(FormElement.DefaultFieldClasses), new List<string> { "form-control" });
            builder.CloseComponent();
        };

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _repo = new FormGeneratorComponentsRepository();
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Properties = EditContext.Model.GetType().GetProperties();
        }
    }
}