using ShortDash.Core.Plugins;

namespace ShortDash.Plugins.Core.Windows
{
    [ShortDashAction(
        Title = "Mute Media [Windows]",
        Description = "Mutes the currently playing system media.")]
    [ShortDashActionDefaultSettings(
        Label = "Mute",
        Icon = "fas fa-volume-mute")]
    public class MediaMuteAction : KeyboardActionBase
    {
        private readonly IShortDashPluginLogger<MediaMuteAction> logger;

        public MediaMuteAction(IShortDashPluginLogger<MediaMuteAction> logger)
        {
            this.logger = logger;
        }

        public override void ExecuteKeyboardAction()
        {
            logger.LogDebug("Sending mute volume keyboard events.");
            PressKey(0xAD /* VK_VOLUME_MUTE */);
        }
    }
}