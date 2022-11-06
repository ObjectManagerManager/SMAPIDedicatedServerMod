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
                    if (Game1.currentSeason == "summer" && Game1.dayOfMonth == 11 && Game1.player.team.luauIngredients.Count > 0)
                    {
                        // If it's the Luau and the pot isn't empty, add a duplicate of someone else's item to the pot. It (mostly) doesn't matter
                        // which item is duplicated. Indeed, the total luau score is simply equal to the lowest score (or some extremum) of any item
                        // added, with two exceptions: 1) if anyone adds the mayor's shorts, the score is set to a magic number (6)
                        // and all other items added are ignored, and 2) if anyone doesn't add an item, the score is set to a magic number (5).
                        // This means that having X players put in X items is no different from having X+1 players put in X+1 items, where the
                        // additional item is a duplicate of one of the original X items. This is the intention. The only possible concern is that it
                        // looks like putting in better items will improve relationships more. But it's probably not all that noticeable of a difference
                        // anyways. So just duplicate the first element with Item.getOne().
                        Game1.player.team.luauIngredients.Add(Game1.player.team.luauIngredients[0].getOne());
                    }
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
