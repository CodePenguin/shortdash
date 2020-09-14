using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Services;
using ShortDash.Target.Shared;
using System;
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
        private readonly ConnectionSettings connectionSettings;
        private readonly IRetryPolicy retryPolicy;
        private ActionService actionService;
        private bool connecting;
        private HubConnection connection;
        private bool disposed;
        private ILogger<TargetHubClient> logger;

        public TargetHubClient(ILogger<TargetHubClient> logger, IRetryPolicy retryPolicy, ActionService actionService, IConfiguration configuration)
        {
            this.logger = logger;
            this.retryPolicy = retryPolicy;
            this.actionService = actionService;
            connectionSettings = new ConnectionSettings();

            configuration.GetSection(ConnectionSettings.Key).Bind(connectionSettings);

            SetupConnection();
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

        public DateTime LastConnectionAttemptDateTime { get; private set; }

        public DateTime LastConnectionDateTime { get; private set; }

        public string ServerUrl { get; private set; }

        public string TargetId { get; private set; }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (connecting || connection == null)
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
            return connection?.State ?? HubConnectionState.Disconnected;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsConnected()
        {
            return connection?.State == HubConnectionState.Connected;
        }

        public void LogDebug<T>(string message, params object[] args)
        {
            connection.SendAsync("LogDebug", typeof(T).FullName, message, args);
        }

        public void LogError<T>(string message, params object[] args)
        {
            connection.SendAsync("LogError", typeof(T).FullName, message, args);
        }

        public void LogInformation<T>(string message, params object[] args)
        {
            connection.SendAsync("LogInformation", typeof(T).FullName, message, args);
        }

        public void LogWarning<T>(string message, params object[] args)
        {
            connection.SendAsync("LogWarning", typeof(T).FullName, message, args);
        }

        public async void Send(string user, string message)
        {
            if (!IsConnected())
            {
                return;
            }
            await connection.SendAsync("SendMessage", user, message);
        }

        public async void SetupConnection()
        {
            if (connection != null)
            {
                await connection.StopAsync();
                await connection.DisposeAsync();
            }

            TargetId = connectionSettings?.TargetId;
            ServerUrl = connectionSettings?.ServerUrl.Trim('/');

            if (string.IsNullOrEmpty(TargetId) && string.IsNullOrEmpty(ServerUrl))
            {
                logger.LogDebug("Server connection has not been initialized.");
                return;
            }

            logger.LogDebug("Server: " + ServerUrl);
            logger.LogDebug("Target ID: " + TargetId);

            var hubUrl = ServerUrl + "/targetshub?targetId=" + Uri.EscapeUriString(TargetId);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
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
            logger.LogDebug($"Connected to server.");
            LastConnectionDateTime = DateTime.Now;
            OnConnected?.Invoke(this, null);
        }

        private void Connecting()
        {
            logger.LogDebug("Connecting to server...");
            LastConnectionAttemptDateTime = DateTime.Now;
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
            LastConnectionDateTime = DateTime.Now;
            OnReconnected?.Invoke(this, null);
            return Task.CompletedTask;
        }

        private Task Reconnecting(Exception error)
        {
            logger.LogDebug("Reconnecting...");
            LastConnectionAttemptDateTime = DateTime.Now;
            OnReconnecting?.Invoke(this, null);
            return Task.CompletedTask;
        }
    }
}
