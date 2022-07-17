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
    internal class UpdateStateBehaviorLink : BehaviorLink
    {
        public UpdateStateBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            state.UpdateOtherPlayers();
            processNext(state);
        }
    }
}
