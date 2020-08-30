using System;

namespace ShortDash.Core.Plugins
{
    public interface IShortDashAction
    {
        string Description { get; }
        Type ParametersType { get; }
        string Title { get; }

        bool Execute(object parameters, ref bool toggleState);
    }
}