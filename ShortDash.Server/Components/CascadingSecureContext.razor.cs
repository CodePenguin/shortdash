using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using ShortDash.Core.Services;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class CascadingSecureContext : ComponentBase
    {
        private const string PublicKeyPrefix = "-----BEGIN PUBLIC KEY-----\n";

        private const string PublicKeySuffix = "\n-----END PUBLIC KEY-----";

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected string ClientPublicKey { get; private set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected IKeyStoreService<TargetsHubEncryptedChannelService> KeyStore { get; set; }

        protected string ServerPublicKey { get; private set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                ClientPublicKey = await GetClientPublicKey();
                Console.WriteLine("Got Public Key: " + ClientPublicKey);
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ServerPublicKey = GetServerPublicKey();
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
        }

        private async Task<string> GetClientPublicKey()
        {
            Console.WriteLine("Getting public key");
            var publicKey = await JSRuntime.InvokeAsync<string>("initSecureContext", ServerPublicKey);
            var startingIndex = publicKey.IndexOf(PublicKeyPrefix) + PublicKeyPrefix.Length;
            var endingIndex = publicKey.IndexOf(PublicKeySuffix, startingIndex);
            publicKey = publicKey.Substring(startingIndex, endingIndex - startingIndex);
            publicKey = publicKey.Replace("\n", "");
            var publicKeyBytes = Convert.FromBase64String(publicKey);
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return rsa.ToXmlString(false);
        }

        private string GetServerPublicKey()
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(KeyStore.RetrieveKey(false));
            var key = rsa.ExportSubjectPublicKeyInfo();
            return PublicKeyPrefix + Convert.ToBase64String(key) + PublicKeySuffix;
        }
    }
}
