using DedicatedServer.HostAutomatorStages;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DedicatedServer.Utils
{
    internal abstract class Sleeping : SleepWorker
    {        
        private Sleeping() : base(null)
        {
        }

        /// <summary>
        /// <inheritdoc cref = "SleepWorker.ShouldSleepOverwrite"/>
        /// </summary>
        public static new bool ShouldSleepOverwrite
        {
            get { return SleepWorker.ShouldSleepOverwrite; }
            set { SleepWorker.ShouldSleepOverwrite = value; }
        }

    }
}
