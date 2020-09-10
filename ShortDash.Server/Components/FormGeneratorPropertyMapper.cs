using Microsoft.AspNetCore.Components.Forms;
using ShortDash.Server.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ShortDash.Server.Components
{
    public class FormGeneratorPropertyMapper
    {
        private readonly Dictionary<string, Type> componentMappings = new Dictionary<string, Type>();

        public FormGeneratorPropertyMapper()
        {
            componentMappings = new Dictionary<string, Type>()
                {
                    { typeof(string).ToString(), typeof(InputText) },
                    { typeof(DateTime).ToString(), typeof(InputDate<>) },
                    { typeof(bool).ToString(), typeof(InputCheckbox) },
                    { typeof(decimal).ToString(), typeof(InputNumber<>) },
                    { typeof(int).ToString(), typeof(InputNumber<>) },
                    { typeof(Color).ToString(), typeof(InputColor) }
                };
        }

        public Type GetComponent(string key)
        {
            componentMappings.TryGetValue(key, out var component);
            return component;
        }
    }
}