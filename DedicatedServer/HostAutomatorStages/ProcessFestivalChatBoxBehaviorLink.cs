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
    internal class ProcessFestivalChatBoxBehaviorLink : BehaviorLink
    {
        public ProcessFestivalChatBoxBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        public override void Process(BehaviorState state)
        {
            Tuple<int, int> voteCounts = state.UpdateFestivalStartVotes();
            if (voteCounts != null)
            {
                if (voteCounts.Item1 == voteCounts.Item2)
                {
                    // Start the festival
                    state.SendChatMessage($"{voteCounts.Item1} / {voteCounts.Item2} votes casted. Starting the festival event...");
                    Game1.CurrentEvent.answerDialogueQuestion(null, "yes");
                    state.DisableFestivalChatBox();
                }
                else
                {
                    state.SendChatMessage($"{voteCounts.Item1} / {voteCounts.Item2} votes casted.");
                }
            } else
            {
                processNext(state);
            }
        }
    }
}
