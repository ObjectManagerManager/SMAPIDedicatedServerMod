using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DedicatedServer.HostAutomatorStages
{
    internal class ReadyCheckHelper
    {
        private static Assembly assembly = typeof(Game1).Assembly;
        private static Type readyCheckType = assembly.GetType("StardewValley.ReadyCheck");
        private static Type netRefType = typeof(NetRef<>);
        private static Type readyCheckNetRefType = netRefType.MakeGenericType(readyCheckType);
        private static Type netStringDictionaryType = typeof(NetStringDictionary<,>);
        private static Type readyCheckDictionaryType = netStringDictionaryType.MakeGenericType(readyCheckType, readyCheckNetRefType);

        private static FieldInfo readyChecksFieldInfo = typeof(FarmerTeam).GetField("readyChecks", BindingFlags.NonPublic | BindingFlags.Instance);
        private static object readyChecks = null;

        private static MethodInfo readyChecksAddMethodInfo = readyCheckDictionaryType.GetMethod("Add", new Type[] { typeof(string), readyCheckType });
        private static PropertyInfo readyChecksItemPropertyInfo = readyCheckDictionaryType.GetProperty("Item");

        private static FieldInfo readyPlayersFieldInfo = readyCheckType.GetField("readyPlayers", BindingFlags.NonPublic | BindingFlags.Instance);

        private static Dictionary<string, NetFarmerCollection> readyPlayersDictionary = new Dictionary<string, NetFarmerCollection>();

        public static void OnDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (readyChecks == null)
            {
                readyChecks = readyChecksFieldInfo.GetValue(Game1.player.team);
            }

            //Checking mailbox sometimes gives some gold, but it's compulsory to unlock some events
            for (int i = 0; i < 10; ++i) {
                Game1.getFarm().mailbox();
            }

            //Unlocks the sewer
            if (!Game1.player.eventsSeen.Contains(295672) && Game1.netWorldState.Value.MuseumPieces.Count() >= 60) {
                Game1.player.eventsSeen.Add(295672);
            }

            //Upgrade farmhouse to match highest level cabin
            var targetLevel = Game1.getFarm().buildings.Where(o => o.isCabin).Select(o => ((Cabin)o.indoors.Value).upgradeLevel).DefaultIfEmpty(0).Max();
            if (targetLevel > Game1.player.HouseUpgradeLevel) {
                Game1.player.HouseUpgradeLevel = targetLevel;
                Game1.player.performRenovation("FarmHouse");
            }
            

            Dictionary<string, NetFarmerCollection> newReadyPlayersDictionary = new Dictionary<string, NetFarmerCollection>();
            foreach (var checkName in readyPlayersDictionary.Keys)
            {
                object readyCheck = null;
                try
                {
                    readyCheck = Activator.CreateInstance(readyCheckType, new object[] { checkName });
                    readyChecksAddMethodInfo.Invoke(readyChecks, new object[] { checkName, readyCheck });
                }
                catch (Exception)
                {
                    readyCheck = readyChecksItemPropertyInfo.GetValue(readyChecks, new object[] { checkName });
                }

                NetFarmerCollection readyPlayers = (NetFarmerCollection) readyPlayersFieldInfo.GetValue(readyCheck);
                newReadyPlayersDictionary.Add(checkName, readyPlayers);
            }
            readyPlayersDictionary = newReadyPlayersDictionary;
        }

        public static void WatchReadyCheck(string checkName)
        {
            readyPlayersDictionary.TryAdd(checkName, null);
        }

        // Prerequisite: OnDayStarted() must have been called at least once prior to this method being called.
        public static bool IsReady(string checkName, Farmer player)
        {
            if (readyPlayersDictionary.TryGetValue(checkName, out NetFarmerCollection readyPlayers) && readyPlayers != null)
            {
                return readyPlayers.Contains(player);
            }

            object readyCheck = null;
            try
            {
                readyCheck = Activator.CreateInstance(readyCheckType, new object[] { checkName });
                readyChecksAddMethodInfo.Invoke(readyChecks, new object[] { checkName, readyCheck });
            }
            catch (Exception)
            {
                readyCheck = readyChecksItemPropertyInfo.GetValue(readyChecks, new object[] { checkName });
            }

            readyPlayers = (NetFarmerCollection) readyPlayersFieldInfo.GetValue(readyCheck);
            if (readyPlayersDictionary.ContainsKey(checkName))
            {
                readyPlayersDictionary[checkName] = readyPlayers;
            } else
            {
                readyPlayersDictionary.Add(checkName , readyPlayers);
            }

            return readyPlayers.Contains(player);
        }
    }
}
