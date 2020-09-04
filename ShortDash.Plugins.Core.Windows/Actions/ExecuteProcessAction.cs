using ShortDash.Core.Plugins;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ShortDash.Plugins.Core.Windows
{
    [ShortDashAction(
        Title = "Execute Process (Windows)",
        Description = "Execute processes, documents and links via the system shell.",
        ParametersType = typeof(ExecuteProcessParameters))]
    public class ExecuteProcessAction : IShortDashAction
    {
        private readonly IShortDashPluginLogger<ExecuteProcessAction> logger;

        public ExecuteProcessAction(IShortDashPluginLogger<ExecuteProcessAction> logger)
        {
            this.logger = logger;
        }

        public bool Execute(object parametersObject, ref bool toggleState)
        {
            var parameters = parametersObject as ExecuteProcessParameters;

            logger.LogDebug($"Executing {parameters.FileName}.");
            using var process = new Process();
            process.StartInfo.FileName = parameters.FileName;
            process.StartInfo.Arguments = parameters.Arguments;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WorkingDirectory = parameters.WorkingDirectory;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            // TODO: Handle ExecuteProcess error scenarios
            return true;
        }
    }

    public class ExecuteProcessParameters : ShortDashActionParameters
    {
        [Display(Order = 3)]
        public string Arguments { get; set; }

        [Required]
        [Display(
            Name = "File Name",
            Order = 1,
            Description = "The location and file name of the application to start, a document, or a URL.",
            Prompt = "Enter a file name or URL")]
        public string FileName { get; set; }

        [Display(Name = "Working Directory", Order = 2)]
        public string WorkingDirectory { get; set; }
    }
}