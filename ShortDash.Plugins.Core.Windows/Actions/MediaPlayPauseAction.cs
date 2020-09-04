using ShortDash.Core.Plugins;

namespace ShortDash.Plugins.Core.Windows
{
    public class MediaPlayPauseAction : KeyboardActionBase
    {
        private readonly IShortDashPluginLogger<MediaPlayPauseAction> logger;

        public MediaPlayPauseAction(IShortDashPluginLogger<MediaPlayPauseAction> logger)
        {
            this.logger = logger;
        }

        public override string Description => "Toggles the currently playing system media.";

        public override string Title => "Play/Pause Media";

        public override bool Execute(object parametersObject, ref bool toggleState)
        {
            logger.LogDebug("Sending play/pause keyboard events.");
            PressKey(0xB3 /* VK_MEDIA_PLAY_PAUSE */);
            return true;
        }
    }
}