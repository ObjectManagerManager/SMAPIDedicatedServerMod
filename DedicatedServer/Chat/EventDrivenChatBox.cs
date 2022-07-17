using StardewValley;
using StardewValley.Menus;
using System;

namespace DedicatedServer.Chat
{
    internal class EventDrivenChatBox : ChatBox
    {
        public event EventHandler<ChatEventArgs> ChatReceived;
        public EventDrivenChatBox() : base() {}

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
    }
}
