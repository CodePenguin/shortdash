using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class ConfirmDialog : BaseDialog<bool>
    {
        [Parameter]
        public string Message { get; set; }

        public static async Task<bool> ShowAsync(IModalService modalService, string message, string title = "Confirm", string confirmLabel = "Confirm", string confirmClass = "btn-primary", Dictionary<string, string> additionalParameters = null)
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(Message), message);
            parameters.Add(nameof(OkClass), confirmClass);
            parameters.Add(nameof(OkLabel), confirmLabel);

            if (additionalParameters != null)
            {
                foreach (var parameter in additionalParameters)
                {
                    parameters.Add(parameter.Key, parameter.Value);
                }
            }
            var modal = modalService.Show<ConfirmDialog>(title, parameters);
            var result = await modal.Result;
            return !result.Cancelled;
        }
    }
}