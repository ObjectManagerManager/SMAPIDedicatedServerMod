using DedicatedServer.Config;
using DedicatedServer.HostAutomatorStages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace DedicatedServer
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        // TODO ModConfig value checking. But perhaps this actually should be done in the SelectFarmStage; if the
        // farm with the name given by the config exists, then none of the rest of the config values really matter,
        // except for the bat / mushroom decision and the pet name (the parts accessed mid-game rather than just at
        // farm creation).

        // TODO Add more config options, like the ability to disable the crop saver (perhaps still keep track of crops
        // in case it's enabled later, but don't alter them).

        // TODO Remove player limit (if the existing attempts haven't already succeeded in doing that).

        // TODO Make the host invisible to everyone else

        // TODO Consider what the automated host should do when another player proposes to them.

        private WaitCondition titleMenuWaitCondition;
        private ModConfig config;
        private IModHelper helper;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            this.config = helper.ReadConfig<ModConfig>();

            // Ensure that the game environment is in a stable state before the mod starts executing
            // Without a waiting time, an invitation code is almost never generated; with a waiting
            // time of 1 second, it is very rare that no more codes are generated
            this.titleMenuWaitCondition = new WaitCondition(
                () => Game1.activeClickableMenu is StardewValley.Menus.TitleMenu,
                60);

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        /// <summary>
        /// Event handler to wait until a specific condition is met before executing.
        /// </summary>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this.titleMenuWaitCondition.IsMet())
            {
                helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
                new StartFarmStage(this.Helper, Monitor, config).Enable();
            }
        }

        /// <summary>
        ///         Represents wait condition.
        /// <br/>   
        /// <br/>   First waits until the condition is met and then waits a certain number of update cycles
        /// </summary>
        private class WaitCondition
        {
            private readonly System.Func<bool> condition;
            private int waitCounter;

            public WaitCondition(System.Func<bool> condition, int initialWait)
            {
                this.condition = condition;
                this.waitCounter = initialWait;
            }

            public bool IsMet()
            {
                if (this.condition())
                {
                    this.waitCounter--;
                }

                if (0 >= this.waitCounter)
                {
                    return true;
                }

                return false;

            }
        }
    }
}
