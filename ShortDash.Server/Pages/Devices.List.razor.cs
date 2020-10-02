using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Devices_List : ComponentBase
    {
        protected List<DashboardDevice> DashboardDevices { get; } = new List<DashboardDevice>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected void LinkDevice()
        {
            NavigationManagerService.NavigateTo($"/devices/link");
        }

        protected override async Task OnParametersSetAsync()
        {
            var list = await DashboardService.GetDashboardDevicesAsync();
            DashboardDevices.Clear();
            DashboardDevices.AddRange(list.OrderBy(d => d.Name).ToList());
        }
    }
}