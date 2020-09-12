using Microsoft.AspNetCore.SignalR.Client;
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

    public class TargetHubService : IDisposable
    {
        private HubConnection connection;
        private bool disposed;
        private IRetryPolicy retryPolicy;
        private Timer timer;

        public TargetHubService(IRetryPolicy retryPolicy)
        {
            this.retryPolicy = retryPolicy;
            Connect();
            CreateTimer();
        }

        ~TargetHubService()
        {
            Dispose(false);
        }

        public event EventHandler OnClosed;

        public event EventHandler OnConnecting;

        public event EventHandler<MessageArgs> OnReceiveMessage;

        public event EventHandler OnReconnected;

        public event EventHandler OnReconnecting;

        public HubConnectionState ConnectionStatus()
        {
            return connection.State;
        }

        public void CreateTimer()
        {
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

        public Task Reconnect()
        {
            return connection.StartAsync();
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
                timer.Dispose();
                _ = connection.DisposeAsync();
            }
            disposed = true;
        }

        private Task Closed(Exception error)
        {
            Console.WriteLine("Connection Closed!");
            OnClosed.Invoke(this, null);
            return Task.CompletedTask;
        }

        private Task Connect()
        {
            if (connection != null)
            {
                connection.DisposeAsync();
                connection = null;
            }

            connection = new HubConnectionBuilder()
                .WithUrl("http://172.16.0.159:5000/targetshub")
                .WithAutomaticReconnect(retryPolicy)
                .Build();

            connection.Closed += Closed;
            connection.Reconnected += Reconnected;
            connection.Reconnecting += Reconnecting;

            connection.On<string, string>("ReceiveMessage", ReceivedMessage);

            Console.WriteLine("Connecting...");
            OnConnecting?.Invoke(this, null);
            return connection.StartAsync();
        }

        private void ReceivedMessage(string user, string message)
        {
            Console.WriteLine("Received Message from " + user + ":" + message);
            OnReceiveMessage?.Invoke(this, new MessageArgs { User = user, Message = message });
        }

        private Task Reconnected(string message)
        {
            Console.WriteLine("Reconnected!");
            OnReconnected?.Invoke(this, null);
            return Task.CompletedTask;
        }

        private Task Reconnecting(Exception error)
        {
            Console.WriteLine("Reconnecting...");
            OnReconnecting?.Invoke(this, null);
            return Task.CompletedTask;
        }
    }
}
