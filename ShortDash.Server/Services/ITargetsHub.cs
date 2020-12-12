using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public interface ITargetsHub
    {
        Task Authenticate(string encryptedChallenge);

        Task ExecuteAction(string encryptedParameters);

        Task Identify(string publicKey);

        Task TargetAuthenticated(string encryptedKey);

        Task UnlinkTarget(string encryptedParameters);
    }
}
