using ShortDash.Core.Plugins;

namespace ShortDash.Plugins.Core.Windows
{
    [ShortDashAction(
        Title = "Play/Pause Media",
        Description = "Toggles the currently playing system media.")]
    public class MediaPlayPauseAction : KeyboardActionBase
    {
        private readonly IShortDashPluginLogger<MediaPlayPauseAction> logger;

        public MediaPlayPauseAction(IShortDashPluginLogger<MediaPlayPauseAction> logger)
        {
            this.logger = logger;
        }

        public override bool Execute(object parametersObject, ref bool toggleState)
        {
            logger.LogDebug("Sending play/pause keyboard events.");
            PressKey(0xB3 /* VK_MEDIA_PLAY_PAUSE */);
            return true;
        }
    }
}