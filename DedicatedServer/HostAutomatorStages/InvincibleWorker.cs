using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    /// <summary>
    /// Makes the host invincible
    /// </summary>
    internal class InvincibleWorker
    {
        /// <summary>
        ///         Overwrites the behavior of the class <see cref="InvincibleWorker"/>
        /// <br/>   
        /// <br/>   null : The behavior of this class is not changed.
        /// <br/>   true : Host is invincible
        /// <br/>   false: Host is not invincible
        /// <br/>   
        /// <br/>   Works only if <see cref="OnUpdateTicked"/> ticks
        /// </summary>
        protected static bool? InvincibilityOverwrite { get; set; } = null;

        private enum InvincibleWorkerStates
        {
            WaitingForWorldIsReady = 0,
            Mortal,
            ToMortal,
            Immortal,
        }

        private InvincibleWorkerStates state;

        private IModHelper helper = null;

        public InvincibleWorker(IModHelper helper)
        {
            this.helper = helper;
        }

        public void Enable()
        {
            helper.Events.GameLoop.OneSecondUpdateTicked += OneSecondUpdateTicked;
        }

        public void Disable()
        {
            Game1.player.temporaryInvincibilityTimer = 0;
            helper.Events.GameLoop.OneSecondUpdateTicked -= OneSecondUpdateTicked;
        }

        private void OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            switch (state)
            {
                case InvincibleWorkerStates.WaitingForWorldIsReady:
                    if (Context.IsWorldReady)
                    {
                        state = InvincibleWorkerStates.Immortal;
                    }
                    break;

                case InvincibleWorkerStates.Mortal:
                    if (false != InvincibilityOverwrite)
                    {
                        state = InvincibleWorkerStates.Immortal;
                    }
                    break;

                case InvincibleWorkerStates.ToMortal:
                    Game1.player.temporaryInvincibilityTimer = 0;
                    state = InvincibleWorkerStates.Mortal;
                    break;

                case InvincibleWorkerStates.Immortal:
                    if (false == InvincibilityOverwrite)
                    {
                        state = InvincibleWorkerStates.ToMortal;
                    }
                    Game1.player.temporarilyInvincible = true;
                    Game1.player.temporaryInvincibilityTimer = -1000000000;
                    break;

                default:
                    break;
            }
        }
    }
}
