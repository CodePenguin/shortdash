using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ShortDash.Server.Components
{
    public class FormGeneratorComponentsRepository
    {
        private readonly Dictionary<string, Type> componentMappings = new Dictionary<string, Type>();

        public FormGeneratorComponentsRepository()
        {
            componentMappings = new Dictionary<string, Type>()
                {
                    { typeof(string).ToString(), typeof(InputText) },
                    { typeof(DateTime).ToString(), typeof(InputDate<>) },
                    { typeof(bool).ToString(), typeof(InputCheckbox) },
                    { typeof(decimal).ToString(), typeof(InputNumber<>) },
                    { typeof(Color).ToString(), typeof(InputColor) }
                };
            DefaultComponent = null;
            FormElementComponent = typeof(FormElement);
        }

        public Type DefaultComponent { get; private set; }
        public Type FormElementComponent { get; private set; }

        public Type GetComponent(string key)
        {
            componentMappings.TryGetValue(key, out var component);
            return component ?? DefaultComponent;
        }
    }
}