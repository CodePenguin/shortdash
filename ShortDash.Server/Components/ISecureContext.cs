using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public interface ISecureContext
    {
        public string DeviceId { get; }

        string Decrypt(string value);

        string Encrypt(string value);

        string GenerateChallenge(out string rawChallenge);

        Task<bool> ValidateUser();

        bool VerifyChallengeResponse(string rawChallenge, string challengeResponse);
    }
}
