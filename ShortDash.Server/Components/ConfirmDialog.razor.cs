using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class ConfirmDialogComponent : ComponentBase
    {
        public static async Task<bool> ShowAsync(IModalService modalService, string message, string title = "Confirm", string confirmLabel = "Confirm", string confirmClass = "btn-primary")
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(ConfirmDialog.Message), message);
            parameters.Add(nameof(ConfirmDialog.ConfirmClass), confirmClass);
            parameters.Add(nameof(ConfirmDialog.ConfirmLabel), confirmLabel);
            var modal = modalService.Show<ConfirmDialog>(title, parameters);
            var result = await modal.Result;
            return !result.Cancelled;
        }
    }
}