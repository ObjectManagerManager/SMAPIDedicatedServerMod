using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace DedicatedServer.Chat
{
    internal class EventDrivenChatBox : ChatBox
    {
        public event EventHandler<ChatEventArgs> ChatReceived;
        private Dictionary<long, Dictionary<string, Tuple<List<string>, Action>>> farmerResponseActions = new Dictionary<long, Dictionary<string, Tuple<List<string>, Action>>>();

        public EventDrivenChatBox() : base()
        {
            ChatReceived += tryResponseAction;
        }

        private void tryResponseAction(object sender, ChatEventArgs e)
        {
            if (e.ChatKind == 3 &&
                    farmerResponseActions.TryGetValue(e.SourceFarmerId, out var responseActionsForFarmer) &&
                    responseActionsForFarmer.TryGetValue(e.Message.ToLower(), out var responseAction)) {
                // Remove all response actions grouped with this response. This must be done
                // before executing the action, which could in-turn overwrite some of these
                // grouped responses. Otherwise, the overwritten one would be deleted.
                foreach (var groupedResponse in responseAction.Item1)
                {
                    responseActionsForFarmer.Remove(groupedResponse);
                }

                // Execute the action if not null
                if (responseAction.Item2 != null)
                {
                    responseAction.Item2();
                }
            }
        }

        public override void receiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
        {
            base.receiveChatMessage(sourceFarmer, chatKind, language, message);
            if (ChatReceived != null)
            {
                var args = new ChatEventArgs
                {
                    SourceFarmerId = sourceFarmer,
                    ChatKind = chatKind,
                    LanguageCode = language,
                    Message = message
                };
                ChatReceived(this, args);
            }
        }

        public void RegisterFarmerResponseActionGroup(long farmerId, Dictionary<string, Action> responseActions)
        {
            Dictionary<string, Tuple<List<string>, Action>> responseActionsForFarmer;
            if (farmerResponseActions.TryGetValue(farmerId, out responseActionsForFarmer))
            {
                // Remove existing response groups for these farmer / responses. That is,
                // remove each of the responses as well as each of the responses grouped
                // with any of these responses.
                foreach (var response in responseActions.Keys)
                {
                    if (responseActionsForFarmer.TryGetValue(response, out var responseActionGroup))
                    {
                        foreach (var groupedResponse in responseActionGroup.Item1)
                        {
                            responseActionsForFarmer.Remove(groupedResponse);
                        }
                    }
                }
            }
            else
            {
                // The farmer does not yet have any response groups recorded; initialize
                // an empty dictionary for them
                farmerResponseActions.Add(farmerId, new Dictionary<string, Tuple<List<string>, Action>>());
                responseActionsForFarmer = farmerResponseActions[farmerId];
            }

            // Construct list of grouped response actions
            var responseGroup = new List<string>();
            foreach (var response in responseActions.Keys)
            {
                responseGroup.Add(response);
            }

            // Register all of the response actions
            foreach (var responseAction in responseActions)
            {
                responseActionsForFarmer[responseAction.Key] = new Tuple<List<string>, Action>(responseGroup, responseAction.Value);
            }
        }
    }
}
