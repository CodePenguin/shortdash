using System;
using System.Runtime.InteropServices;

namespace ShortDash.Plugins.Core.Windows
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        internal static extern void KeyboardEvent(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
    }
}
