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
    public class FormGeneratorComponent<TValue> : OwningComponentBase
    {
        public PropertyInfo[] Properties = Array.Empty<PropertyInfo>();

        private FormGeneratorComponentsRepository _repo;

        public FormGeneratorComponent()
        {
        }

        [Parameter]
        public TValue DataContext { get; set; }

        [Parameter]
        public EventCallback<EditContext> OnValidSubmit { get; set; }

        public bool HasLabel(PropertyInfo propInfo)
        {
            var componentType = _repo.GetComponent(propInfo.PropertyType.ToString());
            var dd = componentType.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            return dd != null && dd.Name.Length > 0;
        }

        public RenderFragment RenderFormElement(PropertyInfo propInfo) => builder =>
        {
            builder.OpenComponent(0, _repo.FormElementComponent);

            builder.AddAttribute(1, nameof(FormElement.FieldIdentifier), propInfo);

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
            Properties = DataContext.GetType().GetProperties();
        }
    }
}