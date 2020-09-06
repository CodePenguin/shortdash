using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class ConfirmDialog : ComponentBase
    {
        [Parameter]
        public string ConfirmClass { get; set; } = "btn-secondary";

        [Parameter]
        public string ConfirmLabel { get; set; } = "Confirm";

        [Parameter]
        public string Message { get; set; }

        public static async Task<bool> ShowAsync(IModalService modalService, string message, string title = "Confirm", string confirmLabel = "Confirm", string confirmClass = "btn-primary")
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(Message), message);
            parameters.Add(nameof(ConfirmClass), confirmClass);
            parameters.Add(nameof(ConfirmLabel), confirmLabel);
            var modal = modalService.Show<ConfirmDialog>(title, parameters);
            var result = await modal.Result;
            return !result.Cancelled;
        }
    }
}