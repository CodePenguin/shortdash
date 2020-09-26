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
    public class SecureInputText : InputBase<string>
    {
        private DotNetObjectReference<SecureInputText> objectReference;

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [CascadingParameter(Name = "SecureContext")]
        protected ISecureContext SecureContext { get; set; }

        protected string UniqueId { get; private set; }

        [JSInvokable]
        public void OnClientChanged(string encryptedValue)
        {
            DecryptConvertValue(encryptedValue);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "id", UniqueId);
            if (!AdditionalAttributes.ContainsKey("type"))
            {
                builder.AddAttribute(2, "type", "text");
            }
            builder.AddMultipleAttributes(3, AdditionalAttributes);
            builder.CloseElement();
        }

        protected void DecryptConvertValue(string encryptedValue)
        {
            CurrentValueAsString = SecureContext.Decrypt(encryptedValue);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                objectReference?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected string EncryptedCurrentValue()
        {
            return SecureContext.Encrypt(Value);
        }

        protected override string FormatValueAsString(string value)
        {
            return value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("secureContext.initSecureInputText", UniqueId, objectReference, EncryptedCurrentValue());
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            objectReference = DotNetObjectReference.Create(this);
        }

        protected override void OnParametersSet()
        {
            UniqueId = Guid.NewGuid().ToString("N");
        }

        protected override bool TryParseValueFromString(string value, out string result, out string validationErrorMessage)
        {
            result = value;
            validationErrorMessage = null;
            return true;
        }
    }
}