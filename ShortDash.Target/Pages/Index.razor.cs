using Microsoft.AspNetCore.Components;
using System;
using ShortDash.Target.Services;

namespace ShortDash.Target.Pages
{
    public partial class Index : ComponentBase, IDisposable
    {
        private bool wasDisposed;
        public string ConnectionStatusClass => TargetHubClient.IsConnected() ? "success" : "danger";
        public bool IsConnected => TargetHubClient.IsConnected();
        public DateTime LastConnection => TargetHubClient.LastConnectionDateTime;
        public DateTime LastConnectionAttempt => TargetHubClient.LastConnectionAttemptDateTime;
        private string ServerUrl => TargetHubClient.ServerUrl;
        private string TargetId => TargetHubClient.TargetId;

        [Inject]
        private TargetHubClient TargetHubClient { get; set; }

        public string DisplayDateTime(DateTime value)
        {
            return (value.Ticks == 0) ? "N/A" : value.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!wasDisposed && disposing)
            {
                TargetHubClient.OnConnected -= TargetHubStatusChangeEvent;
                TargetHubClient.OnConnecting -= TargetHubStatusChangeEvent;
                TargetHubClient.OnClosed -= TargetHubStatusChangeEvent;
                TargetHubClient.OnReconnected -= TargetHubStatusChangeEvent;
                TargetHubClient.OnReconnecting -= TargetHubStatusChangeEvent;
            }
            wasDisposed = true;
        }

        protected override void OnInitialized()
        {
            TargetHubClient.OnConnected += TargetHubStatusChangeEvent;
            TargetHubClient.OnConnecting += TargetHubStatusChangeEvent;
            TargetHubClient.OnClosed += TargetHubStatusChangeEvent;
            TargetHubClient.OnReconnected += TargetHubStatusChangeEvent;
            TargetHubClient.OnReconnecting += TargetHubStatusChangeEvent;
        }

        private void TargetHubStatusChangeEvent(object sender, EventArgs args)
        {
            InvokeAsync(() => StateHasChanged());
        }
    }
}
