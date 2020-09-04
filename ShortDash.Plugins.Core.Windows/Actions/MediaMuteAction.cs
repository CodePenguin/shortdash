using ShortDash.Core.Plugins;

namespace ShortDash.Plugins.Core.Windows
{
    [ShortDashAction(
        Title = "Mute Media",
        Description = "Mutes the currently playing system media.")]
    public class MediaMuteAction : KeyboardActionBase
    {
        private readonly IShortDashPluginLogger<MediaMuteAction> logger;

        public MediaMuteAction(IShortDashPluginLogger<MediaMuteAction> logger)
        {
            this.logger = logger;
        }

        public override bool Execute(object parametersObject, ref bool toggleState)
        {
            logger.LogDebug("Sending mute volume keyboard events.");
            PressKey(0xAD /* VK_VOLUME_MUTE */);
            return true;
        }
    }
}