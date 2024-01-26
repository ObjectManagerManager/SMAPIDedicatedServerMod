using DedicatedServer.Chat;
using DedicatedServer.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class SleepWorker
    {
        private static IModHelper helper = null;

        private static bool _ShouldSleepOverwrite = false;

        public SleepWorker(IModHelper helper)
        {
            SleepWorker.helper = helper;
        }

        /// <summary>
        ///         Checks whether the host is sleeping
        /// </summary>
        /// <returns>
        ///         true : The host is sleeping
        /// <br/>   false: The host is not asleep
        /// </returns>
        public static bool IsSleeping()
        {
            return ReadyCheckHelper.IsReady("sleep", Game1.player);
        }

        /// <summary>
        ///         Checks whether all players are asleep
        /// </summary>
        /// <param name="numOtherPlayers">Number of players</param>
        /// <returns>
        ///         true : All players are sleeping
        /// <br/>   false: Not all players are sleeping
        /// </returns>
        public static bool OthersInBed(int numOtherPlayers)
        {
            return Game1.player.team.GetNumberReady("sleep") == (numOtherPlayers + (IsSleeping() ? 1 : 0));
        }

        /// <summary>
        ///         Checks whether the host should go to bed
        /// </summary>
        /// <param name="numOtherPlayers">Number of players</param>
        /// <returns>
        ///         true : The host should go to bed
        /// <br/>   false: The host should not go to bed</returns>
        public static bool ShouldSleep(int numOtherPlayers)
        {
            return (numOtherPlayers > 0 && (Game1.timeOfDay >= 2530 || OthersInBed(numOtherPlayers))) || ShouldSleepOverwrite;
        }

        /// <summary>
        ///         Changes the behavior of the <see cref="ShouldSleep"/> function.
        /// <br/>   
        /// <br/>   true : The host should go to bed
        /// <br/>   false: The host should not go to bed or get up
        /// <br/>   
        /// <br/>   When all players leave the game, the next day is started.
        /// <br/>   
        /// <br/>   If the host is controlled by a player, the command must not
        /// <br/>   be executed.
        /// </summary>
        protected static bool ShouldSleepOverwrite
        {
            set
            {
                if (value)
                {
                    if (HostAutomation.EnableHostAutomation)
                    {
                        AddOnDayStarted(OnDayStartedWorker);
                        HostAutomation.PreventPause = true;
                        _ShouldSleepOverwrite = true;
                    }
                }
                else
                {
                    _ShouldSleepOverwrite = false;
                    AddOneSecondUpdateTicked(OnOneSecondUpdateTicked);
                }
            }
            get
            {
                return _ShouldSleepOverwrite;
            }
        }

        private static void AddOnDayStarted(EventHandler<DayStartedEventArgs> handler)
        {
            helper.Events.GameLoop.DayStarted += handler;
        }

        private static void RemoveOnDayStarted(EventHandler<DayStartedEventArgs> handler)
        {
            helper.Events.GameLoop.DayStarted -= handler;
        }

        private static void AddOneSecondUpdateTicked(EventHandler<OneSecondUpdateTickedEventArgs> handler)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked += handler;
        }

        private static void RemoveOneSecondUpdateTicked(EventHandler<OneSecondUpdateTickedEventArgs> handler)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked -= handler;
        }

        /// <summary>
        ///         Waits until the host is back on his feet before the handler
        /// <br/>   <see cref="OnDayStartedWorker"/> is removed.
        /// <br/>   
        /// <br/>   Deactivating Sleep restores the normal behavior of the mod,
        /// <br/>   when the host is not controlled by a player.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (false == IsSleeping())
            {
                RemoveOneSecondUpdateTicked(OnOneSecondUpdateTicked);
                RemoveOnDayStarted(OnDayStartedWorker);
                if (HostAutomation.EnableHostAutomation)
                {
                    HostAutomation.TakeOver();
                }
            }
        }

        /// <summary>
        ///         Resets the variable <see cref="ShouldSleepOverwrite"/> at the beginning of the day.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDayStartedWorker(object sender, DayStartedEventArgs e)
        {
            ShouldSleepOverwrite = false;
        }
    }
}
