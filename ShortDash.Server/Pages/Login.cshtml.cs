using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShortDash.Core.Interfaces;
using ShortDash.Server.Data;
using ShortDash.Server.Services;

namespace ShortDash.Server.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly DashboardService dashboardService;
        private readonly DeviceLinkService deviceLinkService;
        private readonly IEncryptedChannelService encryptedChannelService;

        public LoginModel(DashboardService dashboardService, IEncryptedChannelService encryptedChannelService, DeviceLinkService deviceLinkService)
        {
            this.dashboardService = dashboardService;
            this.encryptedChannelService = encryptedChannelService;
            this.deviceLinkService = deviceLinkService;
        }

        public async Task<IActionResult> OnGetAsync(string accessToken)
        {
            // TODO: NEED TO CALL THE DEVICE LINK SERVICE HERE TO DO THE VALIDATION AND NOTIFICATION!!!!
            if (accessToken == null || !encryptedChannelService.TryLocalDecryptVerify<DashboardDevice>(accessToken, out var tokenDevice))
            {
                return LocalRedirect("~/");
            }

            var dashboardDevice = await dashboardService.GetDashboardDeviceAsync(tokenDevice.DashboardDeviceId);
            if (dashboardDevice == null)
            {
                return LocalRedirect("~/");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dashboardDevice.DashboardDeviceId)
            };
            foreach (var claim in dashboardDevice.GetClaimsArray())
            {
                claims.Add(claim);
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = HttpContext.Request.Host.Value
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            return LocalRedirect("~/");
        }
    }
}
