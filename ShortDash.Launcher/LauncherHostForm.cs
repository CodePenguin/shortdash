using System;
using System.Windows.Forms;

namespace ShortDash.Launcher
{
    public partial class LauncherHostForm : Form
    {
        private readonly Launcher launcher;

        public LauncherHostForm()
        {
            InitializeComponent();
            launcher = new Launcher(AppContext.BaseDirectory)
            {
                OnProcessTerminated = ProcessTerminatedEvent
            };
            launcher.Start();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LauncherHostForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            launcher.Stop();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            launcher.OpenProcessUrl();
        }

        private void OpenProcessUrlMenuItem_Click(object sender, EventArgs e)
        {
            launcher.OpenProcessUrl();
        }

        private void ProcessTerminatedEvent(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
            {
                Close();
            }));
        }

        private void ToggleProcessVisibilityMenuItem_Click(object sender, EventArgs e)
        {
            launcher.ToggleProcessVisibility();
            ToggleProcessVisibilityMenuItem.Text = (launcher.ProcessIsVisible ? "Hide" : "Show") + " Process";
        }
    }
}
