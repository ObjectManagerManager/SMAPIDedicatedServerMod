using StardewValley;

namespace DedicatedServer.HostAutomatorStages
{
    internal class EndCommunityCenterBehaviorLink : BehaviorLink
    {
        private bool isEnding;

        public EndCommunityCenterBehaviorLink(BehaviorLink next = null) : base(next)
        {
            isEnding = false;
        }

        public override void Process(BehaviorState state)
        {
            if (!Game1.player.eventsSeen.Contains(191393) && Game1.player.hasCompletedCommunityCenter() && !Game1.IsRainingHere(Game1.getLocationFromName("Town")) && !isEnding && !Utility.isFestivalDay(Game1.Date.DayOfMonth, Game1.Date.Season))
            {
                Game1.warpFarmer("Town", 0, 54, 1);
                isEnding = true;
            }
            else if (isEnding && Game1.player.eventsSeen.Contains(191393)) {
                Game1.warpFarmer("Farm", 64, 10, 1);
                isEnding = false;
            }
            else
            {
                processNext(state);
            }
        }
    }
}
