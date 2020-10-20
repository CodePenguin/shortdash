using System.ComponentModel.DataAnnotations;

namespace ShortDash.Target.Shared
{
    public class ConnectionSettings
    {
        public const string Key = "Connection";

        public string ServerUrl { get; set; }
    }
}
