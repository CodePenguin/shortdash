using System;
using System.Diagnostics;
using System.Media;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace ShortDash.Server.Data
{
    public class ActionProcessorService
    {
        private class ActionParameters
        {
            public string ActionType { get; set; }
            public string ActionTarget { get; set; }
        }

        private class DashLinkProcessParameters
        {
            public int DashboardId { get; set; }
        }

        private class ExecuteProcessParameters
        {
            public string FileName { get; set; }
            public string Arguments { get; set; }
            public string WorkingDirectory { get; set; }
        }

        private readonly NavigationManager navigationManager;

        public ActionProcessorService(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        public void Execute(DashboardAction action, bool toggleState)
        {
            Console.WriteLine($"Clicked {action.DashboardActionId} - {toggleState} - {action.Parameters}");
            if (action.ActionClass == "DashLink")
            {
                var dashLinkParameters = JsonSerializer.Deserialize<DashLinkProcessParameters>(action.Parameters);
                Console.WriteLine($"Clicked dash link button ID {dashLinkParameters.DashboardId}");
                navigationManager.NavigateTo($"/dashboard/{dashLinkParameters.DashboardId}");
            }
            else if (action.ActionClass == "ExecuteProcess")
            {
                var executeProcessParameters = JsonSerializer.Deserialize<ExecuteProcessParameters>(action.Parameters);

                using (var process = new Process())
                {
                    process.StartInfo.FileName = executeProcessParameters.FileName;
                    process.StartInfo.Arguments = executeProcessParameters.Arguments;
                    process.StartInfo.RedirectStandardOutput = false;
                    process.StartInfo.WorkingDirectory = executeProcessParameters.WorkingDirectory;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.StartInfo.UseShellExecute = !System.IO.Path.GetExtension(executeProcessParameters.FileName).ToUpper().Equals(".EXE");
                    process.Start();
                    // TODO: Handle ExecuteProcess error scenarios
                }
            } 
            else
            {
                Console.WriteLine($"Unhandled Action Class: {action.ActionClass}");
                SystemSounds.Exclamation.Play();
            }

        }

    }
}
