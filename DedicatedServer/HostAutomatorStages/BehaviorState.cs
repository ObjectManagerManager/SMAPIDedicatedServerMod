using DedicatedServer.Chat;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class BehaviorState
    {
        private const int startOfDayWaitTicks = 60;
        private static FieldInfo multiplayerFieldInfo = typeof(Game1).GetField("multiplayer", BindingFlags.NonPublic | BindingFlags.Static);
        private static Multiplayer multiplayer = null;

        private int betweenEventsWaitTicks = 0;
        private int betweenDialoguesWaitTicks = 0;
        private int betweenShippingMenusWaitTicks = 0;
        private bool checkedForParsnipSeeds = false;
        private bool exitedFarmhouse = false;
        private int betweenTransitionSleepWaitTicks = 0;
        private int betweenTransitionFestivalAttendanceWaitTicks = 0;
        private int betweenTransitionFestivalEndWaitTicks = 0;
        private int waitTicks = startOfDayWaitTicks;
        private int numFestivalStartVotes = 0;
        private int numFestivalStartVotesRequired = 0;
        private IDictionary<long, Farmer> otherPlayers = new Dictionary<long, Farmer>();
        private IMonitor monitor;
        private FestivalChatBox festivalChatBox;

        public BehaviorState(IMonitor monitor, EventDrivenChatBox chatBox)
        {
            this.monitor = monitor;
            festivalChatBox = new FestivalChatBox(chatBox, otherPlayers);
        }

        public bool HasBetweenEventsWaitTicks()
        {
            return betweenEventsWaitTicks > 0;
        }
        public void DecrementBetweenEventsWaitTicks()
        {
            betweenEventsWaitTicks--;
        }
        public void SkipEvent()
        {
            betweenEventsWaitTicks = (int)(600 * 0.2);
        }
        public void ClearBetweenEventsWaitTicks()
        {
            betweenEventsWaitTicks = 0;
        }

        public bool HasBetweenDialoguesWaitTicks()
        {
            return betweenDialoguesWaitTicks > 0;
        }
        public void DecrementBetweenDialoguesWaitTicks()
        {
            betweenDialoguesWaitTicks--;
        }
        public void SkipDialogue()
        {
            betweenDialoguesWaitTicks = (int)(60 * 0.2);
        }
        public void ClearBetweenDialoguesWaitTicks()
        {
            betweenDialoguesWaitTicks = 0;
        }

        public bool HasBetweenShippingMenusWaitTicks()
        {
            return betweenShippingMenusWaitTicks > 0;
        }
        public void DecrementBetweenShippingMenusWaitTicks()
        {
            betweenShippingMenusWaitTicks--;
        }
        public void SkipShippingMenu()
        {
            betweenShippingMenusWaitTicks = 60;
        }
        public void ClearBetweenShippingMenusWaitTicks()
        {
            betweenShippingMenusWaitTicks = 0;
        }

        public bool HasCheckedForParsnipSeeds()
        {
            return checkedForParsnipSeeds;
        }
        public void CheckForParsnipSeeds()
        {
            checkedForParsnipSeeds = true;
        }

        public bool ExitedFarmhouse()
        {
            return exitedFarmhouse;
        }
        public void ExitFarmhouse()
        {
            exitedFarmhouse = true;
        }

        public bool HasBetweenTransitionSleepWaitTicks()
        {
            return betweenTransitionSleepWaitTicks > 0;
        }
        public void DecrementBetweenTransitionSleepWaitTicks()
        {
            betweenTransitionSleepWaitTicks--;
        }
        public void Sleep()
        {
            betweenTransitionSleepWaitTicks = (int)(60 * 0.2);
        }
        public void WarpToSleep()
        {
            betweenTransitionSleepWaitTicks = 60;
        }
        public void CancelSleep()
        {
            betweenTransitionSleepWaitTicks = (int)(60 * 0.2);
        }
        public void ClearBetweenTransitionSleepWaitTicks()
        {
            betweenTransitionSleepWaitTicks = 0;
        }

        public bool HasBetweenTransitionFestivalAttendanceWaitTicks()
        {
            return betweenTransitionFestivalAttendanceWaitTicks > 0;
        }
        public void DecrementBetweenTransitionFestivalAttendanceWaitTicks()
        {
            betweenTransitionFestivalAttendanceWaitTicks--;
        }
        public void WaitForFestivalAttendance()
        {
            betweenTransitionFestivalAttendanceWaitTicks = (int)(60 * 0.2);
        }
        public void StopWaitingForFestivalAttendance()
        {
            betweenTransitionFestivalAttendanceWaitTicks = (int)(60 * 0.2);
        }
        public void ClearBetweenTransitionFestivalAttendanceWaitTicks()
        {
            betweenTransitionFestivalAttendanceWaitTicks = 0;
        }

        public bool HasBetweenTransitionFestivalEndWaitTicks()
        {
            return betweenTransitionFestivalEndWaitTicks > 0;
        }
        public void DecrementBetweenTransitionFestivalEndWaitTicks()
        {
            betweenTransitionFestivalEndWaitTicks--;
        }
        public void WaitForFestivalEnd()
        {
            betweenTransitionFestivalEndWaitTicks = (int)(60 * 0.2);
        }
        public void StopWaitingForFestivalEnd()
        {
            betweenTransitionFestivalEndWaitTicks = (int)(60 * 0.2);
        }
        public void ClearBetweenTransitionFestivalEndWaitTicks()
        {
            betweenTransitionFestivalEndWaitTicks = 0;
        }

        public bool HasWaitTicks()
        {
            return waitTicks > 0;
        }
        public void SetWaitTicks(int waitTicks)
        {
            this.waitTicks = waitTicks;
        }
        public void DecrementWaitTicks()
        {
            waitTicks--;
        }
        public void ClearWaitTicks()
        {
            waitTicks = 0;
        }
        
        public Tuple<int, int> UpdateFestivalStartVotes()
        {
            if (festivalChatBox.IsEnabled())
            {
                int numFestivalStartVotes = festivalChatBox.NumVoted();
                if (numFestivalStartVotes != this.numFestivalStartVotes || otherPlayers.Count != numFestivalStartVotesRequired)
                {
                    this.numFestivalStartVotes = numFestivalStartVotes;
                    numFestivalStartVotesRequired = otherPlayers.Count;
                    return Tuple.Create(numFestivalStartVotes, numFestivalStartVotesRequired);
                }
            }
            return null;
        }

        public void EnableFestivalChatBox()
        {
            festivalChatBox.Enable();
            numFestivalStartVotes = 0;
            numFestivalStartVotesRequired = otherPlayers.Count;
        }
        public void DisableFestivalChatBox()
        {
            festivalChatBox.Disable();
        }
        public void SendChatMessage(string message)
        {
            festivalChatBox.SendChatMessage(message);
        }

        public int GetNumOtherPlayers()
        {
            return otherPlayers.Count;
        }
        public IDictionary<long, Farmer> GetOtherPlayers()
        {
            return otherPlayers;
        }
        public void UpdateOtherPlayers()
        {
            if (multiplayer == null)
            {
                multiplayer = (Multiplayer)multiplayerFieldInfo.GetValue(null);
            }
            otherPlayers.Clear();
            foreach (var farmer in Game1.otherFarmers.Values)
            {
                if (!multiplayer.isDisconnecting(farmer))
                {
                    otherPlayers.Add(farmer.UniqueMultiplayerID, farmer);
                }
            }
        }

        public void LogDebug(string s)
        {
            monitor.Log(s, LogLevel.Debug);
        }

        public void NewDay()
        {
            betweenEventsWaitTicks = 0;
            betweenDialoguesWaitTicks = 0;
            betweenShippingMenusWaitTicks = 0;
            checkedForParsnipSeeds = false;
            exitedFarmhouse = false;
            betweenTransitionSleepWaitTicks = 0;
            betweenTransitionFestivalAttendanceWaitTicks = 0;
            betweenTransitionFestivalEndWaitTicks = 0;
            waitTicks = startOfDayWaitTicks;
            numFestivalStartVotes = 0;
            numFestivalStartVotesRequired = otherPlayers.Count;
        }
    }
}
