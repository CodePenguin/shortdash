using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class AuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly DashboardService dashboardService;

        public AuthenticationEvents(DashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var user = context.Principal;
            var lastDeviceSyncValue = (from c in user.Claims where c.Type == DeviceClaimTypes.LastDeviceSync select c.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(lastDeviceSyncValue) || !DashboardService.ValidateDeviceSync(lastDeviceSyncValue))
            {
                var requestPath = context.HttpContext.Request.Path.ToString();
                var allowedPaths = new[] { "/login", "/logout", "devicesync" };
                if (allowedPaths.Contains(requestPath) || requestPath.StartsWith("/_blazor"))
                {
                    return;
                }
                var dashboardDevice = await dashboardService.GetDashboardDeviceAsync(user.Identity.Name);
                if (dashboardDevice == null)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }
                context.HttpContext.Response.Redirect("/devicesync");
            }
        }
    }
}
