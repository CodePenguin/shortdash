using System;

namespace ShortDash.Core.Plugins
{
    public sealed class FormInputAttribute : Attribute
    {
        public string TypeName { get; set; } = null;
    }
}