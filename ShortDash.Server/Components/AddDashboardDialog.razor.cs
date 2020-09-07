using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class AddDashboardDialog : ComponentBase
    {
        protected SelectedItem Selected { get; } = new SelectedItem();

        public static Task<ModalResult> ShowAsync(IModalService modalService)
        {
            var modal = modalService.Show<AddDashboardDialog>("Add a dashboard");
            return modal.Result;
        }

        protected class SelectedItem
        {
            public string Value { get; set; }
        }
    }
}