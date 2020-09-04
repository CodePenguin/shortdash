using ShortDash.Core.Plugins;

namespace ShortDash.Plugins.Core.Windows
{
    [ShortDashAction(
        Title = "Previous Track",
        Description = "Goes to the previous track for the current system media.")]
    public class MediaPreviousTrackAction : KeyboardActionBase
    {
        private readonly IShortDashPluginLogger<MediaPreviousTrackAction> logger;

        public MediaPreviousTrackAction(IShortDashPluginLogger<MediaPreviousTrackAction> logger)
        {
            this.logger = logger;
        }

        public override bool Execute(object parametersObject, ref bool toggleState)
        {
            logger.LogDebug("Sending previous track keyboard events.");
            PressKey(0xB1 /* VK_MEDIA_PREV_TRACK */);
            return true;
        }
    }
}