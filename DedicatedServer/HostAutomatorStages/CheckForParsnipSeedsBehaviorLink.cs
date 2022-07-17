using DedicatedServer.Chat;
using DedicatedServer.Config;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DedicatedServer.HostAutomatorStages
{
    internal class CheckForParsnipSeedsBehaviorLink : BehaviorLink
    {
        public CheckForParsnipSeedsBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            if (!state.ExitedFarmhouse() && !state.HasCheckedForParsnipSeeds() && Game1.currentLocation is FarmHouse fh)
            {
                state.CheckForParsnipSeeds();
                foreach (var kvp in fh.Objects.Pairs)
                {
                    var obj = kvp.Value;
                    if (obj is Chest chest)
                    {
                        if (chest.giftbox.Value)
                        {
                            chest.checkForAction(Game1.player);
                            state.SetWaitTicks(60 * 2);
                            break;
                        }
                    }
                }
            } else
            {
                processNext(state);
            }

        }
    }
}
