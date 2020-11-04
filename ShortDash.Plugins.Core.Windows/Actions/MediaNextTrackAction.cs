using ShortDash.Core.Plugins;

namespace ShortDash.Plugins.Core.Windows
{
    [ShortDashAction(
        Title = "Next Track [Windows]",
        Description = "Goes to the next track for the current system media.")]
    [ShortDashActionDefaultSettings(
        Label = "Next",
        Icon = "fas fa-step-forward")]
    public class MediaNextTrackAction : KeyboardActionBase
    {
        private readonly IShortDashPluginLogger<MediaNextTrackAction> logger;

        public MediaNextTrackAction(IShortDashPluginLogger<MediaNextTrackAction> logger)
        {
            this.logger = logger;
        }

        public override void ExecuteKeyboardAction()
        {
            logger.LogDebug("Sending next track keyboard events.");
            PressKey(0xB0 /* VK_MEDIA_NEXT_TRACK */);
        }
    }
}