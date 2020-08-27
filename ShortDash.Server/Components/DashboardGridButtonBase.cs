using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ShortDash.Server.Components
{
    public abstract class DashboardGridButtonBase<TParameterType> : ComponentBase where TParameterType : DashboardGridButtonParameters
    {

        [Parameter]
        public DashboardCell Cell { get; set; }

        protected TParameterType Parameters { get; private set; }

        protected Dictionary<string, object> ButtonAttributes;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Parameters = JsonSerializer.Deserialize<TParameterType>(Cell.DashboardAction.Parameters);
            ButtonAttributes = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Cell.BackgroundColor))
                ButtonAttributes.Add("style", "background-colorX: " + Cell.BackgroundColor);
        }
    }
}
