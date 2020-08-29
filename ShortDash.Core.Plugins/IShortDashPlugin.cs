using System;
using System.Collections.Generic;

namespace ShortDash.Core.Plugins
{
    public interface IShortDashPlugin
    {
        string Name { get; }
        string Description { get; }

        IEnumerable<Type> RegisterActions();
    }
}