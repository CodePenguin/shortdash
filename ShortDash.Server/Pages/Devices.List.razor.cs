using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Devices_List : PageBase
    {
        private List<DashboardDevice> DashboardDevices { get; } = new List<DashboardDevice>();

        protected async override Task OnParametersSetAsync()
        {
            var list = await DashboardService.GetDashboardDevicesAsync();
            DashboardDevices.Clear();
            DashboardDevices.AddRange(list.OrderBy(d => d.Name).ToList());
        }

        private void LinkDevice()
        {
            NavigationManager.NavigateTo($"/devices/link");
        }
    }
}