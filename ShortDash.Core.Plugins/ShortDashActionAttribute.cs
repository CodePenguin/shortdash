using System;

namespace ShortDash.Core.Plugins
{
    public sealed class ShortDashActionAttribute : Attribute
    {
        public string Description { get; set; } = "";
        public Type ParametersType { get; set; } = null;
        public string Title { get; set; } = "";
        public bool Toggle { get; set; } = false;
    }
}