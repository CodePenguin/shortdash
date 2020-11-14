using System;
using System.Runtime.InteropServices;

namespace ShortDash.Launcher
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    }
}
