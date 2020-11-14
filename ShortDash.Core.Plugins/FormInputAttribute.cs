using System;

namespace ShortDash.Core.Plugins
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormInputAttribute : Attribute
    {
        public string TypeName { get; set; } = null;
    }
}
