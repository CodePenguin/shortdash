using ShortDash.Core.Plugins;
using System;

namespace ShortDash.Core.Models
{
    public class ActionExecutedParameters
    {
        public Guid RequestId { get; set; }
        public ShortDashActionResult Result { get; set; }
    }
}
