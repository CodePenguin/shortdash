using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public static class Claims
    {
        public static string DashboardAccess(int dashboardId)
        {
            return $"DashboardAccess_{dashboardId}";
        }
    }
}
