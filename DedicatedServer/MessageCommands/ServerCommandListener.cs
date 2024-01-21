using DedicatedServer.Chat;
using DedicatedServer.Config;
using DedicatedServer.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
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

        private void AddOnSaved(EventHandler<SavedEventArgs> handler)
        {
            helper.Events.GameLoop.Saved += handler;
        }

        private void RemoveOnSaved(EventHandler<SavedEventArgs> handler)
        {
            helper.Events.GameLoop.Saved -= handler;
        }

        private void AddOnOneSecondUpdateTicked(EventHandler<OneSecondUpdateTickedEventArgs> handler)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked += handler;
        }

        private void RemoveOnOneSecondUpdateTicked(EventHandler<OneSecondUpdateTickedEventArgs> handler)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked -= handler;
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
                    case "haon":
                        HostAutomation.EnableHostAutomation = true;
                        break;
                    case "haoff":
                        HostAutomation.EnableHostAutomation = false;
                        break;
                    case "ppon":
                        HostAutomation.PreventPause = true;
                        break;
                    case "ppoff":
                        HostAutomation.PreventPause = false;
                        break;

                    #region DEBUG_COMMANDS

                    #endregion
                }
            }            

            if (ChatBox.privateMessage != e.ChatKind ) { return; }

            string param = 1 < tokens.Length ? tokens[1].ToLower() : "";

            var sourceFarmer = Game1.otherFarmers.Values
                .Where(farmer => farmer.UniqueMultiplayerID == e.SourceFarmerId)
                .FirstOrDefault()?
                .Name ?? Game1.player.Name;

            switch (command)
            {
                case "multiplayer": // /message ServerBot MultiPlayer
                    MultiplayerOptions.EnableServer = true;
                    break;

                case "singleplayer": // /message ServerBot SinglePlayer
                    MultiplayerOptions.EnableServer = false;
                    break;

                case "safeinvitecode": // /message ServerBot SafeInviteCode
                    MultiplayerOptions.SaveInviteCode();
                    if (MultiplayerOptions.IsInviteCodeAvailable)
                    {
                        chatBox.textBoxEnter($"Your invite code is saved in the mod folder in the file {MultiplayerOptions.inviteCodeSaveFile}.");
                    }
                    else
                    {
                        chatBox.textBoxEnter($"The game has no invite code.");
                    }
                    break;

                case "invitecode": // /message ServerBot InviteCode
                    chatBox.textBoxEnter($"Invite code: {MultiplayerOptions.InviteCode}");
                    break;

                case "resetday": // /message ServerBot ResetDay
                    SavesGameRestartsDay(
                        time: 10,
                        keepsCurrentDay: true,
                        quit: false, 
                        action: (seconds) => chatBox.textBoxEnter($"Attention: Server will reset the day in {seconds} seconds"));
                    break;

                case "shutdown": // /message ServerBot Shutdown
                    SavesGameRestartsDay(
                        time: 10,
                        keepsCurrentDay: false,
                        quit: true,
                        action: (seconds) => chatBox.textBoxEnter($"Attention: Server will shut down in {seconds} seconds"));
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

        #region RESET_DAY

        private int time;
        private bool keepsCurrentDay;
        private bool quit;
        private Action<int> action;

        /// <summary>
        ///         Saves the game and resets the day
        /// <br/>
        /// <br/>   1. Prevents the pausing of the dedicated server.
        /// <br/>   2. Conditional: Retains the current day.
        /// <br/>   3. Kicks out all online players.
        /// <br/>   4. Saves the game.
        /// <br/>   5. Resets the normal pause behavior of the dedicated server.
        /// <br/>   6. Conditional: Quit the game.
        /// </summary>
        /// <param name="time">Wait time in seconds</param>
        /// <param name="keepsCurrentDay">
        /// <br/>   true : The day counter retains the current day.
        /// <br/>   false: The day counter is incremented. </param>
        /// <param name="quit">
        /// <br/>   true : After saving the game the game is quit
        /// <br/>   false: The game will be continued </param>
        /// <param name="action">
        ///         Function that is executed every second until the
        /// <br/>   event is executed. The parameter is the remaining time. </param>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keepsCurrentDay"></param>
        /// <param name="quit"></param>
        private void SavesGameRestartsDay(int time = 0, bool keepsCurrentDay = true, bool quit = false, Action<int> action = null)
        {
            HostAutomation.PreventPause = true;

            if(0 < this.time)
            {
                this.time = time;
                return;
            }

            this.time = time;
            this.keepsCurrentDay = keepsCurrentDay;
            this.quit = quit;
            this.action = action;

            AddOnOneSecondUpdateTicked(SavesGameRestartsDayWorker);

        }

        private void SavesGameRestartsDayWorker(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if(0 < time)
            {
                time--;
                action?.Invoke(time);
                return;
            }

            RemoveOnOneSecondUpdateTicked(SavesGameRestartsDayWorker);

            if (keepsCurrentDay)
            {
                if (Game1.dayOfMonth > 1)
                {
                    Game1.stats.DaysPlayed--;
                    Game1.dayOfMonth--;
                }
            }

            foreach (var farmer in Game1.otherFarmers.Values)
            {
                Game1.server.kick(farmer.UniqueMultiplayerID);
            }

            Game1.player.isInBed.Value = true;
            Game1.currentLocation.answerDialogueAction("Sleep_Yes", null);

            AddOnSaved(OnSavedActivateHostAutomation);

            if (quit)
            {
                AddOnSaved(OnSavedQuit);
            }

        }

        private void OnSavedQuit(object sender, SavedEventArgs e)
        {
            RemoveOnSaved(OnSavedQuit);

            Game1.quit = true;
        }

        public void OnSavedActivateHostAutomation(object sender, SavedEventArgs e)
        {
            RemoveOnSaved(OnSavedActivateHostAutomation);

            HostAutomation.PreventPause = false;
        }

        #endregion
    }
}
