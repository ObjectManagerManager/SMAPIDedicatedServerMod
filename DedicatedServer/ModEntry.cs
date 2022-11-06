using DedicatedServer.Config;
using DedicatedServer.HostAutomatorStages;
using StardewModdingAPI;
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

        // TODO Implement chat commands to build cabins, move buildings, and destroy buildings. Normally only the host
        // can do these things

        // TODO Implement unlimited players
        
        // TODO Make host invisible

        // TODO Make host add to Luau pot or change how the score is computed, if possible

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            var config = helper.ReadConfig<ModConfig>();
            new StartFarmStage(helper, Monitor, config).Enable();
            helper.Events.GameLoop.UpdateTicked += PrintDebug;
        }

        private void PrintDebug(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
        }
    }
}