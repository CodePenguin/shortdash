using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using ShortDash.Target.Services;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel.DataAnnotations;

namespace ShortDash.Target.Pages
{
    public partial class Index : ComponentBase, IDisposable
    {
        private bool wasDisposed;
        private bool IsConnected => TargetHubClient.IsConnected();
        private DateTime LastConnection => TargetHubClient.LastConnectionDateTime;
        private DateTime LastConnectionAttempt => TargetHubClient.LastConnectionAttemptDateTime;
        private string ServerId => TargetHubClient.ServerId;
        private string ServerUrl => TargetHubClient.ServerUrl;
        private string TargetId => TargetHubClient.TargetId;
        private bool IsLinked => TargetHubClient.Linked;
        private bool IsLinking => TargetHubClient.Linking;
        private TargetLinkModel Model { get; set; } = new TargetLinkModel();
        private bool ServerConfigured => !string.IsNullOrWhiteSpace(TargetHubClient.ServerUrl);

        [Inject]
        private TargetHubClient TargetHubClient { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!wasDisposed && disposing)
            {
                TargetHubClient.OnAuthenticated -= TargetHubStatusChangeEvent;
                TargetHubClient.OnConnected -= TargetHubStatusChangeEvent;
                TargetHubClient.OnConnecting -= TargetHubStatusChangeEvent;
                TargetHubClient.OnClosed -= TargetHubStatusChangeEvent;
                TargetHubClient.OnReconnected -= TargetHubStatusChangeEvent;
                TargetHubClient.OnReconnecting -= TargetHubStatusChangeEvent;
                TargetHubClient.OnUnlinked -= TargetHubStatusChangeEvent;
            }
            wasDisposed = true;
        }

        protected override void OnInitialized()
        {
            TargetHubClient.OnAuthenticated += TargetHubStatusChangeEvent;
            TargetHubClient.OnConnected += TargetHubStatusChangeEvent;
            TargetHubClient.OnConnecting += TargetHubStatusChangeEvent;
            TargetHubClient.OnClosed += TargetHubStatusChangeEvent;
            TargetHubClient.OnReconnected += TargetHubStatusChangeEvent;
            TargetHubClient.OnReconnecting += TargetHubStatusChangeEvent;
            TargetHubClient.OnUnlinked += TargetHubStatusChangeEvent;
        }

        private void CancelLinking()
        {
            if (IsLinking)
            {
                TargetHubClient.StopLinking();
            }
            else
            {
                Model.TargetLinkCode = "";
                StateHasChanged();
            }
        }

        private string GetConnectionStatus()
        {
            var connectionStatus = TargetHubClient.ConnectionStatus();
            if (!IsConnected)
            {
                return connectionStatus.ToString();
            }
            if (TargetHubClient.Authenticated)
            {
                return "Authenticated";
            }
            return "Connected - Pending " + (IsLinked ? "Authentication" : "Link");
        }

        private string GetConnectionStatusClass()
        {
            var connectionStatus = TargetHubClient.ConnectionStatus();
            if (connectionStatus != HubConnectionState.Connected)
            {
                return "danger";
            }
            return TargetHubClient.Authenticated ? "success" : "warning";
        }

        private void StartLinking()
        {
            if (IsLinking)
            {
                return;
            }
            var targetLinkCode = Model.TargetLinkCode.Replace(" ", "").Trim();
            TargetHubClient.StartLinking(targetLinkCode);
        }

        private void TargetHubStatusChangeEvent(object sender, EventArgs args)
        {
            InvokeAsync(() => StateHasChanged());
        }

        private class TargetLinkModel
        {
            [Required]
            [Display(Name = "Target Code")]
            public string TargetLinkCode { get; set; }
        }
    }
}
