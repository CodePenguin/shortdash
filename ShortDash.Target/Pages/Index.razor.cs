using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ShortDash.Target.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace ShortDash.Target.Pages
{
    public partial class Index : ComponentBase, IDisposable
    {
        private CancellationTokenSource cancelConnectTokenSource;
        private bool IsEditingConnection = false;
        private bool wasDisposed;
        private bool InitializedDataProtection => TargetHubClient.InitializedDataProtection;
        private bool IsConnected => TargetHubClient.IsConnected();
        private bool IsConnecting => TargetHubClient.IsConnecting();
        private DateTime LastConnection => TargetHubClient.LastConnectionDateTime;
        private DateTime LastConnectionAttempt => TargetHubClient.LastConnectionAttemptDateTime;
        private string ServerId => TargetHubClient.ServerId;
        private string ServerUrl => TargetHubClient.ServerUrl;
        private string TargetId => TargetHubClient.TargetId;
        private TargetConnectModel ConnectModel { get; set; } = new TargetConnectModel();
        private bool IsLinked => TargetHubClient.Linked;
        private bool IsLinking => TargetHubClient.Linking;
        private TargetLinkModel LinkModel { get; set; } = new TargetLinkModel();
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
                cancelConnectTokenSource?.Dispose();
                TargetHubClient.OnStatusChanged -= TargetHubStatusChangeEvent;
            }
            wasDisposed = true;
        }

        protected override void OnInitialized()
        {
            TargetHubClient.OnStatusChanged += TargetHubStatusChangeEvent;
        }

        private async Task CancelLinking()
        {
            if (IsLinking)
            {
                TargetHubClient.StopLinking();
            }
            else
            {
                LinkModel.TargetLinkCode = "";
                await ResetConnection();
                StateHasChanged();
            }
        }

        private async Task ConnectToServerUrl()
        {
            IsEditingConnection = false;
            await ResetConnection();

            cancelConnectTokenSource = new CancellationTokenSource();
            await TargetHubClient.ConnectAsync(ConnectModel.ServerUrl, cancelConnectTokenSource.Token);
        }

        private string GetConnectionStatus()
        {
            var connectionStatus = TargetHubClient.ConnectionStatus();
            if (IsConnecting)
            {
                return "Connecting";
            }
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
            if (!IsConnecting && connectionStatus != HubConnectionState.Connected)
            {
                return "danger";
            }
            return TargetHubClient.Authenticated ? "success" : "warning";
        }

        private async Task ResetConnection()
        {
            cancelConnectTokenSource?.Cancel();
            cancelConnectTokenSource?.Dispose();
            await TargetHubClient.ResetConnection();
        }

        private void StartLinking()
        {
            if (IsLinking)
            {
                return;
            }
            var targetLinkCode = LinkModel.TargetLinkCode.Replace(" ", "").Trim();
            TargetHubClient.StartLinking(targetLinkCode);
            LinkModel.TargetLinkCode = string.Empty;
        }

        private void TargetHubStatusChangeEvent(object sender, EventArgs args)
        {
            InvokeAsync(() => StateHasChanged());
        }

        private void ToggleEditConnection()
        {
            IsEditingConnection = !IsEditingConnection;
            if (IsEditingConnection)
            {
                ConnectModel.ServerUrl = TargetHubClient.ServerUrl;
            }
        }

        private class TargetConnectModel
        {
            [Required]
            [Display(Name = "Server URL")]
            [Url(ErrorMessage = "The Server URL must be a valid fully-qualified URL.")]
            public string ServerUrl { get; set; }
        }

        private class TargetLinkModel
        {
            [Required]
            [Display(Name = "Target Code")]
            public string TargetLinkCode { get; set; }
        }
    }
}
