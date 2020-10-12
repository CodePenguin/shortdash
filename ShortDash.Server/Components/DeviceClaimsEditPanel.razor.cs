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

        public EditContext EditContext { get; set; }
        protected List<Dashboard> Dashboards { get; set; }

        [Inject]
        protected DashboardService DashboardService { get; set; }

        protected bool IsLoading => EditContext == null;
        protected DeviceClaimsModel Model { get; set; }

        public void ReadClaims()
        {
            Model.IsAdministrator = DeviceClaims.Find(c => c.Type == ClaimTypes.Role && c.Value.Equals(Roles.Administrator)) != null;

            foreach (var dashboard in Dashboards)
            {
                var claim = DeviceClaims.Find(c => c.Type == Claims.DashboardAccess(dashboard.DashboardId));
                Model.DashboardAccess[dashboard.DashboardId] = claim != null;
            }
        }

        public void UpdateClaims()
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

        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            Model = new DeviceClaimsModel();
            Dashboards = await DashboardService.GetDashboardsAsync();
            ReadClaims();

            EditContext = new EditContext(Model);
            EditContext.OnFieldChanged += OnFieldChanged;
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
