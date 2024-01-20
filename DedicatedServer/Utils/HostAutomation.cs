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
    }
}
