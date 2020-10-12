using System;

namespace ShortDash.Server.Services
{
    public class DeviceUnlinkedEventArgs : EventArgs
    {
        public string DeviceId { get; set; }
    }
}
