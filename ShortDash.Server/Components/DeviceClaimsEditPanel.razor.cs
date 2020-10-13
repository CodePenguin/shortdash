using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DeviceClaimsEditPanel : ComponentBase
    {
        [Parameter]
        public DeviceClaims DeviceClaims { get; set; }

        private List<Dashboard> Dashboards { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        private EditContext EditContext { get; set; }
        private bool IsLoading => EditContext == null;
        private DeviceClaimsModel Model { get; set; }

        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            Model = new DeviceClaimsModel();
            Dashboards = await DashboardService.GetDashboardsAsync();
            ReadClaims();

            EditContext = new EditContext(Model);
            EditContext.OnFieldChanged += OnFieldChanged;
        }

        private void OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            UpdateClaims();
        }

        private void ReadClaims()
        {
            Model.IsAdministrator = DeviceClaims.Find(c => c.Type == ClaimTypes.Role && c.Value.Equals(Roles.Administrator)) != null;

            foreach (var dashboard in Dashboards)
            {
                var claim = DeviceClaims.Find(c => c.Type == Claims.DashboardAccess(dashboard.DashboardId));
                Model.DashboardAccess[dashboard.DashboardId] = claim != null;
            }
        }

        private void ToggleDashboardAccess(int dashboardId)
        {
            Model.DashboardAccess[dashboardId] = !Model.DashboardAccess[dashboardId];
            UpdateClaims();
        }

        private void UpdateClaims()
        {
            DeviceClaims.Clear();
            if (Model.IsAdministrator)
            {
                DeviceClaims.Add(new DeviceClaim(ClaimTypes.Role, Roles.Administrator));
            }
            else
            {
                foreach (var dashboard in Dashboards)
                {
                    if (!Model.DashboardAccess.TryGetValue(dashboard.DashboardId, out var canAccess) || !canAccess)
                    {
                        continue;
                    }
                    DeviceClaims.Add(new DeviceClaim(Claims.DashboardAccess(dashboard.DashboardId), "VIEW"));
                }
            }
        }

        private class DeviceClaimsModel
        {
            public Dictionary<int, bool> DashboardAccess { get; } = new Dictionary<int, bool>();
            public bool IsAdministrator { get; set; }
        }
    }
}
