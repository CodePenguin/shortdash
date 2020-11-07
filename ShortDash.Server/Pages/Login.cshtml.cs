using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShortDash.Core.Interfaces;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
using ShortDash.Server.Services;

namespace ShortDash.Server.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly DeviceLinkService deviceLinkService;

        public LoginModel(DeviceLinkService deviceLinkService)
        {
            this.deviceLinkService = deviceLinkService;
        }

        public async Task<IActionResult> OnGetAsync(string accessToken)
        {
            var response = await deviceLinkService.ValidateAccessToken(accessToken);
            if (response == null)
            {
                return LocalRedirect("~/");
            }

            deviceLinkService.DeviceLinked(response);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = HttpContext.Request.Host.Value
            };
            var claimsPrincipal = response.DeviceClaims.ToClaimsPrincipal(response.DeviceId);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);
            return LocalRedirect("~/");
        }
    }
}
