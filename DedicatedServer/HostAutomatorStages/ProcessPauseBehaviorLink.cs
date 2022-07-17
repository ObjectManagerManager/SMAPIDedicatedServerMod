using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class ProcessPauseBehaviorLink : BehaviorLink
    {
        private bool paused = false;

        public ProcessPauseBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            if (!Game1.netWorldState.Value.IsPaused)
            {
                paused = false;
            }

            if (state.GetNumOtherPlayers() == 0 && !Game1.isFestival() && !Game1.netWorldState.Value.IsPaused)
            {
                paused = true;
                Game1.netWorldState.Value.IsPaused = true;
            }
            else if ((state.GetNumOtherPlayers() > 0 && paused) || (Game1.isFestival() && Game1.netWorldState.Value.IsPaused))
            {
                paused = false;
                Game1.netWorldState.Value.IsPaused = false;
            }
            else if (!Game1.netWorldState.Value.IsPaused)
            {
                processNext(state);
            }
        }
    }
}
