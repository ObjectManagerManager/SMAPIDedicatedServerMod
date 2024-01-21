using DedicatedServer.Chat;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static StardewValley.Polygon;
using static System.Collections.Specialized.BitVector32;

namespace DedicatedServer.Utils
{
    /// <summary>
    /// Offers multiplayer options and multiplayer support functions
    /// </summary>
    internal class MultiplayerOptions
    {
        /// <summary>
        ///         The server mode option that you see in the game settings,
        /// <br/>   Options -> Multiplayer -> Server Mode
        /// <br/>  
        /// <br/>   Read the current state with <see cref="Game1.options.serverPrivacy"/>
        /// <br/>   Setzen Sie ihn mit der Methode<see cref="Game1.options.setServerMode()"/>
        /// </summary>
        public enum ServerModeTypes
        {
            InviteOnly  = ServerPrivacy.InviteOnly,
            FriendsOnly = ServerPrivacy.FriendsOnly,
            Offline     = ServerPrivacy.Public,
        }

        /// <summary>
        /// Effects the variable <see cref="Game1.multiplayerMode"/>
        /// </summary>
        private enum GameOptionMenue
        {
            /// <summary>
            /// Set game to singleplayer, without multiplayer options
            /// </summary>
            Singleplayer = 0,

            // / <summary>
            // / Set game to multiplayer, as client.
            // / </summary>
            //multiplayerClient = 1,

            /// <summary>
            /// Set game to multiplayer, as host with multiplayer options
            /// </summary>
            MultiplayerServer = 2
        }
        
        private static IModHelper helper;

        private static EventDrivenChatBox chatBox;

        public const string inviteCodeSaveFile = "invite_code.txt";

        /// <summary>
        /// Cclass requires helper of type <see cref="IModHelper"/>
        /// </summary>
        /// <param name="helper">The helper must be initialized.</param>
        public MultiplayerOptions(IModHelper helper, EventDrivenChatBox chatBox)
        {
            MultiplayerOptions.helper = helper;
            MultiplayerOptions.chatBox = chatBox;

            TryActivatingInviteCode();
        }

        /// <summary>
        /// Checks whether the invitation code is available
        /// </summary>
        /// <returns></returns>
        public static bool IsInviteCodeAvailable
        {
            get { return Game1.server?.canOfferInvite() ?? false; }
        }

        /// <summary>
        /// Reads the invitation code
        /// </summary>
        public static string InviteCode
        {
            get
            {
                try { return Game1.server?.getInviteCode() ?? ""; }
                catch { return ""; }
            }
        }

        /// <summary>
        /// Saves your invitaion code in a file (<see cref="inviteCodeSaveFile"/>).
        /// </summary>
        public static void SaveInviteCode()
        {
            string inviteCode = InviteCode;
            try
            {
                helper?.Data.WriteJsonFile(inviteCodeSaveFile, inviteCode);
            }
            catch { }
        }

        /// <summary>
        ///         Sets and gets the option whether players can connect directly via IP address.
        /// <br/>   Check <see cref="EnableServer"/> beforehand, if the server is offline you cannot join the server
        /// </summary>
        public static bool IpConnectionsEnabled
        {
            get { return Game1.options.ipConnectionsEnabled; }
            set { Game1.options.ipConnectionsEnabled = value; }
        }

        /// <summary>
        ///         Activates and deactivates the server by switching
        /// <br/>   between a multiplayer and a single-player game
        /// <br/>   
        /// <br/>   If the game did not create an invitation code at startup,
        /// <br/>   toggling this property will create an invitation code.
        /// </summary>
        public static bool EnableServer
        {
            get {
                // You cannot use `Game1.IsServer`, it only checks
                // if `Game1.multiplayerMode` is 2 and not if the
                // server is running.
                return Game1.options.enableServer;
            }
            set
            {
                if (value)
                {
                    if (false == EnableServer)
                    {
                        // Enables the server
                        ServerMode = ServerModeTypes.FriendsOnly;

                        // Changes the host options menu and `Game1.IsServer` checks this option whether the server is online.
                        Game1.multiplayerMode = (int)GameOptionMenue.MultiplayerServer;
                    }
                }
                else
                {
                    if (true == EnableServer)
                    {
                        KickAll();

                        // Disable the server
                        ServerMode = ServerModeTypes.Offline;

                        // Changes the host options menu and `Game1.IsServer` checks this option whether the server is online.
                        Game1.multiplayerMode = (int)GameOptionMenue.Singleplayer;
                    }
                }
            }
        }

