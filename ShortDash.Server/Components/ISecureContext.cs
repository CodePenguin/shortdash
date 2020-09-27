using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public interface ISecureContext
    {
        public string ReceiverId { get; }

        string Decrypt(string value);

        string Encrypt(string value);
    }
}
