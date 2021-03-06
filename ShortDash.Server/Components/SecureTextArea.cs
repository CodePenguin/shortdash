﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class SecureTextArea : InputBase<string>
    {
        private string lastSentValue;
        private DotNetObjectReference<SecureTextArea> objectReference;

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [CascadingParameter]
        private ISecureContext SecureContext { get; set; }

        private string UniqueId { get; set; }

        [JSInvokable]
        public void OnClientChanged(string encryptedValue)
        {
            DecryptConvertValue(encryptedValue);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "textarea");
            builder.AddAttribute(1, "id", UniqueId);
            if (AdditionalAttributes != null && !AdditionalAttributes.ContainsKey("rows"))
            {
                builder.AddAttribute(2, "rows", "10");
            }
            builder.AddMultipleAttributes(3, AdditionalAttributes);
            builder.CloseElement();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                objectReference?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                lastSentValue = CurrentValue;
                await JSRuntime.InvokeVoidAsync("secureContext.initSecureElement", UniqueId, objectReference, EncryptedCurrentValue());
            }
            else if (lastSentValue != CurrentValue)
            {
                lastSentValue = CurrentValue;
                await JSRuntime.InvokeVoidAsync("secureContext.setSecureElementValue", UniqueId, EncryptedCurrentValue());
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            objectReference = DotNetObjectReference.Create(this);
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            UniqueId = AdditionalAttributes.TryGetValue("id", out var id) ? id.ToString() : Guid.NewGuid().ToString("N");
        }

        protected override bool TryParseValueFromString(string value, out string result, out string validationErrorMessage)
        {
            result = value;
            validationErrorMessage = null;
            return true;
        }

        private void DecryptConvertValue(string encryptedValue)
        {
            CurrentValueAsString = SecureContext.Decrypt(encryptedValue);
        }

        private string EncryptedCurrentValue()
        {
            return SecureContext.Encrypt(Value);
        }
    }
}