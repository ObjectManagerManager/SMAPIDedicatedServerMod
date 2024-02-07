using DedicatedServer.HostAutomatorStages;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.Utils
{
    internal abstract class RestartDay : RestartDayWorker
    {
        private RestartDay() : base(null)
        {
        }

        /// <summary>
        ///         Kicks all players and starts the next day
        /// </summary>
        /// <param name="action">
        ///         Function that is executed every second until the
        /// <br/>   event is executed. The parameter is the remaining time.</param>
        public static void ForceSleep(Action<int> action)
        {
            RestartDayWorker.SavesGameRestartsDay(
                time: 10,
                keepsCurrentDay: false,
                quit: false,
                action: action);
        }

        /// <summary>
        ///         Kicks all players and restarts the day
        /// </summary>
        /// <param name="action">
        ///         Function that is executed every second until the
        /// <br/>   event is executed. The parameter is the remaining time.</param>
        public static void ResetDay(Action<int> action)
        {
            RestartDayWorker.SavesGameRestartsDay(
                time: 10,
                keepsCurrentDay: true,
                quit: false,
                action: action);
        }

        /// <summary>
        ///         Kicks all players and starts a new day
        /// </summary>
        /// <param name="action">
        ///         Function that is executed every second until the
        /// <br/>   event is executed. The parameter is the remaining time.</param>
        public static void ShutDown(Action<int> action)
        {
            RestartDayWorker.SavesGameRestartsDay(
                time: 10,
                keepsCurrentDay: false,
                quit: true,
                action: action);
        }
    }
}
