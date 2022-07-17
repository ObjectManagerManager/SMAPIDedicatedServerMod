using DedicatedServer.HostAutomatorStages;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.Utils
{
    internal class Festivals
    {
        private static int getFestivalEndTime()
        {
            if (Game1.weatherIcon == 1)
            {
                return Convert.ToInt32(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[1].Split(' ')[1]);
            }

            return -1;
        }
        public static bool IsWaitingToAttend()
        {
            return ReadyCheckHelper.IsReady("festivalStart", Game1.player);
        }
        public static bool OthersWaitingToAttend(int numOtherPlayers)
        {
            return Game1.player.team.GetNumberReady("festivalStart") == (numOtherPlayers + (IsWaitingToAttend() ? 1 : 0));
        }
        private static bool isTodayBeachNightMarket()
        {
            return Game1.currentSeason.Equals("winter") && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17;
        }
        public static bool ShouldAttend(int numOtherPlayers)
        {
            return numOtherPlayers > 0 && OthersWaitingToAttend(numOtherPlayers) && Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && !isTodayBeachNightMarket() && Game1.timeOfDay >= Utility.getStartTimeOfFestival() && Game1.timeOfDay <= getFestivalEndTime();
        }

        public static bool IsWaitingToLeave()
        {
            return ReadyCheckHelper.IsReady("festivalEnd", Game1.player);
        }
        public static bool OthersWaitingToLeave(int numOtherPlayers)
        {
            return Game1.player.team.GetNumberReady("festivalEnd") == (numOtherPlayers + (IsWaitingToLeave() ? 1 : 0));
        }
        public static bool ShouldLeave(int numOtherPlayers)
        {
            return Game1.isFestival() && OthersWaitingToLeave(numOtherPlayers);
        }
    }
}
