using Microsoft.AspNetCore.Components;
using System;
using System.Timers;
using ShortDash.Target.Services;
using System.Collections.Generic;

namespace ShortDash.Target.Pages
{
    public partial class Index : ComponentBase, IDisposable
    {
        private readonly List<string> messages = new List<string>();
        private Timer timer;
        private bool wasDisposed;
        public bool IsConnected => TargetHubClient.IsConnected();
        public DateTime LastConnection { get; set; }
        public DateTime LastConnectionAttempt { get; set; }
        public DateTime LastMessageReceived { get; set; }
        public DateTime LastRefresh { get; set; }
        public string MessageInput { get; set; }

        public string UserInput { get; set; }

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
                timer.Dispose();
                TargetHubClient.OnReceiveMessage -= ReceivedMessageEvent;
            }
            wasDisposed = true;
        }

        protected override void OnInitialized()
        {
            TargetHubClient.OnConnected += ConnectedEvent;
            TargetHubClient.OnConnecting += ConnectingEvent;
            TargetHubClient.OnClosed += TargetHubStatusChangeEvent;
            TargetHubClient.OnReceiveMessage += ReceivedMessageEvent;
            TargetHubClient.OnReconnected += TargetHubStatusChangeEvent;
            TargetHubClient.OnReconnecting += ConnectingEvent;

            timer = new Timer(1000);
            timer.Elapsed += (sender, args) => InvokeAsync(Refresh);
            timer.Enabled = false;
        }

        private void ConnectedEvent(object sender, EventArgs args)
        {
            InvokeAsync(() =>
            {
                LastConnection = DateTime.Now;
                StateHasChanged();
            });
        }

        private void ConnectingEvent(object sender, EventArgs args)
        {
            InvokeAsync(() =>
            {
                LastConnectionAttempt = DateTime.Now;
                StateHasChanged();
            });
        }

        private void ReceivedMessage(string user, string message)
        {
            LastMessageReceived = DateTime.Now;
            var encodedMsg = $"{user}: {message}";
            messages.Add(encodedMsg);
            if (messages.Count > 10)
            {
                messages.Clear();
            }
            StateHasChanged();
        }

        private void ReceivedMessageEvent(object sender, MessageArgs args)
        {
            InvokeAsync(() => ReceivedMessage(args.User, args.Message));
        }

        private void Refresh()
        {
            LastRefresh = DateTime.Now;
            StateHasChanged();
        }

        private void Send() => TargetHubClient.Send(UserInput, MessageInput);

        private void TargetHubStatusChangeEvent(object sender, EventArgs args)
        {
            InvokeAsync(() => StateHasChanged());
        }
    }
}
