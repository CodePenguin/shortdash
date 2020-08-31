using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class FormGeneratorComponentsRepository
    {
        private Dictionary<string, Type> componentMappings = new Dictionary<string, Type>();

        public FormGeneratorComponentsRepository()
        {
            componentMappings = new Dictionary<string, Type>()
                  {
                        { typeof(string).ToString(), typeof(InputText) },
                        { typeof(DateTime).ToString(), typeof(InputDate<>) },
                        { typeof(bool).ToString(), typeof(InputCheckbox) },
                        { typeof(decimal).ToString(), typeof(InputNumber<>) }
                  };
            DefaultComponent = null; // typeof(InputText);
            FormElementComponent = typeof(FormElement);
        }

        public Type DefaultComponent { get; private set; }
        public Type FormElementComponent { get; private set; }

        public void Clear()
        {
            componentMappings.Clear();
        }

        public Type GetComponent(string key)
        {
            componentMappings.TryGetValue(key, out var component);
            return component ?? DefaultComponent;
        }

        public void RegisterComponent(string key, Type component)
        {
            componentMappings.Add(key, component);
        }

        public void RemoveComponent(string key)
        {
            componentMappings.Remove(key);
        }
    }
}