        /// <summary>
        /// Changes the Server Mode option
        /// </summary>
        public static ServerModeTypes ServerMode {
            get
            {
                return (ServerModeTypes)Game1.options.serverPrivacy;
            }
            set
            {
                switch (value)
                {
                    case ServerModeTypes.Offline:
                        Game1.options.setServerMode("offline");
                        // Sets in-game option to offline
                        Game1.options.serverPrivacy = ServerPrivacy.Public;
                        break;
                    case ServerModeTypes.FriendsOnly:
                        Game1.options.setServerMode("friends");
                        break;
                    case ServerModeTypes.InviteOnly:
                        Game1.options.setServerMode("invite");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Kicks all online players
        /// </summary>
        public static void KickAll()
        {
            foreach (var farmer in Game1.otherFarmers.Values.ToList())
            {
                Game1.server.kick(farmer.UniqueMultiplayerID);
            }
        }

        #region TRY_ACTIVATING_INVITE_CODE

        private enum TryActivatingStates
        {
            None,
            WaitForInviteCode,
            DisableServer,
            WaitUntilActivation,
            EnableServer,
            WaitForNewInviteCode,
        }

        private static int time;

        private static TryActivatingStates tryActivatingState;

        private static int[] tryActivatingWaitTimes = { 9, 3, 9 };

        /// <summary>
        ///         Attempts to obtain the invitation code.
        /// <br/>   If successful, it is saved in a file.
        /// <br/>   If this is not successful in the first time period,
        /// <br/>   the multiplayer server is closed, a second time is
        /// <br/>   waited for and the multiplayer server is restarted.
        /// <br/>   In the third section, another attempt is made to
        /// <br/>   obtain the invite code.
        /// </summary>
        /// <returns>
        ///         true : if the handler has been started
        /// <br/>   false: the handler is already running and could not be started.</returns>
        public static bool TryActivatingInviteCode()
        {
            if (0 < MultiplayerOptions.time) { return false; }

            MultiplayerOptions.time = tryActivatingWaitTimes[0];
            tryActivatingState = TryActivatingStates.WaitForInviteCode;

            AddOnOneSecondUpdateTicked(TryActivatingInviteCodeWorker);

            return true;
        }

        private static void TryActivatingInviteCodeWorker(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (0 < time)
            {
                time--;
            }

            switch (tryActivatingState)
            {
                case TryActivatingStates.None:
                    RemoveOnOneSecondUpdateTicked(TryActivatingInviteCodeWorker);
                    return;

                case TryActivatingStates.WaitForInviteCode:
                    if("" != InviteCode)
                    {
                        SaveInviteCode();
                        tryActivatingState = TryActivatingStates.None;
                        chatBox.textBoxEnter($"Could receive the invitation code {InviteCode}" + TextColor.Green);
                        return;
                    }
                    if(0 == time)
                    {
                        tryActivatingState = TryActivatingStates.DisableServer;
                    }
                    chatBox.textBoxEnter($"Attention: Server will shut down in {time} seconds" + TextColor.Yellow);
                    break;

                case TryActivatingStates.DisableServer:
                    time = tryActivatingWaitTimes[1];
                    EnableServer = false;
                    tryActivatingState = TryActivatingStates.WaitUntilActivation;
                    break;

                case TryActivatingStates.WaitUntilActivation:
                    if (0 == time)
                    {
                        tryActivatingState = TryActivatingStates.EnableServer;
                    }
                    chatBox.textBoxEnter($"Attention: The server is started in {time} seconds" + TextColor.Yellow);
                    break;

                case TryActivatingStates.EnableServer:
                    time = tryActivatingWaitTimes[2];
                    tryActivatingState = TryActivatingStates.WaitForNewInviteCode;
                    EnableServer = true;
                    break;

                case TryActivatingStates.WaitForNewInviteCode:
                    if ("" != InviteCode)
                    {
                        SaveInviteCode();
                        tryActivatingState = TryActivatingStates.None;
                        // chatBox.textBoxEnter($"Could receive the invitation code {InviteCode}" + TextColor.Green);
                        return;
                    }
                    if (0 == time)
                    {
                        tryActivatingState = TryActivatingStates.None;
                        // chatBox.textBoxEnter($"Attention: Invitation code could not be retrieved" + TextColor.Red);
                        return;
                    }
                    // chatBox.textBoxEnter($"Attention: Try to get the invitation code, remaining time {time} seconds" + TextColor.Yellow);
                    break;
            }
        }

        private static void AddOnOneSecondUpdateTicked(EventHandler<OneSecondUpdateTickedEventArgs> handler)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked += handler;
        }

        private static void RemoveOnOneSecondUpdateTicked(EventHandler<OneSecondUpdateTickedEventArgs> handler)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked -= handler;
        }

        #endregion
    }
}
