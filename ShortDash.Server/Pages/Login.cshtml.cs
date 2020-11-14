using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShortDash.Server.Extensions;
using ShortDash.Server.Services;
using System.Threading.Tasks;

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
