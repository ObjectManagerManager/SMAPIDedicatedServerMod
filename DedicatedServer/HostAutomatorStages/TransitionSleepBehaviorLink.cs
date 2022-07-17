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
    internal class TransitionSleepBehaviorLink : BehaviorLink
    {
        private static MethodInfo info = typeof(GameLocation).GetMethod("doSleep", BindingFlags.Instance | BindingFlags.NonPublic);

        public TransitionSleepBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            if (Utils.Sleeping.ShouldSleep(state.GetNumOtherPlayers()) && !Utils.Sleeping.IsSleeping())
            {
                if (state.HasBetweenTransitionSleepWaitTicks())
                {
                    state.DecrementBetweenTransitionSleepWaitTicks();
                }
                else if (Game1.currentLocation is FarmHouse)
                {
                    Game1.player.isInBed.Value = true;
                    Game1.player.sleptInTemporaryBed.Value = true;
                    Game1.player.timeWentToBed.Value = Game1.timeOfDay;
                    Game1.player.team.SetLocalReady("sleep", ready: true);
                    Game1.dialogueUp = false;
                    Game1.activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: true, delegate
                    {
                        Game1.player.isInBed.Value = true;
                        Game1.player.sleptInTemporaryBed.Value = true;
                        info.Invoke(Game1.currentLocation, new object[]{});
                    }, delegate (Farmer who)
                    {
                        if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog rcd)
                        {
                            rcd.closeDialog(who);
                        }

                        who.timeWentToBed.Value = 0;
                    });

                    if (!Game1.player.team.announcedSleepingFarmers.Contains(Game1.player))
                        Game1.player.team.announcedSleepingFarmers.Add(Game1.player);

                    state.Sleep();
                }
                else
                {
                    var farmHouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                    var entryLocation = farmHouse.getEntryLocation();
                    var warp = new Warp(entryLocation.X, entryLocation.Y, farmHouse.NameOrUniqueName, entryLocation.X, entryLocation.Y, false);
                    Game1.player.warpFarmer(warp);
                    state.WarpToSleep();
                }
            } else if (!Utils.Sleeping.ShouldSleep(state.GetNumOtherPlayers()) && Utils.Sleeping.IsSleeping())
            {
                if (state.HasBetweenTransitionSleepWaitTicks())
                {
                    state.DecrementBetweenTransitionSleepWaitTicks();
                }
                else
                {
                    if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog rcd)
                    {
                        rcd.closeDialog(Game1.player);
                    }
                    Game1.player.team.SetLocalReady("sleep", false);
                    state.CancelSleep();
                }
            }
            else
            {
                state.ClearBetweenTransitionSleepWaitTicks();
                processNext(state);
            }
        }
    }
}
