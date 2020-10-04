using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            var requestPath = context.HttpContext.Request.Path.ToString();
            var lastDeviceSyncValue = (from c in user.Claims where c.Type == DeviceClaimTypes.LastDeviceSync select c.Value).FirstOrDefault();
            if (requestPath.StartsWith("/_blazor") || DashboardService.ValidateDeviceSync(lastDeviceSyncValue))
            {
                return;
            }
            var dashboardDevice = await dashboardService.GetDashboardDeviceAsync(user.Identity.Name);
            if (ValidateClaims(dashboardDevice, user))
            {
                dashboardDevice.LastSeenDateTime = DateTime.Now;
                await dashboardService.UpdateDashboardDeviceAsync(dashboardDevice);
                return;
            }
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        private bool ValidateClaims(DashboardDevice dashboardDevice, ClaimsPrincipal user)
        {
            if (dashboardDevice == null)
            {
                return false;
            }
            var ignoreClaimTypes = new[] { ClaimTypes.Name, DeviceClaimTypes.LastDeviceSync };
            var userClaims = new DeviceClaims();
            foreach (var claim in user.Claims)
            {
                if (ignoreClaimTypes.Contains(claim.Type))
                {
                    continue;
                }
                userClaims.Add(claim.Type, claim.Value);
            }
            return dashboardDevice.GetClaimsList().Equals(userClaims);
        }
    }
}
