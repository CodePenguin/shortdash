using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShortDash.Target.Services
{
    public struct MessageArgs
    {
        public string Message;
        public string User;
    }

    public class TargetHubClient : IDisposable
    {
        private readonly HubConnection connection;
        private readonly IRetryPolicy retryPolicy;
        private ActionService actionService;
        private bool connecting;
        private bool disposed;
        private ILogger<TargetHubClient> logger;
        private Timer timer;

        public TargetHubClient(ILogger<TargetHubClient> logger, IRetryPolicy retryPolicy, ActionService actionService)
        {
            this.logger = logger;
            this.retryPolicy = retryPolicy;
            this.actionService = actionService;

            const string baseUrl = "http://172.16.0.159:5000";
            var hubUrl = baseUrl + "/targetshub?targetId=2";
            connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect(retryPolicy)
                .Build();

            connection.Closed += Closed;
            connection.Reconnected += Reconnected;
            connection.Reconnecting += Reconnecting;

            connection.On<string, string>("ReceiveMessage", ReceivedMessage);
            connection.On<string, string, bool>("ExecuteAction", ExecuteAction);
        }

        ~TargetHubClient()
        {
            Dispose(false);
        }

        public event EventHandler OnClosed;

        public event EventHandler OnConnected;

        public event EventHandler OnConnecting;

        public event EventHandler<MessageArgs> OnReceiveMessage;

        public event EventHandler OnReconnected;

        public event EventHandler OnReconnecting;

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (connecting)
            {
                return;
            }
            try
            {
                connecting = true;
                var retryContext = new RetryContext();
                while (true)
                {
                    try
                    {
                        Connecting();
                        await connection.StartAsync(cancellationToken);
                        Connected();
                        return;
                    }
                    catch when (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch
                    {
                        retryContext.PreviousRetryCount += 1;
                        var retryDelay = retryPolicy.NextRetryDelay(retryContext);
                        if (retryDelay == null)
                        {
                            return;
                        }
                        await Task.Delay((int)retryDelay.GetValueOrDefault().TotalMilliseconds);
                    }
                }
            }
            finally
            {
                connecting = false;
            }
        }

        public HubConnectionState ConnectionStatus()
        {
            return connection.State;
        }

        public void CreateTimer()
        {
            // TODO: Remove once testing is complete
            timer = new Timer((state) =>
            {
                if (!IsConnected())
                {
                    return;
                }
                try
                {
                    Send("Bob", DateTime.Now.ToString());
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }, null, 1000, 5000);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsConnected()
        {
            return connection.State == HubConnectionState.Connected;
        }

        public async void Send(string user, string message)
        {
            if (!IsConnected())
            {
                return;
            }
            await connection.SendAsync("SendMessage", user, message);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                timer?.Dispose();
                _ = connection.DisposeAsync();
            }
            disposed = true;
        }

        private Task Closed(Exception error)
        {
            logger.LogDebug("Connection Closed!");
            OnClosed?.Invoke(this, null);
            return Task.CompletedTask;
        }

        private void Connected()
        {
            logger.LogDebug("Connected to server.");
            OnConnected?.Invoke(this, null);
            CreateTimer();
        }

        private void Connecting()
        {
            logger.LogDebug("Connecting to server...");
            OnConnecting?.Invoke(this, null);
        }

        private async void ExecuteAction(string actionTypeName, string parameters, bool toggleState)
        {
            logger.LogDebug($"Received execute action request: {actionTypeName}");
            var result = await actionService.Execute(actionTypeName, parameters, toggleState);
            // TODO: Send the result back to the server
        }

        private void ReceivedMessage(string user, string message)
        {
            logger.LogDebug($"Received Message from {user}: {message}");
            OnReceiveMessage?.Invoke(this, new MessageArgs { User = user, Message = message });
        }

        private Task Reconnected(string message)
        {
            logger.LogDebug("Reconnected!");
            OnReconnected?.Invoke(this, null);
            return Task.CompletedTask;
        }

        private Task Reconnecting(Exception error)
        {
            logger.LogDebug("Reconnecting...");
            OnReconnecting?.Invoke(this, null);
            return Task.CompletedTask;
        }
    }
}
