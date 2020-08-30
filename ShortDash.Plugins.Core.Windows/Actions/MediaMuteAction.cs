using ShortDash.Core.Plugins;
using System;
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

        public string Description => "Mutes the currently playing system media.";

        public Type ParametersType => typeof(object);

        string IShortDashAction.Title => "Mute Media";

        public bool Execute(object parametersObject, ref bool toggleState)
        {
            // TODO: Actually mute the volume
            logger.LogDebug($"Executing {GetType().Name}");
            SystemSounds.Exclamation.Play();
            return true;
        }
    }
}