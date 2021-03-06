﻿using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class BaseDialog<TResult> : ComponentBase
    {
        [Parameter]
        public string CancelClass { get; set; } = "btn-secondary";

        [Parameter]
        public string CancelLabel { get; set; } = "Cancel";

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string HeaderClass { get; set; } = "";

        [Parameter]
        public string HeaderIcon { get; set; } = "";

        [Parameter]
        public string OkClass { get; set; } = "btn-primary";

        [Parameter]
        public string OkLabel { get; set; } = "OK";

        [Parameter]
        public TResult Result { get; set; } = default;

        [CascadingParameter]
        private BlazoredModalInstance BlazoredModal { get; set; }

        private Task CancelClick()
        {
            return BlazoredModal.Cancel();
        }

        private Task OkClick()
        {
            return BlazoredModal.Close(ModalResult.Ok(Result));
        }
    }
}