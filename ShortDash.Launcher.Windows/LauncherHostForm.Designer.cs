
namespace ShortDash.Launcher.Windows
{
    partial class LauncherHostForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LauncherHostForm));
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.IconMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenProcessUrlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToggleProcessVisibilityMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.IconMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.ContextMenuStrip = this.IconMenu;
            this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
            this.NotifyIcon.Text = "ShortDash";
            this.NotifyIcon.Visible = true;
            this.NotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // IconMenu
            // 
            this.IconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenProcessUrlMenuItem,
            this.MenuSeparator1,
            this.ToggleProcessVisibilityMenuItem,
            this.MenuSeparator2,
            this.ExitMenuItem});
            this.IconMenu.Name = "iconMenu";
            this.IconMenu.Size = new System.Drawing.Size(147, 82);
            // 
            // OpenProcessUrlMenuItem
            // 
            this.OpenProcessUrlMenuItem.Name = "OpenProcessUrlMenuItem";
            this.OpenProcessUrlMenuItem.Size = new System.Drawing.Size(146, 22);
            this.OpenProcessUrlMenuItem.Text = "Open";
            this.OpenProcessUrlMenuItem.Click += new System.EventHandler(this.OpenProcessUrlMenuItem_Click);
            // 
            // MenuSeparator1
            // 
            this.MenuSeparator1.Name = "MenuSeparator1";
            this.MenuSeparator1.Size = new System.Drawing.Size(143, 6);
            // 
            // ToggleProcessVisibilityMenuItem
            // 
            this.ToggleProcessVisibilityMenuItem.Name = "ToggleProcessVisibilityMenuItem";
            this.ToggleProcessVisibilityMenuItem.Size = new System.Drawing.Size(146, 22);
            this.ToggleProcessVisibilityMenuItem.Text = "Show Process";
            this.ToggleProcessVisibilityMenuItem.Click += new System.EventHandler(this.ToggleProcessVisibilityMenuItem_Click);
            // 
            // MenuSeparator2
            // 
            this.MenuSeparator2.Name = "MenuSeparator2";
            this.MenuSeparator2.Size = new System.Drawing.Size(143, 6);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(146, 22);
            this.ExitMenuItem.Text = "E&xit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // LauncherHostForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "LauncherHostForm";
            this.ShowInTaskbar = false;
            this.Text = "ShortDash Launcher";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LauncherHostForm_FormClosed);
            this.IconMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Windows.Forms.ContextMenuStrip IconMenu;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToggleProcessVisibilityMenuItem;
        private System.Windows.Forms.ToolStripSeparator MenuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem OpenProcessUrlMenuItem;
        private System.Windows.Forms.ToolStripSeparator MenuSeparator1;
    }
}

