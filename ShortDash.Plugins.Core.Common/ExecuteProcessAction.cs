using ShortDash.Core.Plugins;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ShortDash.Plugins.Core.Common
{
    [ShortDashAction(
        Title = "Execute Process",
        Description = "Execute processes, documents and links via the system shell.",
        ParametersType = typeof(ExecuteProcessParameters))]
    public class ExecuteProcessAction : IShortDashAction
    {
        private readonly IShortDashPluginLogger<ExecuteProcessAction> logger;

        public ExecuteProcessAction(IShortDashPluginLogger<ExecuteProcessAction> logger)
        {
            this.logger = logger;
        }

        public ShortDashActionResult Execute(object parametersObject, bool toggleState)
        {
            var parameters = parametersObject as ExecuteProcessParameters;
            using var process = new Process();
            process.StartInfo.FileName = parameters.FileName;
            process.StartInfo.Arguments = parameters.Arguments;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WorkingDirectory = parameters.WorkingDirectory;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = true;
            try
            {
                process.Start();
                process.WaitForExit();
                logger.LogDebug($"Process exited with code {process.ExitCode}");
                return new ShortDashActionResult { Success = true, ToggleState = toggleState };
            }
            catch (Exception ex)
            {
                logger.LogError($"Unable to execute process: {ex}");
                return new ShortDashActionResult { UserMessage = ex.Message };
            }
        }
    }

    public class ExecuteProcessParameters
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