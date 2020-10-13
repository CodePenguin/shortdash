using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using ShortDash.Core.Services;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public sealed class SecureQrCode : ComponentBase
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public int Height { get; set; } = 150;

        [Parameter]
        public string Value { get; set; }

        [Parameter]
        public int Width { get; set; } = 150;

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [CascadingParameter]
        private ISecureContext SecureContext { get; set; }

        private string UniqueId { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "id", UniqueId);
            if (AdditionalAttributes != null)
            {
                builder.AddMultipleAttributes(2, AdditionalAttributes);
            }
            builder.CloseElement();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            await JSRuntime.InvokeVoidAsync("secureContext.setSecureQRCode", UniqueId, SecureContext.Encrypt(Value), Width, Height);
        }

        protected override void OnParametersSet()
        {
            UniqueId = Guid.NewGuid().ToString("N");
        }
    }
}