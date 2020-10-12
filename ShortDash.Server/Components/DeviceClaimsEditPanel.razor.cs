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
        public DeviceClaims Claims { get; set; }

        public EditContext ClaimsEditContext { get; set; }

        protected List<Dashboard> Dashboards { get; set; }

        [Inject]
        protected DashboardService DashboardService { get; set; }

        protected bool IsLoading => ClaimsEditContext == null;
        protected DeviceClaimsModel Model { get; set; }

        public void ReadClaims()
        {
            Model.IsAdministrator = Claims.Find(c => c.Type == ClaimTypes.Role && c.Value.Equals(DeviceClaimTypes.AdministratorRole)) != null;

            foreach (var dashboard in Dashboards)
            {
                var claim = Claims.Find(c => c.Type == DeviceClaimTypes.DashboardAccess(dashboard.DashboardId));
                Model.DashboardAccess[dashboard.DashboardId] = claim != null;
            }
        }

        public void UpdateClaims()
        {
            Claims.Clear();
            if (Model.IsAdministrator)
            {
                Claims.Add(new DeviceClaim(ClaimTypes.Role, DeviceClaimTypes.AdministratorRole));
            }
            else
            {
                foreach (var dashboard in Dashboards)
                {
                    if (!Model.DashboardAccess.TryGetValue(dashboard.DashboardId, out var canAccess) || !canAccess)
                    {
                        continue;
                    }
                    Claims.Add(new DeviceClaim(DeviceClaimTypes.DashboardAccess(dashboard.DashboardId), "VIEW"));
                }
            }
        }

        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            Model = new DeviceClaimsModel();
            Dashboards = await DashboardService.GetDashboardsAsync();
            ReadClaims();

            ClaimsEditContext = new EditContext(Model);
            ClaimsEditContext.OnFieldChanged += OnFieldChanged;
        }

        protected void ToggleDashboardAccess(int dashboardId)
        {
            Model.DashboardAccess[dashboardId] = !Model.DashboardAccess[dashboardId];
            UpdateClaims();
        }

        private void OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            UpdateClaims();
        }

        protected class DeviceClaimsModel
        {
            public Dictionary<int, bool> DashboardAccess { get; } = new Dictionary<int, bool>();
            public bool IsAdministrator { get; set; }
        }
    }
}
