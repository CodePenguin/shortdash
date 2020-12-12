using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Extensions;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Models;
using ShortDash.Core.Services;
using ShortDash.Target.Shared;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ShortDash.Target.Services
{
    public class TargetHubClient : IDisposable
    {
        private readonly ActionService actionService;
        private readonly IDataProtectionService dataProtectionService;
        private readonly IEncryptedChannelService encryptedChannelService;
        private readonly string keyPurpose = "ServerPublic";
        private readonly ISecureKeyStoreService keyStore;
        private readonly ILogger<TargetHubClient> logger;
        private readonly IRetryPolicy retryPolicy;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private bool connecting;
        private HubConnection connection;
        private bool disposed;
        private CancellationTokenSource internalConnectCancellationTokenSource;
        private string pendingServerKey;
        private string serverChannelId;

        public TargetHubClient(ILogger<TargetHubClient> logger, IRetryPolicy retryPolicy, IDataProtectionService dataProtectionService,
            IEncryptedChannelService encryptedChannelService, ISecureKeyStoreService keyStore, ActionService actionService,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.retryPolicy = retryPolicy;
            this.dataProtectionService = dataProtectionService;
            this.encryptedChannelService = encryptedChannelService;
            this.actionService = actionService;
            this.keyStore = keyStore;
            this.serviceScopeFactory = serviceScopeFactory;

            ServerUrl = GetServerUrl();

            SetupConnection();
        }

        ~TargetHubClient()
        {
            Dispose(false);
        }

        public event EventHandler OnStatusChanged;

        public bool Authenticated { get; private set; }
        public bool InitializedDataProtection { get; private set; }
        public DateTime LastConnectionAttemptDateTime { get; private set; }
        public DateTime LastConnectionDateTime { get; private set; }
        public bool Linked { get; private set; }
        public bool Linking { get; private set; }
        public string ServerId { get; private set; }
        public string ServerUrl { get; private set; }
        public string TargetId { get; private set; }

        public async Task ConnectAsync(CancellationToken externalCancellationToken)
        {
            if (connecting || connection == null)
            {
                return;
            }
            internalConnectCancellationTokenSource = new CancellationTokenSource();
            using var connectCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken, internalConnectCancellationTokenSource.Token);
            try
            {
                var cancellationToken = connectCancellationTokenSource.Token;
                connecting = true;
                var retryContext = new RetryContext();
                while (true)
                {
                    if (!connecting || connection == null)
                    {
                        break;
                    }
                    try
                    {
                        Connecting();
                        await connection.StartAsync(cancellationToken);
                        Connected();
                        return;
                    }
                    catch when (cancellationToken.IsCancellationRequested)
                    {
                        Linking = false;
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Connection attempt failed: " + ex.Message);
                        retryContext.PreviousRetryCount += 1;
                        var retryDelay = retryPolicy.NextRetryDelay(retryContext);
                        if (retryDelay == null)
                        {
                            return;
                        }
                        await Task.Delay((int)retryDelay.GetValueOrDefault().TotalMilliseconds, cancellationToken);
                    }
                }
            }
            finally
            {
                connecting = false;
            }
        }

        public async Task ConnectAsync(string serverUrl, CancellationToken cancellationToken)
        {
            SetServerUrl(serverUrl);
            SetupConnection();
            await ConnectAsync(cancellationToken);
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

        public bool IsConnecting()
        {
            return connecting || connection?.State == HubConnectionState.Connecting;
        }

        public void LogCritical<T>(string message, params object[] args)
        {
            SendLog(LogLevel.Critical, typeof(T).FullName, message, args);
        }

        public void LogDebug<T>(string message, params object[] args)
        {
            SendLog(LogLevel.Debug, typeof(T).FullName, message, args);
        }

        public void LogError<T>(string message, params object[] args)
        {
            SendLog(LogLevel.Error, typeof(T).FullName, message, args);
        }

        public void LogInformation<T>(string message, params object[] args)
        {
            SendLog(LogLevel.Information, typeof(T).FullName, message, args);
        }

        public void LogTrace<T>(string message, params object[] args)
        {
            SendLog(LogLevel.Trace, typeof(T).FullName, message, args);
        }

        public void LogWarning<T>(string message, params object[] args)
        {
            SendLog(LogLevel.Warning, typeof(T).FullName, message, args);
        }

        public async Task ResetConnection()
        {
            UnlinkTarget();
            await Disconnect();
            SetServerUrl(string.Empty);
        }

        public async void SetupConnection()
        {
            await Disconnect();

            InitializedDataProtection = dataProtectionService.Initialized();
            if (!InitializedDataProtection)
            {
                logger.LogCritical("Data protection initialization failed.");
                return;
            }

            TargetId = encryptedChannelService.SenderId();
            Linked = keyStore.HasKey(keyPurpose);
            ServerId = Linked ? GetServerId(keyStore.RetrieveSecureKey(keyPurpose)) : null;
            Authenticated = false;

            if (string.IsNullOrEmpty(ServerUrl))
            {
                logger.LogWarning("Server connection has not been initialized.");
                return;
            }

            logger.LogDebug("Server: " + ServerUrl);
            logger.LogDebug("Target ID: " + TargetId);

            var hubUrl = ServerUrl.Trim('/') + "/targetshub?targetId=" + HttpUtility.UrlEncode(TargetId);
            connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect(retryPolicy)
                .Build();

            connection.Closed += Closed;
            connection.Reconnected += Reconnected;
            connection.Reconnecting += Reconnecting;

            connection.On<string>("Authenticate", Authenticate);
            connection.On<string>("ExecuteAction", ExecuteAction);
            connection.On<string>("Identify", Identify);
            connection.On<string>("TargetAuthenticated", TargetAuthenticated);
            connection.On<string>("UnlinkTarget", UnlinkTarget);
        }

        public void StartLinking(string targetLinkCode)
        {
            if (Linked || Linking || string.IsNullOrWhiteSpace(pendingServerKey))
            {
                return;
            }
            Linking = true;
            var parameters = new LinkTargetParameters
            {
                PublicKey = encryptedChannelService.ExportPublicKey(),
                Name = Environment.MachineName,
                Platform = EnvironmentExtensions.Platform(),
                TargetId = TargetId,
                TargetLinkCode = targetLinkCode
            };
            var encryptedParameters = encryptedChannelService.LocalEncryptForPublicKey(pendingServerKey, parameters);
            connection.SendAsync("LinkTarget", encryptedParameters);
        }

        public void StopLinking()
        {
            Authenticated = false;
            Linked = false;
            Linking = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                internalConnectCancellationTokenSource?.Cancel();
                internalConnectCancellationTokenSource?.Dispose();
                _ = connection?.DisposeAsync();
            }
            disposed = true;
        }

        private static string GetServerId(string publicKey)
        {
            return RSAExtensions.GetPublicKeyFingerprint(publicKey);
        }

        private void Authenticate(string challenge)
        {
            logger.LogDebug("Received authentication request...");
            if (!Linking && !keyStore.HasKey(keyPurpose))
            {
                logger.LogWarning("Requesting server identification after unexpected authentication request.");
                connection.SendAsync("Identify");
                return;
            }
            var serverKey = !string.IsNullOrEmpty(pendingServerKey) ? pendingServerKey : keyStore.RetrieveSecureKey(keyPurpose);
            var challengeResponse = encryptedChannelService.GenerateChallengeResponse(challenge, serverKey);
            if (string.IsNullOrEmpty(challengeResponse))
            {
                logger.LogError("The authentication request could not be verified.");
                return;
            }
            logger.LogDebug("Sending authentication response...");
            connection.SendAsync("Authenticate", challengeResponse);
        }

        private Task Closed(Exception error)
        {
            logger.LogDebug("Connection Closed!");
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private void Connected()
        {
            logger.LogDebug($"Connected to server.");
            LastConnectionDateTime = DateTime.Now;
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Connecting()
        {
            if (serverChannelId != null)
            {
                encryptedChannelService.CloseChannel(serverChannelId);
                serverChannelId = null;
            }
            logger.LogDebug("Connecting to server...");
            LastConnectionAttemptDateTime = DateTime.Now;
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private TParameterType DecryptParameters<TParameterType>(string encryptedParameters)
        {
            if (!encryptedChannelService.TryDecryptVerify(serverChannelId, encryptedParameters, out var decryptedParameters))
            {
                return default;
            }
            return JsonSerializer.Deserialize<TParameterType>(decryptedParameters);
        }

        private async Task Disconnect()
        {
            if (connection == null)
            {
                return;
            }
            try
            {
                internalConnectCancellationTokenSource?.Cancel();
                internalConnectCancellationTokenSource?.Dispose();
                await connection.StopAsync();
            }
            finally
            {
                await connection.DisposeAsync();
                connection = null;
            }
        }

        private string EncryptParameters(object parameters)
        {
            var data = JsonSerializer.Serialize(parameters);
            return encryptedChannelService.EncryptSigned(serverChannelId, data);
        }

        private void ExecuteAction(string encryptedParameters)
        {
            var parameters = DecryptParameters<ExecuteActionParameters>(encryptedParameters);
            logger.LogDebug($"Received execute action request: {parameters.ActionTypeName}");
            if (parameters == null)
            {
                logger.LogError("Invalid ExecuteAction parameters");
                return;
            }
            var resultParameters = new ActionExecutedParameters
            {
                RequestId = parameters.RequestId,
                Result = actionService.Execute(parameters.ActionTypeName, parameters.Parameters, parameters.ToggleState)
            };
            logger.LogDebug($"Sending action result for: {parameters.ActionTypeName}");
            encryptedParameters = EncryptParameters(resultParameters);
            connection.SendAsync("ActionExecuted", encryptedParameters);
        }

        private string GetServerUrl()
        {
            using var scope = serviceScopeFactory.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
            return configurationService.GetSection(ApplicationSettings.Key);
        }

        private void Identify(string serverPublicKey)
        {
            logger.LogDebug("Received public key from server.");
            ServerId = GetServerId(serverPublicKey);
            pendingServerKey = serverPublicKey;
            if (keyStore.HasKey(keyPurpose))
            {
                logger.LogWarning("The server attempted to authenticate as new target.");
                UnlinkTarget();
                return;
            }
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private Task Reconnected(string message)
        {
            logger.LogDebug("Reconnected!");
            LastConnectionDateTime = DateTime.Now;
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private Task Reconnecting(Exception error)
        {
            encryptedChannelService.CloseChannel(serverChannelId);
            serverChannelId = null;
            Authenticated = false;
            logger.LogDebug("Reconnecting...");
            LastConnectionAttemptDateTime = DateTime.Now;
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private void SendLog(LogLevel logLevel, string categoryName, string message, object[] args)
        {
            var parameters = new LogParameters
            {
                LogLevel = logLevel,
                Category = categoryName,
                Message = string.Format(message, args),
            };
            var encryptedParameters = EncryptParameters(parameters);
            connection.SendAsync("Log", encryptedParameters);
        }

        private void SetServerUrl(string value)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
            configurationService.SetSection(ApplicationSettings.Key, value);
            ServerUrl = value;
        }

        private void TargetAuthenticated(string encryptedKey)
        {
            if (!string.IsNullOrEmpty(pendingServerKey))
            {
                keyStore.StoreSecureKey(keyPurpose, pendingServerKey);
                pendingServerKey = null;
            }
            logger.LogDebug("Received session key from server.");
            serverChannelId = encryptedChannelService.OpenChannel(keyStore.RetrieveSecureKey(keyPurpose), encryptedKey);
            ServerId = encryptedChannelService.ReceiverId(serverChannelId);
            Authenticated = true;
            Linked = true;
            Linking = false;
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UnlinkTarget()
        {
            if (pendingServerKey == null)
            {
                pendingServerKey = keyStore.RetrieveSecureKey(keyPurpose);
            }
            keyStore.RemoveKey(keyPurpose);
            Authenticated = false;
            Linked = false;
            Linking = false;
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UnlinkTarget(string encryptedParameters)
        {
            logger.LogDebug($"Received unlink target request");
            var parameters = DecryptParameters<UnlinkTargetParameters>(encryptedParameters);
            if (parameters == null || parameters.TargetId != TargetId)
            {
                logger.LogError("Invalid UnlinkTargetParameters parameters");
                return;
            }
            UnlinkTarget();
        }
    }
}
