using Microsoft.Extensions.Logging;

namespace ShortDash.Core.Models
{
    public class LogParameters
    {
        public string Category { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
    }
}
