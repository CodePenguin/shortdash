using ShortDash.Core.Services;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public interface ITargetsHub
    {
        Task Authenticate(string challenge, string publicKey);

        Task ExecuteAction(string actionTypeName, string parameters, bool toggleState);

        Task LogDebug(string category, string message, params object[] args);

        Task LogError(string category, string message, params object[] args);

        Task LogInformation(string category, string message, params object[] args);

        Task LogWarning(string category, string message, params object[] args);

        Task ReceiveMessage(string user, string message);

        Task TargetAuthenticated(string encryptedKey);
    }
}
