using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class ProcessPauseBehaviorLink : BehaviorLink
    {
        /// <summary>
        ///         This allows you to deactivate the execution of host automation. Default is true
        /// <br/>   
        /// <br/>   true : The normal behavior of this mod is running
        /// <br/>   false: Keeps the behavior of the mod as it would be in the paused state
        /// <br/>   
        /// <br/>   Works only if <see cref="AutomatedHost"/> ticks and the <see cref="BehaviorChain"/> is executed.
        /// </summary>
        protected static bool enableHostAutomation = true;

        /// <summary>
        ///         Prevents the pause state. Default is false
        /// <br/>   
        /// <br/>   true : The server never switches to pause mode
        /// <br/>   false: The normal behavior of this mod is running
        /// </summary>
        protected static bool preventPause = false;

        private bool IsPaused
        {
            set { Game1.netWorldState.Value.IsPaused = value; }
            get { return Game1.netWorldState.Value.IsPaused; }
        }

        public ProcessPauseBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        private enum internalStates
        {
            /// <summary> The server runs as long as players are online </summary>
            WaitingForPlayersToLeave = 0,

            /// <summary> Transitional action, enables pause </summary>
            EnablePause,

            /// <summary> The server is paused as long as no players are online </summary>
            WaitingForUpcomingPlayers,

            /// <summary> Transitional action, disables pause </summary>
            DisablePause,


            /// <summary> Transitional action, disables pause </summary>
            PreparePreventPause,

            /// <summary>
            ///         Prevents the pause state. You can switch to this state 
            /// <br/>   from any state and switch back to the original state.
            /// </summary>
            PreventPause,


            /// <summary>
            ///         If the server is running, you can pause the game by setting
            /// <br/>   <see cref="Game1.netWorldState.Value.IsPaused"/> to true.
            /// <br/>   To go back, the same value must be set to false.
            /// <br/>   
            /// <br/>   So you can come from state <see cref="WaitingForPlayersToLeave"/>
            /// <br/>   to this one and go back.
            /// </summary>
            ExternalPause,

            /// <summary>
            ///         If the server is paused, you can run the game by setting
            /// <br/>   <see cref="Game1.netWorldState.Value.IsPaused"/> to false.
            /// <br/>   To go back, the same value must be set to true.
            /// <br/>   
            /// <br/>   So you can come from state <see cref="WaitingForUpcomingPlayers"/>
            /// <br/>   to this one and go back.
            /// </summary>
            ExternalPauseDisabled,
        }

        private internalStates internalState = internalStates.WaitingForPlayersToLeave;

        private internalStates saveInternalState;
        private bool saveIsPaused;

        public override void Process(BehaviorState state)
        {
            if (preventPause && 
                internalStates.PreventPause != internalState)
            {
                saveInternalState = internalState;
                saveIsPaused = IsPaused;
                internalState = internalStates.PreparePreventPause;
            }

            switch (internalState)
            {
                case internalStates.WaitingForPlayersToLeave:
                    if (IsPaused)
                    {
                        internalState = internalStates.ExternalPause;
                        return; 
                    }

                    if (  0   == state.GetNumOtherPlayers() && // If no other player is online
                        false == Game1.isFestival()         )  // if it is not a festival
                    {
                        internalState = internalStates.EnablePause;
                        return;
                    }
                    if (enableHostAutomation)
                    {
                        processNext(state);
                    }
                    return;

                case internalStates.EnablePause:
                    IsPaused = true;
                    internalState = internalStates.WaitingForUpcomingPlayers;
                    return;

                case internalStates.WaitingForUpcomingPlayers:
                    if (false == IsPaused)
                    {
                        internalState = internalStates.ExternalPauseDisabled;
                        return;
                    }

                    if (  0  <  state.GetNumOtherPlayers() || 
                        true == Game1.isFestival()         )
                    {
                        internalState = internalStates.DisablePause;
                    }
                    return;

                case internalStates.DisablePause:
                    IsPaused = false;
                    internalState = internalStates.WaitingForPlayersToLeave;
                    return;

                case internalStates.PreparePreventPause:
                    IsPaused = false;
                    internalState = internalStates.PreventPause;
                    goto case internalStates.PreventPause;

                case internalStates.PreventPause:
                    if (false == preventPause)
                    {
                        internalState = saveInternalState;
                        IsPaused = saveIsPaused;
                        return;
                    }
                    if (enableHostAutomation)
                    {
                        processNext(state);
                    }
                    return;

                case internalStates.ExternalPause:
                    if (false == IsPaused)
                    {
                        internalState = internalStates.WaitingForPlayersToLeave;
                        return;
                    }
                    return;

                case internalStates.ExternalPauseDisabled:
                    if (true == IsPaused)
                    {
                        internalState = internalStates.WaitingForUpcomingPlayers;
                        return;
                    }
                    if (enableHostAutomation)
                    {
                        processNext(state);
                    }
                    return;
            }
        }
    }
}
