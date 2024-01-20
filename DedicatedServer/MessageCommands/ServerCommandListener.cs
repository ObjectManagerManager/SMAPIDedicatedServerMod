using DedicatedServer.Chat;
using DedicatedServer.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
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

            if (tokens.Length == 0) { return; }

            tokens[0] = tokens[0].ToLower();

            // As the host you can run commands in the chat box, using a forward slash(/) before the command.
            // See: <seealso href="https://stardewcommunitywiki.com/Multiplayer"/>
            var moveBuildPermissionCommand = new List<string>() { "mbp", "movebuildpermission", "movepermissiong" };
           
            if( (ChatBox.privateMessage == e.ChatKind               ) &&
                (moveBuildPermissionCommand.Any(tokens[0].Equals) ) )
            {
                string newBuildPermission;

                if (2 == tokens.Length)
                {
                    newBuildPermission = tokens[1].ToLower();
                }
                else
                {
                    newBuildPermission = "";
                }

                var sourceFarmer = Game1.otherFarmers.Values
                    .Where( farmer => farmer.UniqueMultiplayerID == e.SourceFarmerId)
                    .FirstOrDefault()?
                    .Name ?? Game1.player.Name;

                var moveBuildPermissionParameter = new List<string>() { "off", "owned", "on" };

                if (moveBuildPermissionParameter.Any(newBuildPermission.Equals))
                {
                    if (config.MoveBuildPermission == newBuildPermission)
                    {
                        chatBox.textBoxEnter("/message " + sourceFarmer + " Error: The parameter is already " + config.MoveBuildPermission);
                    }
                    else
                    {
                        config.MoveBuildPermission = newBuildPermission;
                        chatBox.textBoxEnter(sourceFarmer + " Changed MoveBuildPermission to " + config.MoveBuildPermission);
                        chatBox.textBoxEnter("/mbp " + config.MoveBuildPermission);
                        helper.WriteConfig(config);
                    }
                }
                else
                {
                    chatBox.textBoxEnter("/message " + sourceFarmer + " Error: Only the following parameter are valid: " + String.Join(", ", moveBuildPermissionParameter.ToArray()));
                }
            }
        }
    }
}
