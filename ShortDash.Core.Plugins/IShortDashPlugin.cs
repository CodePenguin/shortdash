using System;
using System.Collections.Generic;

namespace ShortDash.Core.Plugins
{
    public interface IShortDashPlugin
    {
        string Description { get; }
        string Name { get; }

        IEnumerable<Type> RegisterActions();
    }
}