using DedicatedServer.HostAutomatorStages;
using Netcode;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DedicatedServer.Utils
{
    internal class Sleeping
    {
        public static bool IsSleeping()
        {
            return ReadyCheckHelper.IsReady("sleep", Game1.player);
        }
        public static bool OthersInBed(int numOtherPlayers)
        {
            return Game1.player.team.GetNumberReady("sleep") == (numOtherPlayers + (IsSleeping() ? 1 : 0));
        }
        public static bool ShouldSleep(int numOtherPlayers)
        {
            return numOtherPlayers > 0 && (Game1.timeOfDay >= 2530 || OthersInBed(numOtherPlayers));
        }
    }
}
