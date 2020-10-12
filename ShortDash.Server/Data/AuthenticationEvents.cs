using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ShortDash.Server.Components;
using ShortDash.Server.Extensions;
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

        public async override Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var user = context.Principal;
            var requestPath = context.HttpContext.Request.Path.ToString();
            if (requestPath.StartsWith("/_blazor"))
            {
                return;
            }
            var validationResult = await ValidatePrincipal(user);
            if (!validationResult.IsValid)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }
            if (validationResult.RequiresUpdate)
            {
                var claimsPrincipal = validationResult.Claims.ToClaimsPrincipal(validationResult.DeviceId);
                context.ReplacePrincipal(claimsPrincipal);
                context.ShouldRenew = true;
            }
        }

        public async Task<AuthenticationValidationResult> ValidatePrincipal(ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
            {
                return new AuthenticationValidationResult();
            }
            var dashboardDevice = await dashboardService.GetDashboardDeviceAsync(user.Identity.Name);
            if (dashboardDevice == null)
            {
                return new AuthenticationValidationResult();
            }
            if (ValidateClaims(dashboardDevice, user))
            {
                dashboardDevice.LastSeenDateTime = DateTime.Now;
                await dashboardService.UpdateDashboardDeviceAsync(dashboardDevice);
                return new AuthenticationValidationResult(dashboardDevice.DashboardDeviceId);
            }
            return new AuthenticationValidationResult(dashboardDevice.DashboardDeviceId, dashboardDevice.GetClaimsList());
        }

        private bool ValidateClaims(DashboardDevice dashboardDevice, ClaimsPrincipal user)
        {
            var ignoreClaimTypes = new[] { ClaimTypes.Name };
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
