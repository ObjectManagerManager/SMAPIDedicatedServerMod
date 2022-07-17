using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class ProcessWaitTicksBehaviorLink : BehaviorLink
    {
        public ProcessWaitTicksBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            if (state.HasWaitTicks())
            {
                state.DecrementWaitTicks();
            } else
            {
                processNext(state);
            }
        }
    }
}
