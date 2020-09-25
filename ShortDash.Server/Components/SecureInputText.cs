using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ShortDash.Core.Services;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class SecureInputText : InputBase<string>, IDisposable
    {
        private DotNetObjectReference<SecureInputText> objectReference;

        [CascadingParameter(Name = "ClientPublicKey")]
        protected string ClientPublicKey { get; set; }

        protected string InputType => "text";

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [CascadingParameter(Name = "ServerPublicKey")]
        protected string ServerPublicKey { get; set; }

        protected string UniqueId { get; private set; }

        [Inject]
        private IKeyStoreService<TargetsHubEncryptedChannelService> KeyStore { get; set; }

        [Inject]
        private ILogger<SecureInputText> Logger { get; set; } // TODO: REMOVE?

        public void Dispose()
        {
            Logger.LogDebug("Disposing SecureInputText");
            objectReference?.Dispose();
        }

        [JSInvokable]
        public void OnClientChanged(string encryptedValue)
        {
            Console.WriteLine("OnClientChanged: " + encryptedValue);
            DecryptConvertValue(encryptedValue);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "id", UniqueId);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddAttribute(3, "type", "text");
            builder.CloseElement();
        }

        protected void DecryptConvertValue(string encryptedValue)
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(KeyStore.RetrieveKey(false));
            var data = Convert.FromBase64String(encryptedValue);
            var decryptedBytes = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            var decryptedValue = Encoding.UTF8.GetString(decryptedBytes);
            CurrentValueAsString = decryptedValue;
        }

        protected string EncryptedCurrentValue()
        {
            Console.WriteLine("Encrypting Value: " + Value);
            Console.WriteLine("ClientPublicKey: " + ClientPublicKey);

            if (string.IsNullOrWhiteSpace(ClientPublicKey))
            {
                return "";
            }

            using var rsa = RSA.Create();
            rsa.FromXmlString(ClientPublicKey);
            return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(Value), RSAEncryptionPadding.Pkcs1));
        }

        protected override string FormatValueAsString(string value)
        {
            Logger.LogDebug($"FormatValueAsString - {value}");
            return value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            Logger.LogDebug("OnAfterRenderAsync for Input");
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
            Console.WriteLine("UniqueID: " + UniqueId);
        }

        protected override bool TryParseValueFromString(string value, out string result, out string validationErrorMessage)
        {
            Logger.LogDebug($"TryParseValueFromString - {value}");
            result = value;
            validationErrorMessage = null;
            return true;
        }
    }
}