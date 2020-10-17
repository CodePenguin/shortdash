using Blazored.Modal.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class DangerConfirmDialog : ConfirmDialog
    {
        public static Task<bool> ShowAsync(IModalService modalService, string message, string title, string confirmLabel)
        {
            var additionalParameters = new Dictionary<string, string>
            {
                { nameof(HeaderClass), "bg-danger text-light" },
                { nameof(HeaderIcon), "fas fa-exclamation-triangle" }
            };
            return ShowAsync(modalService, message, title, confirmLabel, "btn-danger", additionalParameters: additionalParameters);
        }
    }
}
