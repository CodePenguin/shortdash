using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public static class DeviceClaimTypes
    {
        public const string AdministratorRole = "Administrator";
        public const string LastDeviceSync = "LastDeviceSync";

        public static string DashboardAccess(int dashboardId)
        {
            return $"DashboardAccess_{dashboardId}";
        }
    }
}
