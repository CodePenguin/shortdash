using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class ShortActionService
    {

        private static readonly string[] ButtonLabels = new[]
        {
            "Twitter", "Terminal", "Mute", "Next", "Back", "Restart", "100%", "Hot", "Batch", "Discord"
        };

        public Task<ShortAction[]> GetActionButtonsAsync()
        {
            var rng = new Random();
            return Task.FromResult(Enumerable.Range(1, 25).Select(index => new ShortAction
            {
                Label = ButtonLabels[rng.Next(ButtonLabels.Length)]
            }).ToArray());
        }

    }
}
