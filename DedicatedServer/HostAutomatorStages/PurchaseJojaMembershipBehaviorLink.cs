using DedicatedServer.Config;
using StardewValley;

namespace DedicatedServer.HostAutomatorStages
{
    internal class PurchaseJojaMembershipBehaviorLink : BehaviorLink
    {
        private ModConfig config;

        public PurchaseJojaMembershipBehaviorLink(ModConfig config, BehaviorLink next = null) : base(next) {
            this.config = config;
        }

        public override void Process(BehaviorState state)
        {
            // If the community center has been unlocked, the config specifies that the host
            // should purchase the joja membership, and the host has not yet purchased it...
            var ccAvailable = Game1.player.eventsSeen.Contains(611439);
            var purchased = Game1.player.mailForTomorrow.Contains("JojaMember%&NL&%") || Game1.player.mailReceived.Contains("JojaMember");
            if (ccAvailable && config.PurchaseJojaMembership && !purchased)
            {
                // Then purchase it
                Game1.addMailForTomorrow("JojaMember", noLetter: true, sendToEveryone: true);
                Game1.player.removeQuest(26);
            }
            else
            {
                processNext(state);
            }
        }
    }
}
