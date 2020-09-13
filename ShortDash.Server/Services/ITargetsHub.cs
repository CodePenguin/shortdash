using ShortDash.Core.Services;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public interface ITargetsHub
    {
        Task ExecuteAction(string actionTypeName, string parameters, bool toggleState);

        Task ReceiveMessage(string user, string message);
    }
}
