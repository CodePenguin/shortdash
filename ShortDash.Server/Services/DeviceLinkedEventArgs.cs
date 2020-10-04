using System;

namespace ShortDash.Server.Services
{
    public class DeviceLinkedEventArgs : EventArgs
    {
        public string DeviceId { get; set; }
        public string DeviceLinkCode { get; set; }
    }
}
