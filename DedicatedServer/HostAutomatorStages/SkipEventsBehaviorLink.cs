using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class SkipEventsBehaviorLink : BehaviorLink
    {
        public SkipEventsBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            if (Game1.CurrentEvent != null && Game1.CurrentEvent.skippable)
            {
                if (state.HasBetweenEventsWaitTicks())
                {
                    state.DecrementBetweenEventsWaitTicks();
                }
                else
                {
                    Game1.CurrentEvent.skipEvent();
                    state.SkipEvent(); // Set up wait ticks to wait before trying to skip event again, and wait to anticipate another following event
                }
            } else
            {
                state.ClearBetweenEventsWaitTicks();
                processNext(state);
            }
        }
    }
}
