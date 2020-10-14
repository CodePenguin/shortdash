using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public interface ISecureContext
    {
        public string DeviceId { get; }

        Task<bool> AuthorizeAsync(string policy);

        string Decrypt(string value);

        string Encrypt(string value);

        string GenerateChallenge(out string rawChallenge);

        Task<bool> ValidateUserAsync();

        bool VerifyChallengeResponse(string rawChallenge, string challengeResponse);
    }
}
