using Microsoft.AspNetCore.Authorization;

namespace ShortDash.Server.Data
{
    public static class Policies
    {
        public const string EditActions = "EditActions";
        public const string EditDashboards = "EditDashboards";
        public const string EditDevices = "EditDevices";
        public const string EditTargets = "EditTargets";
        public const string ViewDashboards = "ViewDashboards";

        public static AuthorizationPolicy IsAdminPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(Roles.Administrator)
                .Build();
        }

        public static AuthorizationPolicy IsUserPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        }
    }
}
