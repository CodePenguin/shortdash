﻿using ShortDash.Core.Plugins;
using System.Media;

namespace ShortDash.Plugins.Core.Windows
{
    public class MediaMuteAction : IShortDashAction
    {
        private readonly IShortDashPluginLogger<MediaMuteAction> logger;

        public MediaMuteAction(IShortDashPluginLogger<MediaMuteAction> logger)
        {
            this.logger = logger;
        }

        public bool Execute(string parameters, ref bool toggleState)
        {
            // TODO: Actually mute the volume
            logger.LogDebug($"Executing {GetType().Name}");
            SystemSounds.Exclamation.Play();
            return true;
        }
    }
}