using ShortDash.Core.Plugins;
using System.Diagnostics;
using System.Text.Json;

namespace ShortDash.Core.Actions
{
    public class ExecuteProcessAction : IShortDashAction
    {
        bool IShortDashAction.Execute(string parameters, ref bool toggleState)
        {
            var executeProcessParameters = JsonSerializer.Deserialize<ExecuteProcessParameters>(parameters);

            using var process = new Process();
            process.StartInfo.FileName = executeProcessParameters.FileName;
            process.StartInfo.Arguments = executeProcessParameters.Arguments;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WorkingDirectory = executeProcessParameters.WorkingDirectory;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = !System.IO.Path.GetExtension(executeProcessParameters.FileName).ToUpper().Equals(".EXE");
            process.Start();
            // TODO: Handle ExecuteProcess error scenarios
            return true;
        }
    }

    internal class ExecuteProcessParameters
    {
        public string Arguments { get; set; }
        public string FileName { get; set; }
        public string WorkingDirectory { get; set; }
    }
}