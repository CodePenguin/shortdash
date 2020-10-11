using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ShortDash.Server.Components;
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

        public enum AuthenticationValidationResult
        {
            Invalid,
            Valid,
            ValidRequiresUpdate
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
            if (!validationResult)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }
        }

        public async Task<bool> ValidatePrincipal(ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
            {
                return true;
            }
            var dashboardDevice = await dashboardService.GetDashboardDeviceAsync(user.Identity.Name);
            if (ValidateClaims(dashboardDevice, user))
            {
                dashboardDevice.LastSeenDateTime = DateTime.Now;
                await dashboardService.UpdateDashboardDeviceAsync(dashboardDevice);
                return true;
            }
            return false;
        }

        private bool ValidateClaims(DashboardDevice dashboardDevice, ClaimsPrincipal user)
        {
            if (dashboardDevice == null)
            {
                return false;
            }
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
