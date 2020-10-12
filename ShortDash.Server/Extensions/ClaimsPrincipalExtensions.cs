using Microsoft.AspNetCore.Authentication.Cookies;
using ShortDash.Server.Data;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ShortDash.Server.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool CanAccessDashboard(this ClaimsPrincipal user, int dashboardId)
        {
            return user.IsInRole(Roles.Administrator) || user.HasClaim(c => c.Type == Claims.DashboardAccess(dashboardId) && c.Value.Equals("VIEW"));
        }

        public static ClaimsPrincipal ToClaimsPrincipal(this DeviceClaims deviceClaims, string deviceId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, deviceId)
            };
            foreach (var claim in deviceClaims)
            {
                claims.Add(new Claim(claim.Type, claim.Value));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
