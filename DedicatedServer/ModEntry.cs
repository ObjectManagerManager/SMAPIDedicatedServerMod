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

        // TODO Implement chat commands to move buildings, or overwrite the existing built-in building-moving permission.
        // The former would introduce a carpenter-independent way of moving buildings, which could then be controlled
        // strictly with server-side per-player permission configuration. The latter would allow non-hosts to move buildings
        // via the carpenter UI. Ideally, a "complete" solution would enable the UI moving mechanic when no server-side
        // permissions are configured (or when the "move building" action is permissible to everyone), and disable it
        // otherwise, forcing players to use the strictly-controlled chat commands. But permission files don't need to
        // be supported for v1.0.0, so maybe we'll just do the built-in UI permission override for now.

        // TODO Remove player limit (if the existing attempts haven't already succeeded in doing that).
        
        // TODO Make the host invisible to everyone else
        
        // TODO Consider what the automated host should do when another player proposes to them.

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