using System;
using System.Diagnostics;
using System.Media;
using System.Text.Json;

namespace ShortDash.Server.Data
{
    public class ActionProcessorService
    {
        private class ActionParameters
        {
            public string ActionType { get; set; }
            public string ActionTarget { get; set; }
        }

        private class ExecuteProcessParameters
        {
            public string FileName { get; set; }
            public string Arguments { get; set; }
            public string WorkingDirectory { get; set; }
        }

        // TODO: Change actionId to a GUID?
        public void Execute(string actionId, string parameters, bool toggleState)
        {
            Console.WriteLine($"Clicked {actionId} - {toggleState} - {parameters}");
            var actionParameters = JsonSerializer.Deserialize<ActionParameters>(parameters);
            if (actionParameters.ActionType == "ExecuteProcess")
            {
                var executeProcessParameters = JsonSerializer.Deserialize<ExecuteProcessParameters>(parameters);

                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = executeProcessParameters.FileName;
                process.StartInfo.Arguments = executeProcessParameters.Arguments;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.WorkingDirectory = executeProcessParameters.WorkingDirectory;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.Start();
            } 
            else
            {
                Console.WriteLine($"Invalid Action Type: {actionParameters.ActionType}");
                SystemSounds.Exclamation.Play();
            }

        }

    }
}
