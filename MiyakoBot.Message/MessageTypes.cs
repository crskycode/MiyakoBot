namespace MiyakoBot.Message
{
    /// <summary>
    /// See https://github.com/project-mirai/mirai-api-http/blob/master/docs/api/MessageType.md
    /// </summary>
    public enum MessageTypes
    {
        None,
        FriendMessage,
        GroupMessage,
        TempMessage,
        StrangerMessage,
        OtherClientMessage,
        FriendSyncMessage,
        GroupSyncMessage,
        TempSyncMessage,
        StrangerSyncMessage
    }
}
