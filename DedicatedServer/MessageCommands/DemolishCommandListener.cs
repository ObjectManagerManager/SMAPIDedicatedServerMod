using DedicatedServer.Chat;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace DedicatedServer.MessageCommands
{
    internal class DemolishCommandListener
    {
        private EventDrivenChatBox chatBox;

        public DemolishCommandListener(EventDrivenChatBox chatBox)
        {
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

        private void destroyCabin(string farmerName, Building building, Farm f)
        {
            Action buildingLockFailed = delegate
            {
                chatBox.textBoxEnter("/message " + farmerName + " Error: " + Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"));
            };
            Action continueDemolish = delegate
            {
                if (building.daysOfConstructionLeft.Value > 0 || building.daysUntilUpgrade.Value > 0)
                {
                    chatBox.textBoxEnter("/message " + farmerName + " Error: " + Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction"));
                }
                else if (building.indoors.Value != null && building.indoors.Value is AnimalHouse && (building.indoors.Value as AnimalHouse).animalsThatLiveHere.Count > 0)
                {
                    chatBox.textBoxEnter("/message " + farmerName + " Error: " + Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere"));
                }
                else if (building.indoors.Value != null && building.indoors.Value.farmers.Any())
                {
                    chatBox.textBoxEnter("/message " + farmerName + " Error: " + Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"));
                }
                else
                {
                    if (building.indoors.Value != null && building.indoors.Value is Cabin)
                    {
                        foreach (Farmer allFarmer in Game1.getAllFarmers())
                        {
                            if (allFarmer.currentLocation != null && allFarmer.currentLocation.Name == (building.indoors.Value as Cabin).GetCellarName())
                            {
                                chatBox.textBoxEnter("/message " + farmerName + " Error: " + Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"));
                                return;
                            }
                        }
                    }

                    if (building.indoors.Value is Cabin && (building.indoors.Value as Cabin).farmhand.Value.isActive())
                    {
                        chatBox.textBoxEnter("/message " + farmerName + " Error: " + Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_FarmhandOnline"));
                    }
                    else
                    {
                        building.BeforeDemolish();
                        Chest chest = null;
                        if (building.indoors.Value is Cabin)
                        {
                            List<Item> list = (building.indoors.Value as Cabin).demolish();
                            if (list.Count > 0)
                            {
                                chest = new Chest(playerChest: true);
                                chest.fixLidFrame();
                                chest.items.Set(list);
                            }
                        }

                        if (f.destroyStructure(building))
                        {
                            _ = building.tileY.Value;
                            _ = building.tilesHigh.Value;
                            Game1.flashAlpha = 1f;
                            building.showDestroyedAnimation(Game1.getFarm());
                            Utility.spreadAnimalsAround(building, f);
                            if (chest != null)
                            {
                                f.objects[new Vector2(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2)] = chest;
                            }
                        }
                    }
                }
            };

            Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
        }

        private Action genDestroyCabinAction(string farmerName, Building building)
        {
            void destroyCabinAction()
            {
                Farm f = Game1.getFarm();
                destroyCabin(farmerName, building, f);
            }

            return destroyCabinAction;
        }

        private Action genCancelDestroyCabinAction(string farmerName)
        {
            void cancelDestroyCabinAction()
            {
                chatBox.textBoxEnter("/message " + farmerName + " Action canceled.");
            }

            return cancelDestroyCabinAction;
        }

        private void chatReceived(object sender, ChatEventArgs e)
        {
            var tokens = e.Message.ToLower().Split(' ');
            if (tokens.Length == 0)
            {
                return;
            }
            // Private message chatKind is 3
            if (e.ChatKind == 3 && tokens[0] == "demolish")
            {
                // Find the farmer it came from and determine their location
                foreach (var farmer in Game1.otherFarmers.Values)
                {
                    if (farmer.UniqueMultiplayerID == e.SourceFarmerId)
                    {
                        if (tokens.Length != 1)
                        {
                            chatBox.textBoxEnter("/message " + farmer.Name + " Error: Invalid command usage.");
                            chatBox.textBoxEnter("/message " + farmer.Name + " Usage: demolish");
                            return;
                        }
                        var location = farmer.currentLocation;
                        if (location is Farm f)
                        {
                            var tileLocation = farmer.getTileLocation();
                            switch (farmer.facingDirection.Value)
                            {
                                case 1: // Right
                                    tileLocation.X++;
                                    break;
                                case 2: // Down
                                    tileLocation.Y++;
                                    break;
                                case 3: // Left
                                    tileLocation.X--;
                                    break;
                                default: // 0 = up
                                    tileLocation.Y--;
                                    break;
                            }
                            foreach (var building in f.buildings)
                            {
                                if (building.occupiesTile(tileLocation))
                                {
                                    // Determine if the building can be demolished
                                    var demolishCheckBlueprint = new BluePrint(building.buildingType.Value);
                                    if (demolishCheckBlueprint.moneyRequired < 0)
                                    {
                                        // Hard-coded magic number (< 0) means it cannot be demolished
                                        chatBox.textBoxEnter("/message " + farmer.Name + " Error: This building can't be demolished.");
                                        return;
                                    }
                                    else if (demolishCheckBlueprint.name == "Shipping Bin")
                                    {
                                        int num = 0;
                                        foreach (var b in Game1.getFarm().buildings)
                                        {
                                            if (b is ShippingBin)
                                            {
                                                num++;
                                            }

                                            if (num > 1)
                                            {
                                                break;
                                            }
                                        }

                                        if (num <= 1)
                                        {
                                            // Must have at least one shipping bin at all times.
                                            chatBox.textBoxEnter("/message " + farmer.Name + " Error: Can't demolish the last shipping bin.");
                                            return;
                                        }
                                    }

                                    if (building.indoors.Value is Cabin)
                                    {
                                        Cabin cabin = building.indoors.Value as Cabin;
                                        if (cabin.farmhand.Value != null && cabin.farmhand.Value.isCustomized.Value)
                                        {
                                            // The cabin is owned by someone. Ask the player if they're certain; record in memory the action to destroy the building.
                                            var responseActions = new Dictionary<string, Action>();
                                            responseActions["yes"] = genDestroyCabinAction(farmer.Name, building);
                                            responseActions["no"] = genCancelDestroyCabinAction(farmer.Name);
                                            chatBox.RegisterFarmerResponseActionGroup(farmer.UniqueMultiplayerID, responseActions);
                                            chatBox.textBoxEnter("/message " + farmer.Name + " This cabin belongs to a player. Are you sure you want to remove it? Message me \"yes\" or \"no\".");
                                            return;
                                        }
                                    }

                                    // The cabin doesn't belong to anyone. Destroy it immediately without confirmation.
                                    destroyCabin(farmer.Name, building, f);
                                    return;
                                }
                            }

                            chatBox.textBoxEnter("/message " + farmer.Name + " Error: No building found. You must be standing next to a building and facing it.");
                        }
                        else
                        {
                            chatBox.textBoxEnter("/message " + farmer.Name + " Error: You cannot demolish buildings outside of the farm.");
                        }
                        break;
                    }
                }
            }
        }
    }
}
