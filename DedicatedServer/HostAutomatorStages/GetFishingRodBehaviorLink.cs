using StardewValley;

namespace DedicatedServer.HostAutomatorStages
{
    internal class GetFishingRodBehaviorLink : BehaviorLink
    {
        public GetFishingRodBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            //If we don't get the fishing rod, Willy isn't available
            if (Game1.player.hasQuest(13) && !Game1.player.eventsSeen.Contains(739330) && Game1.timeOfDay == 900 && !Utility.isFestivalDay(Game1.Date.DayOfMonth, Game1.Date.Season))
            {
                Game1.warpFarmer("Beach", 1, 20, 1);
            } 
            processNext(state);
        }
    }
}
