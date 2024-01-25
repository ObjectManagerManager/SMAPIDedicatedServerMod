using DedicatedServer.Chat;
using DedicatedServer.Config;
using DedicatedServer.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DedicatedServer.MessageCommands
{
    internal class ServerCommandListener
    {
        private EventDrivenChatBox chatBox;

        private ModConfig config;

        private IModHelper helper;

        public ServerCommandListener(IModHelper helper, ModConfig config, EventDrivenChatBox chatBox)
        {
            this.helper  = helper;
            this.config  = config;
            this.chatBox = chatBox;
        }

        public void Enable()
        {
            chatBox.ChatReceived += chatReceived;
        }

        public void Disable()
        {
            chatBox.ChatReceived -= chatReceived;
        }

        private void chatReceived(object sender, ChatEventArgs e)
        {
            var tokens = e.Message.Split(' ');

            if (0 == tokens.Length) { return; }

            string command = tokens[0].ToLower();

            if(Game1.player.UniqueMultiplayerID == e.SourceFarmerId)
            {
                switch (command)
                {
                    case "letmeplay":
                        HostAutomation.LetMePlay();
                        break;

                    case "takeover":
                        HostAutomation.TakeOver();
                        break;

                    #region DEBUG_COMMANDS
                    #if false

                    case "letmecontrol":
                        HostAutomation.LetMeControl();
                        break;

                    case "multiplayer":
                        MultiplayerOptions.EnableServer = true;
                        break;

                    case "singleplayer":
                        MultiplayerOptions.EnableServer = false;
                        break;

                    case "mine":
                        var mine = Game1.getLocationFromName("Mine") as Mine;
                        var warp = new Warp(18, 13, mine.NameOrUniqueName, 18, 13, false);
                        Game1.player.warpFarmer(warp);
                        break;

                    case "location":
                        var location = Game1.player.getTileLocation();
                        chatBox.textBoxEnter("x: " + location.X + ", y:" + location.Y);
                        break;

                    #endif
                    #endregion
                }
            }
            else
            {
                if (ChatBox.privateMessage != e.ChatKind)
                {
                    return;
                }
            }      

            string param = 1 < tokens.Length ? tokens[1].ToLower() : "";

            var sourceFarmer = Game1.otherFarmers.Values
                .Where(farmer => farmer.UniqueMultiplayerID == e.SourceFarmerId)
                .FirstOrDefault()?
                .Name ?? Game1.player.Name;

            switch (command)
            {
                case "safeinvitecode": // /message ServerBot SafeInviteCode
                    MultiplayerOptions.SaveInviteCode();
                    if (MultiplayerOptions.IsInviteCodeAvailable)
                    {
                        chatBox.textBoxEnter($"Your invite code is saved in the mod folder in the file {MultiplayerOptions.inviteCodeSaveFile}." + TextColor.Green);
                    }
                    else
                    {
                        chatBox.textBoxEnter($"The game has no invite code." + TextColor.Pink);
                    }
                    break;

                case "invitecode": // /message ServerBot InviteCode
                    chatBox.textBoxEnter($"Invite code: {MultiplayerOptions.InviteCode}");
                    break;

                case "sleep": // /message ServerBot Sleep
                    if (Sleeping.ShouldSleepOverwrite)
                    {
                        Sleeping.ShouldSleepOverwrite = false;
                        chatBox.textBoxEnter($"The host is back on his feet.");
                    }
                    else
                    {
                        chatBox.textBoxEnter($"The host will go to sleep.");
                        Sleeping.ShouldSleepOverwrite = true;
                    }
                    
                    break;

                case "resetday": // /message ServerBot ResetDay
                    RestartDay.ResetDay((seconds) => chatBox.textBoxEnter($"Attention: Server will reset the day in {seconds} seconds"));
                    break;

                case "shutdown": // /message ServerBot Shutdown
                    RestartDay.ShutDown((seconds) => chatBox.textBoxEnter($"Attention: Server will shut down in {seconds} seconds"));
                    break;

                case "spawnmonster": // /message ServerBot SpawnMonster
                    if (MultiplayerOptions.SpawnMonstersAtNight)
                    {
                        chatBox.textBoxEnter($"No more monsters will appear." + TextColor.Green);
                        MultiplayerOptions.SpawnMonstersAtNight = false;
                    }
                    else
                    {
                        chatBox.textBoxEnter($"Monsters will appear." + TextColor.Red);
                        MultiplayerOptions.SpawnMonstersAtNight = true;
                    }
                    break;

                case "mbp": // /message ServerBot mbp on
                case "movebuildpermission":
                case "movepermissiong":
                    // As the host you can run commands in the chat box, using a forward slash(/) before the command.
                    // See: <seealso href="https://stardewcommunitywiki.com/Multiplayer"/>

                    var moveBuildPermissionParameter = new List<string>() { "off", "owned", "on" };

                    if (moveBuildPermissionParameter.Any(param.Equals))
                    {
                        if (config.MoveBuildPermission == param)
                        {
                            chatBox.textBoxEnter("/message " + sourceFarmer + " Error: The parameter is already " + config.MoveBuildPermission);
                        }
                        else
                        {
                            config.MoveBuildPermission = param;
                            chatBox.textBoxEnter(sourceFarmer + " Changed MoveBuildPermission to " + config.MoveBuildPermission);
                            chatBox.textBoxEnter("/mbp " + config.MoveBuildPermission);
                            helper.WriteConfig(config);
                        }
                    }
                    else
                    {
                        chatBox.textBoxEnter("/message " + sourceFarmer + " Error: Only the following parameter are valid: " + String.Join(", ", moveBuildPermissionParameter.ToArray()));
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
