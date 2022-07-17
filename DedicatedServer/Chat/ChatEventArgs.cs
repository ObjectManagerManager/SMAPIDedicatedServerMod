using StardewValley;

namespace DedicatedServer.Chat
{
    internal struct ChatEventArgs
    {
        public long SourceFarmerId { get; set; }
        public int ChatKind { get; set; }
        public LocalizedContentManager.LanguageCode LanguageCode { get; set; }
        public string Message { get; set; }
    }
}
