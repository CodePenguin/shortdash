using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using System.Text.Json;

namespace ShortDash.Server.Components
{
    public abstract class DashboardGridButtonBase<TParameterType> : ComponentBase where TParameterType : DashboardGridButtonParameters
    {

        [Parameter]
        public DashboardCell Cell { get; set; }

        protected TParameterType Parameters { get; private set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Parameters = JsonSerializer.Deserialize<TParameterType>(Cell.DashboardAction.Parameters);
        }
    }
}
