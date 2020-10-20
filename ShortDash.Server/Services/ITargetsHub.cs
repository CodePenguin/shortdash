using ShortDash.Core.Services;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public interface ITargetsHub
    {
        Task Authenticate(string encryptedChallenge);

        Task ExecuteAction(string encryptedParameters);

        Task Identify(string publicKey);

        Task LogDebug(string category, string message, params object[] args);

        Task LogError(string category, string message, params object[] args);

        Task LogInformation(string category, string message, params object[] args);

        Task LogWarning(string category, string message, params object[] args);

        Task ReceiveMessage(string user, string message);

        Task TargetAuthenticated(string encryptedKey);

        Task UnlinkTarget(string encryptedParameters);
    }
}
