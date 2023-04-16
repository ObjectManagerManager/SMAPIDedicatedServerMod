using DedicatedServer.Chat;
using DedicatedServer.Config;
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
    internal class BehaviorChain
    {
        private BehaviorLink head;

        public BehaviorChain(IModHelper helper, IMonitor monitor, ModConfig config, EventDrivenChatBox chatBox)
        {
            // 1. Perform prerequisite per-tick state updates, such as detecting the number of other players online
            //      (this is a non-blocking chain link; the process will always follow through to the next link).
            // 2. Transition the game pause state
            // 3. Process wait ticks
            // 4. Skip skippable events
            // 5. Respond to dialogue box question if present, skipping non-question dialogue
            // 6. Skip shipping menu
            // 7. If in farmhouse and haven't checked for parsnip seeds, check for parsnip seeds
            // 8. If in farmhouse and haven't left farmhouse for the day, leave farmhouse
            // 9. If we don't have the fishing rod yet, and it's available, get it.
            // 10. If we haven't unlocked the community center yet, and we can, then unlock it.
            // 12. If we haven't watched the end cutscene for the community scenter yet, and we can, then watch it.
            // 13. If our sleep state should be switched, then switch it
            // 14. If our state of festival attendance should be switched, then switch it
            // 15. If our leave festival state should be switched, then switch it
            // 16. If we're at a festival and we need to watch the festival chatbox, then watch it

            var chain = new BehaviorLink[] {
                new UpdateStateBehaviorLink(),
                new ProcessPauseBehaviorLink(),
                new ProcessWaitTicksBehaviorLink(),
                new SkipEventsBehaviorLink(),
                new ProcessDialogueBehaviorLink(config),
                new SkipShippingMenuBehaviorLink(),
                new CheckForParsnipSeedsBehaviorLink(),
                new ExitFarmHouseBehaviorLink(),
                new GetFishingRodBehaviorLink(),
                new UnlockCommunityCenterBehaviorLink(),
                new PurchaseJojaMembershipBehaviorLink(config),
                new EndCommunityCenterBehaviorLink(),
                new TransitionSleepBehaviorLink(),
                new TransitionFestivalAttendanceBehaviorLink(),
                new TransitionFestivalEndBehaviorLink(),
                new ProcessFestivalChatBoxBehaviorLink()
            };
            // Build chain and set head
            for (int i = 0; i < chain.Length - 1; i++)
            {
                chain[i].SetNext(chain[i + 1]);
            }
            head = chain[0];
        }

        public void Process(BehaviorState state)
        {
            head.Process(state);
        }
    }
}
