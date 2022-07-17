using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class ExitFarmHouseBehaviorLink : BehaviorLink
    {
        public ExitFarmHouseBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            if (!state.ExitedFarmhouse() && Game1.currentLocation != null && Game1.currentLocation is FarmHouse)
            {
                var farm = Game1.getLocationFromName("Farm") as Farm;
                var warp = new Warp(64, 15, farm.NameOrUniqueName, 64, 15, false); // 64, 15 coords are "magic numbers" pulled from Game1.cs, line 11282, warpFarmer()
                Game1.player.warpFarmer(warp);
                state.ExitFarmhouse(); // Mark as exited
                state.SetWaitTicks(60); // Set up wait ticks to wait for possible event
            }
            else
            {
                processNext(state);
            }
        }
    }
}
