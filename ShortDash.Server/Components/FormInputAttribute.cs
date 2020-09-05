using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public class FormInputAttribute : Attribute
    {
        public Type Type { get; set; } = null;
    }
}