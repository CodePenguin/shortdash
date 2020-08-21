using System;

namespace ShortDash.Server.Data
{
    public class ActionProcessorService
    {
        // TODO: Change actionId to a GUID?
        public void Execute(string actionId, string parameters, bool toggleState)
        {
            Console.WriteLine($"Clicked {actionId} - {toggleState} - {parameters}");
        }

    }
}
