using System;

namespace ShortDash.Server.Components
{
    public sealed class FormInputAttribute : Attribute
    {
        public Type Type { get; set; } = null;
        public string TypeName { get; set; } = null;
    }
}