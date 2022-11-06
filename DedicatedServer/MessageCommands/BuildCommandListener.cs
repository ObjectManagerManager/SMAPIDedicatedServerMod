using DedicatedServer.Chat;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.MessageCommands
{
    internal class BuildCommandListener
    {
        private static Dictionary<string, Action<EventDrivenChatBox, Farmer>> buildingActions = new Dictionary<string, Action<EventDrivenChatBox, Farmer>>
        {
            {"stone_cabin", genBuildCabin("Stone Cabin")},
            {"plank_cabin", genBuildCabin("Plank Cabin")},
            {"log_cabin", genBuildCabin("Log Cabin")},
        };
        private static readonly string validBuildingNamesList = genValidBuildingNamesList();

        private EventDrivenChatBox chatBox;

        public BuildCommandListener(EventDrivenChatBox chatBox)
        {
            this.chatBox = chatBox;
        }

        private static Action<EventDrivenChatBox, Farmer> genBuildCabin(string cabinBlueprintName)
        {
            void buildCabin(EventDrivenChatBox chatBox, Farmer farmer)
            {
                var point = farmer.getTileLocation();
                var blueprint = new BluePrint(cabinBlueprintName);
                point.X -= blueprint.humanDoor.X; // Shift the point so that the door is at the player's horizontal location
                point.Y -= blueprint.tilesHeight; // Shift the point so that the cabin's directly above the player
                Game1.player.team.buildLock.RequestLock(delegate
                {
                    if (Game1.locationRequest == null)
                    {
                        var res = ((Farm)Game1.getLocationFromName("Farm")).buildStructure(blueprint, new Vector2(point.X, point.Y), Game1.player, false);
                        if (res)
                        {
                            chatBox.textBoxEnter(farmer.Name + " just built a " + cabinBlueprintName);
                        }
                        else
                        {
                            chatBox.textBoxEnter("/message " + farmer.Name + " Error: " + Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"));
                        }
                    }
                    Game1.player.team.buildLock.ReleaseLock();
                });
            }
            return buildCabin;
        }

        private static string genValidBuildingNamesList()
        {
            string str = "";
            var buildingActionsEnumerable = buildingActions.Keys.ToArray();
            for (int i = 0; i < buildingActionsEnumerable.Length; i++)
            {
                str += "\"" + buildingActionsEnumerable[i] + "\"";
                if (i + 1 < buildingActionsEnumerable.Length)
                {
                    str += ", ";
                }
                if (i + 1 == buildingActionsEnumerable.Length - 1)
                {
                    str += "and ";
                }
            }
            return str;
        }

        private void pmValidBuildingNames(Farmer farmer)
        {
            var str = "/message " + farmer.Name + " Valid building names include " + validBuildingNamesList;
            chatBox.textBoxEnter(str);
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
            // Private message chatKind is 3
            var tokens = e.Message.ToLower().Split(' ');
            if (tokens.Length == 0)
            {
                return;
            }
            if (e.ChatKind == 3 && tokens[0] == "build")
            {
                // Find the farmer it came from and determine their location
                foreach (var farmer in Game1.otherFarmers.Values)
                {
                    if (farmer.UniqueMultiplayerID == e.SourceFarmerId)
                    {
                        if (tokens.Length != 2)
                        {
                            chatBox.textBoxEnter("/message " + farmer.Name + " Error: Invalid command usage.");
                            chatBox.textBoxEnter("/message " + farmer.Name + " Usage: build [building_name]");
                            pmValidBuildingNames(farmer);
                            return;
                        }
                        var buildingName = tokens[1];
                        if (buildingActions.TryGetValue(buildingName, out var action))
                        {
                            var location = farmer.currentLocation;
                            if (location is Farm f)
                            {
                                action(chatBox, farmer);
                            }
                            else
                            {
                                chatBox.textBoxEnter("/message " + farmer.Name + " Error: You cannot place buildings outside of the farm!");
                            }
                        }
                        else
                        {
                            chatBox.textBoxEnter("/message " + farmer.Name + " Error: Unrecognized building name \"" + buildingName + "\"");
                            pmValidBuildingNames(farmer);
                        }
                        break;
                    }
                }
            }
        }
    }
}
