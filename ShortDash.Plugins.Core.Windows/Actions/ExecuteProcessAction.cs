using ShortDash.Core.Plugins;
using System;
using System.Diagnostics;

namespace ShortDash.Plugins.Core.Windows
{
    public class ExecuteProcessAction : IShortDashAction
    {
        private readonly IShortDashPluginLogger<ExecuteProcessAction> logger;

        public ExecuteProcessAction(IShortDashPluginLogger<ExecuteProcessAction> logger)
        {
            this.logger = logger;
        }

        string IShortDashAction.Description => "Execute processes, documents and links via the system shell.";

        Type IShortDashAction.ParametersType => typeof(ExecuteProcessParameters);

        string IShortDashAction.Title => "Execute Process (Windows)";

        bool IShortDashAction.Execute(object parametersObject, ref bool toggleState)
        {
            var parameters = parametersObject as ExecuteProcessParameters;

            logger.LogDebug($"Executing {parameters.FileName}.");
            using var process = new Process();
            process.StartInfo.FileName = parameters.FileName;
            process.StartInfo.Arguments = parameters.Arguments;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WorkingDirectory = parameters.WorkingDirectory;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = !System.IO.Path.GetExtension(parameters.FileName).ToUpper().Equals(".EXE");
            process.Start();
            // TODO: Handle ExecuteProcess error scenarios
            return true;
        }
    }

    public class ExecuteProcessParameters
    {
        public string Arguments { get; set; }
        public string FileName { get; set; }
        public string WorkingDirectory { get; set; }
    }
}