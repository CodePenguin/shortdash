using ShortDash.Core.Plugins;
using System;

namespace ShortDash.Plugins.Core.Windows
{
    public abstract class KeyboardActionBase : IShortDashAction
    {
        private const uint KeyEventKeyExtendedKey = 0x0001;

        private const uint KeyEventKeyUp = 0x0002;

        public ShortDashActionResult Execute(object parametersObject, bool toggleState)
        {
            ExecuteKeyboardAction();
            return new ShortDashActionResult { Success = true, ToggleState = toggleState };
        }

        public abstract void ExecuteKeyboardAction();

        protected static void PressKey(byte keyCode)
        {
            if (!TargetEnvironment.IsWindows)
            {
                throw new PlatformNotSupportedException("This operation is only supported on Windows.");
            }
            NativeMethods.KeyboardEvent(keyCode, 0, KeyEventKeyExtendedKey, IntPtr.Zero);
            NativeMethods.KeyboardEvent(keyCode, 0, KeyEventKeyUp, IntPtr.Zero);
        }
    }
}
