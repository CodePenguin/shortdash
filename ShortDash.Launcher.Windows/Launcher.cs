using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ShortDash.Launcher.Windows
{
    public sealed class Launcher : IDisposable
    {
        public EventHandler OnProcessTerminated;
        private const string EXE_SERVER = "ShortDash.Server.exe";
        private const string EXE_TARGET = "ShortDash.Target.exe";
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private readonly string basePath;
        private readonly string processExeName;
        private Process process;
        private string processUrl;

        public Launcher(string basePath)
        {
            this.basePath = basePath;
            processExeName = File.Exists(Path.Combine(basePath, EXE_TARGET)) ? EXE_TARGET : EXE_SERVER;
            LoadAppSettings();
        }

        public bool ProcessIsVisible { get; private set; }

        public void Dispose()
        {
            Stop();
        }

        public void OpenProcessUrl()
        {
            using var process = new Process();
            process.StartInfo.FileName = processUrl;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        public void Start()
        {
            process = new Process
            {
                EnableRaisingEvents = true
            };
            process.Exited += ProcessExitedEvent;
            process.StartInfo.FileName = processExeName;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = basePath;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();

            // Wait for the console process to start and create its window so it can be forced to hide
            while ((int)process.MainWindowHandle == 0 && !process.HasExited)
            {
                System.Threading.Thread.Sleep(100);
            }
            NativeMethods.ShowWindowAsync(process.MainWindowHandle, SW_HIDE);
        }

        public void Stop()
        {
            if (process == null)
            {
                return;
            }
            if (!process.HasExited)
            {
                process.Kill();
            }
            process.Dispose();
            process = null;
        }

        public void ToggleProcessVisibility()
        {
            var windowState = !ProcessIsVisible ? SW_SHOW : SW_HIDE;
            var result = NativeMethods.ShowWindowAsync(process.MainWindowHandle, windowState);
            if (result)
            {
                ProcessIsVisible = !ProcessIsVisible;
            }
        }

        private void LoadAppSettings()
        {
            var appSettingsFileName = Path.Combine(basePath, "appsettings.json");
            var appSettings = File.Exists(appSettingsFileName)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(appSettingsFileName))
                : new AppSettings();
            // Generate process URL
            var port = (processExeName == EXE_TARGET ? 5101 : 5100);
            var url = string.IsNullOrWhiteSpace(appSettings.Urls) ? $"http://localhost:{port}" : appSettings.Urls.Split(";")[0];
            url = url.Replace("*", "localhost");
            var uri = new Uri(url);
            processUrl = uri.ToString();
        }

        private void ProcessExitedEvent(object sender, EventArgs e)
        {
            Stop();
            OnProcessTerminated?.Invoke(this, e);
        }

        internal class AppSettings
        {
            public string Urls { get; set; }
        }
    }
}
