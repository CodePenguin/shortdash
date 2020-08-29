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
            var actionParameters = JsonSerializer.Deserialize<ActionParameters>(action.Parameters);
            if (action.ActionClass == "DashLink")
            {
                var dashLinkParameters = JsonSerializer.Deserialize<DashLinkProcessParameters>(action.Parameters);
                Console.WriteLine($"Clicked dash link button ID {dashLinkParameters.DashboardId}");
                navigationManager.NavigateTo($"/dashboard/{dashLinkParameters.DashboardId}");
            }
            else if (action.ActionClass == "ExecuteProcess")
            {
                var executeProcessParameters = JsonSerializer.Deserialize<ExecuteProcessParameters>(action.Parameters);

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
                Console.WriteLine($"Unhandled Action Class: {action.ActionClass}");
                SystemSounds.Exclamation.Play();
            }

        }

    }
}
