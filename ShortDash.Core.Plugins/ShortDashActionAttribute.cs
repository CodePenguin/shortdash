using System;

namespace ShortDash.Core.Plugins
{
    public sealed class ShortDashActionAttribute : Attribute
    {
        public string Description { get; set; } = "";
        public Type ParametersType { get; set; } = typeof(ShortDashActionParameters);
        public string Title { get; set; } = "";
    }
}