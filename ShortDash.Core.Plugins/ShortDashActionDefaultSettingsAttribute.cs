using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Drawing;

namespace ShortDash.Core.Plugins
{
    public sealed class ShortDashActionDefaultSettingsAttribute : Attribute
    {
        public Color? BackgroundColor { get; set; } = null;
        public string Icon { get; set; } = "";
        public string Label { get; set; } = "";
        public Color? ToggleBackgroundColor { get; set; } = null;
        public string ToggleIcon { get; set; } = "";
        public string ToggleLabel { get; set; } = "";
    }
}