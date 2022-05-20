namespace MiyakoBot.Message
{
    /// <summary>
    /// See https://github.com/project-mirai/mirai-api-http/blob/master/docs/api/MessageType.md
    /// </summary>
    public enum MessageTypes
    {
        None,
        // Events
        BotOnlineEvent,
        BotOfflineEventActive,
        BotOfflineEventForce,
        BotOfflineEventDropped,
        BotReloginEvent,
        FriendInputStatusChangedEvent,
        FriendNickChangedEvent,
        BotGroupPermissionChangeEvent,
        BotMuteEvent,
        BotUnmuteEvent,
        BotJoinGroupEvent,
        BotLeaveEventActive,
        BotLeaveEventKick,
        GroupRecallEvent,
        FriendRecallEvent,
        NudgeEvent,
        GroupNameChangeEvent,
        GroupEntranceAnnouncementChangeEvent,
        GroupMuteAllEvent,
        GroupAllowAnonymousChatEvent,
        GroupAllowConfessTalkEvent,
        GroupAllowMemberInviteEvent,
        MemberJoinEvent,
        MemberLeaveEventKick,
        MemberLeaveEventQuit,
        MemberCardChangeEvent,
        MemberSpecialTitleChangeEvent,
        MemberPermissionChangeEvent,
        MemberMuteEvent,
        MemberUnmuteEvent,
        MemberHonorChangeEvent,
        NewFriendRequestEvent,
        MemberJoinRequestEvent,
        BotInvitedJoinGroupRequestEvent,
        OtherClientOnlineEvent,
        OtherClientOfflineEvent,
        CommandExecutedEvent,
        // Messages
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
