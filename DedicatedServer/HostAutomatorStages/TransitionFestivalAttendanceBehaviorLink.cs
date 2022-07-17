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
    internal class TransitionFestivalAttendanceBehaviorLink : BehaviorLink
    {
        private static MethodInfo info = typeof(Game1).GetMethod("performWarpFarmer", BindingFlags.Static | BindingFlags.NonPublic);
        public TransitionFestivalAttendanceBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        private static string getLocationOfFestival()
        {
            if (Game1.weatherIcon == 1)
            {
                return Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[0];
            }

            return null;
        }

        public override void Process(BehaviorState state)
        {
            if (Utils.Festivals.ShouldAttend(state.GetNumOtherPlayers()) && !Utils.Festivals.IsWaitingToAttend())
            {
                if (state.HasBetweenTransitionFestivalAttendanceWaitTicks())
                {
                    state.DecrementBetweenTransitionFestivalAttendanceWaitTicks();
                } else
                {
                    var location = Game1.getLocationFromName(getLocationOfFestival());
                    var warp = new Warp(0, 0, location.NameOrUniqueName, 0, 0, false);
                    Game1.player.team.SetLocalReady("festivalStart", ready: true);
                    Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", allowCancel: true, delegate (Farmer who)
                    {
                        Game1.exitActiveMenu();
                        info.Invoke(null, new object[] { Game1.getLocationRequest(warp.TargetName), 0, 0, Game1.player.facingDirection.Value });
                        if ((Game1.currentSeason != "fall" || Game1.dayOfMonth != 27) && (Game1.currentSeason != "winter" || Game1.dayOfMonth != 25)) // Don't enable chat box on spirit's even nor feast of the winter star
                        {
                            state.EnableFestivalChatBox();
                        }
                    });
                    state.WaitForFestivalAttendance();
                }
            } else if (!Utils.Festivals.ShouldAttend(state.GetNumOtherPlayers()) && Utils.Festivals.IsWaitingToAttend())
            {
                if (state.HasBetweenTransitionFestivalAttendanceWaitTicks())
                {
                    state.DecrementBetweenTransitionFestivalAttendanceWaitTicks();
                } else
                {
                    if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog rcd)
                    {
                        rcd.closeDialog(Game1.player);
                    }
                    Game1.player.team.SetLocalReady("festivalStart", false);
                    state.StopWaitingForFestivalAttendance();
                }
            }
            else
            {
                state.ClearBetweenTransitionFestivalAttendanceWaitTicks();
                processNext(state);
            }
        }
    }
}
