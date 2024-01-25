using DedicatedServer.HostAutomatorStages;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.Utils
{
    internal abstract class HostAutomation : ProcessPauseBehaviorLink
    {
        /// <summary>
        /// Alias for <c>Game1.netWorldState.Value.IsPaused</c>
        /// </summary>
        public static bool IsPaused
        {
            get { return Game1.netWorldState.Value.IsPaused; }
            set { Game1.netWorldState.Value.IsPaused = value; }
        }
        /// <summary>
        /// <inheritdoc cref = "ProcessPauseBehaviorLink.preventPause"/>
        /// </summary>
        public static bool PreventPause
        {
            get { return ProcessPauseBehaviorLink.preventPause; }
            set { ProcessPauseBehaviorLink.preventPause = value; }
        }

        /// <summary>
        /// <inheritdoc cref = "ProcessPauseBehaviorLink.enableHostAutomation"/>
        /// </summary>
        public static bool EnableHostAutomation
        {
            get { return ProcessPauseBehaviorLink.enableHostAutomation; }
            set { ProcessPauseBehaviorLink.enableHostAutomation = value; }
        }

        /// <summary>
        ///         Lets the player take over the host. All host functions are switched off.
        /// </summary>
        public static void LetMePlay()
        {
            EnableHostAutomation = false;
            PreventPause = true;
            Invincible.InvincibilityOverwrite = false;
        }

        /// <summary>
        ///         The player returns control to the host, all host functions are switched on.   
        /// </summary>
        public static void TakeOver()
        {
            EnableHostAutomation = true;
            PreventPause = false;
            Invincible.InvincibilityOverwrite = null;
        }

        /// <summary>
        ///         Lets the player take over the host but all host functions are switched on.
        /// <br/>   The main reason for this setting is debugging
        /// </summary>
        public static void LetMeControl()
        {
            EnableHostAutomation = true;
            PreventPause = true;
        }
    }
}
