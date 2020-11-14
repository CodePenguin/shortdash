using Microsoft.AspNetCore.Components;
using OtpNet;
using ShortDash.Core.Interfaces;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Threading.Tasks;
using System.Web;

namespace ShortDash.Server.Pages
{
    public sealed partial class Targets_Link : PageBase, IDisposable
    {
        [Inject]
        private IEncryptedChannelService EncryptedChannelService { get; set; }

        private bool Linking { get; set; }
        private string ServerId { get; set; }
        private string TargetLinkCode { get; set; }

        public void Dispose()
        {
            StopLinking();
        }

        protected override Task OnParametersSetAsync()
        {
            Linking = false;
            StartLinking();
            return base.OnParametersSetAsync();
        }

        private void Cancel()
        {
            StopLinking();
            NavigationManager.NavigateTo("/targets");
        }

        private string GenerateTargetLinkCode()
        {
            var otp = new Totp(KeyGeneration.GenerateRandomKey(10));
            return otp.ComputeTotp();
        }

        private async void StartLinking()
        {
            if (!await SecureContext.ValidateUserAsync())
            {
                return;
            }
            ServerId = EncryptedChannelService.SenderId();

            TargetLinkCode = GenerateTargetLinkCode();

            TargetLinkService.OnTargetLinked += TargetLinkedEvent;

            var request = new LinkTargetRequest { TargetLinkCode = TargetLinkCode };
            TargetLinkService.AddRequest(request);

            Linking = true;
        }

        private void StopLinking()
        {
            if (!Linking)
            {
                return;
            }
            Linking = false;
            TargetLinkService.OnTargetLinked -= TargetLinkedEvent;
        }

        private void TargetLinked(string targetLinkCode, string targetId)
        {
            if (!targetLinkCode.Equals(TargetLinkCode))
            {
                return;
            }
            StopLinking();
            ToastService.ShowSuccess("The target has been linked!", "LINKED");
            NavigationManager.NavigateTo("/targets/" + HttpUtility.UrlEncode(targetId));
        }

        private void TargetLinkedEvent(object sender, TargetLinkedEventArgs eventArgs)
        {
            InvokeAsync(() => TargetLinked(eventArgs.TargetLinkCode, eventArgs.TargetId));
        }
    }
}
