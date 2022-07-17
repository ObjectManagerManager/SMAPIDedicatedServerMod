using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class SkipShippingMenuBehaviorLink : BehaviorLink
    {
        private static MethodInfo info = typeof(ShippingMenu).GetMethod("okClicked", BindingFlags.Instance | BindingFlags.NonPublic);
        
        public SkipShippingMenuBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            if (Game1.activeClickableMenu is ShippingMenu sm)
            {
                if (state.HasBetweenShippingMenusWaitTicks())
                {
                    state.DecrementBetweenShippingMenusWaitTicks();
                } else
                {
                    info.Invoke(sm, new object[]{});
                    state.SkipShippingMenu();
                }
            } else
            {
                state.ClearBetweenShippingMenusWaitTicks();
                processNext(state);
            }
        }
    }
}
