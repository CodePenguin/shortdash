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
    public sealed class SecureText : ComponentBase
    {
        private string lastSentValue;

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public string Value { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [CascadingParameter]
        private ISecureContext SecureContext { get; set; }

        private string UniqueId { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "span");
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
            if (Value != lastSentValue)
            {
                lastSentValue = Value;
                await JSRuntime.InvokeVoidAsync("secureContext.setSecureElementValue", UniqueId, SecureContext.Encrypt(Value));
            }
        }

        protected override void OnParametersSet()
        {
            UniqueId = Guid.NewGuid().ToString("N");
        }
    }
}