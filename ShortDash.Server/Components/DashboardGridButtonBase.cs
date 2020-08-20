using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using System.Collections.Generic;
using System.Text.Json;

namespace ShortDash.Server.Components
{
    public abstract class DashboardButtonBase : ComponentBase
    {
        [Parameter]
        public GridCell Cell { get; set; }

        protected Dictionary<string, object> ButtonAttributes;
        protected abstract void Click();

        protected T GetParameters<T>() 
        {
            return JsonSerializer.Deserialize<T>(Cell.Parameters);
        }

        protected override void OnInitialized()
        {
            ButtonAttributes = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Cell.BackgroundColor))
                ButtonAttributes.Add("style", "background-color: " + Cell.BackgroundColor);
        }
    }
}
