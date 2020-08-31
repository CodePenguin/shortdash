using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class FormGeneratorComponentsRepository
    {
        private Dictionary<string, Type> _ComponentDict = new Dictionary<string, Type>();

        public FormGeneratorComponentsRepository()
        {
            _ComponentDict = new Dictionary<string, Type>()
                  {
                        {typeof(string).ToString(), typeof(InputText) },
                        {typeof(DateTime).ToString(), typeof(InputDate<>) },
                        {typeof(bool).ToString(), typeof(InputCheckbox) },
                        {typeof(decimal).ToString(), typeof(InputNumber<>) }
                  };
            _DefaultComponent = null; // typeof(InputText);
            FormElementComponent = typeof(FormElement);
        }

        public Type _DefaultComponent { get; private set; }
        public Type FormElementComponent { get; private set; }

        public void Clear()
        {
            _ComponentDict.Clear();
        }

        public Type GetComponent(string key)
        {
            _ComponentDict.TryGetValue(key, out var component);
            return component ?? _DefaultComponent;
        }

        public void RegisterComponent(string key, Type component)
        {
            _ComponentDict.Add(key, component);
        }

        public void RemoveComponent(string key)
        {
            _ComponentDict.Remove(key);
        }
    }
}