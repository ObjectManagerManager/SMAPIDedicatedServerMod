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
    internal class TransitionFestivalEndBehaviorLink : BehaviorLink
    {
        public TransitionFestivalEndBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            if (Utils.Festivals.ShouldLeave(state.GetNumOtherPlayers()) && !Utils.Festivals.IsWaitingToLeave())
            {
                if (state.HasBetweenTransitionFestivalEndWaitTicks())
                {
                    state.DecrementBetweenTransitionFestivalEndWaitTicks();
                } else
                {
                    Game1.player.team.SetLocalReady("festivalEnd", ready: true);
                    Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", allowCancel: true, delegate (Farmer who)
                    {
                        Game1.currentLocation.currentEvent.forceEndFestival(who);
                        state.DisableFestivalChatBox();
                    });
                    state.WaitForFestivalEnd();
                }
            } else if (!Utils.Festivals.ShouldLeave(state.GetNumOtherPlayers()) && Utils.Festivals.IsWaitingToLeave())
            {
                if (state.HasBetweenTransitionFestivalEndWaitTicks())
                {
                    state.DecrementBetweenTransitionFestivalEndWaitTicks();
                } else
                {
                    if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog rcd)
                    {
                        rcd.closeDialog(Game1.player);
                    }
                    Game1.player.team.SetLocalReady("festivalEnd", false);
                    state.StopWaitingForFestivalEnd();
                }
            }
            else
            {
                state.ClearBetweenTransitionFestivalEndWaitTicks();
                processNext(state);
            }
        }
    }
}
