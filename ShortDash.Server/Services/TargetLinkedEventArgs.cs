using System;

namespace ShortDash.Server.Services
{
    public class TargetLinkedEventArgs : EventArgs
    {
        public string TargetId { get; set; }
        public string TargetLinkCode { get; set; }
        public string TargetPublicKey { get; set; }
    }
}
