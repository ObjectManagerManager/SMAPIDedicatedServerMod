using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace DedicatedServer.HostAutomatorStages
{
    internal abstract class HostAutomatorStage
    {
        protected IModHelper helper;

        public HostAutomatorStage(IModHelper helper)
        {
            this.helper = helper;
        }

        private void execute(object sender, UpdateTickedEventArgs e)
        { 
            if (!Game1.netWorldState.Value.IsPaused)
            {
                Execute(sender, e);
            }
        }

        public void Enable()
        {
            helper.Events.GameLoop.UpdateTicked += execute;
        }

        public void Disable()
        {
            helper.Events.GameLoop.UpdateTicked -= execute;
        }

        public abstract void Execute(object sender, UpdateTickedEventArgs e);
    }
}
