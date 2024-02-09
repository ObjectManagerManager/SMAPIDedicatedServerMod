using StardewValley.Menus;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DedicatedServer.Chat;
using DedicatedServer.Config;
using StardewModdingAPI;
using StardewValley.Minigames;

namespace DedicatedServer.Utils
{
    internal class ServerHost
    {        
        private static EventDrivenChatBox chatBox;

        public ServerHost(EventDrivenChatBox chatBox)
        {
            ServerHost.chatBox = chatBox;
        }

        /// <summary>
        ///         Empty the host inventory, tools are not deleted
        /// </summary>
        static public void EmptyHostInventory()
        {
            for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                var item = Game1.player.Items[i];

                if (null == item) continue;

                if (item.canBeTrashed())
                {
                    chatBox?.textBoxEnter($" Item {item.Name} deleted");
                    Game1.player.removeItemFromInventory(item);
                }
            }
        }

        /// <summary>
        ///         Ensure that a number of free slots are available
        /// </summary>
        /// <param name="numberOfFreeSlot"></param>
        /// <returns>The result of the method:
        /// <br/>   true : The requested slots are free
        /// <br/>   false: The requested slots could not be provided</returns>
        static public bool EnsureFreeSlotNumber(int numberOfFreeSlot)
        {
            for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                var item = Game1.player.Items[i];

                if (null == item)
                {
                    numberOfFreeSlot--;
                }
                else
                {
                    if (item.canBeTrashed())
                    {
                        chatBox?.textBoxEnter($" Item {item.Name} deleted");
                        Game1.player.removeItemFromInventory(item);
                        numberOfFreeSlot--;
                    }
                }

                if(numberOfFreeSlot <= 0)
                {
                    break;
                }
            }

            if(0 < numberOfFreeSlot)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        ///         Deletes the last set of items when the inventory is full
        /// <br/>   Goes from right to left, tools are not deleted
        /// </summary>
        static public void ClearLastItem()
        {
            if (Game1.player.isInventoryFull())
            {
                for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
                {
                    var item = Game1.player.Items[i];

                    if (null == item) continue;

                    if (item.canBeTrashed())
                    {
                        chatBox?.textBoxEnter($" Item {item.Name} dumped");
                        Game1.player.removeItemFromInventory(item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///         Closes all open menus
        /// </summary>
        static public void ForceClosingAllMenu()
        {
            while (null != Game1.activeClickableMenu)
            {
                Game1.activeClickableMenu = null;
                if (Game1.CurrentEvent != null)
                {
                    Game1.CurrentEvent.CurrentCommand++;
                }

                Game1.playSound("bigDeSelect");
            }
        }
    }
}
