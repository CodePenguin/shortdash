using ShortDash.Server.Data;
using System;
using System.Security.Claims;

namespace ShortDash.Server.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool CanAccessDashboard(this ClaimsPrincipal user, int dashboardId)
        {
            return user.IsInRole(DeviceClaimTypes.AdministratorRole) || user.HasClaim(c => c.Type == DeviceClaimTypes.DashboardAccess(dashboardId) && c.Value.Equals("VIEW"));
        }
    }
}
