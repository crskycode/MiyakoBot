namespace MiyakoBot.Message
{
    /// <summary>
    /// See https://github.com/project-mirai/mirai-api-http/blob/master/docs/api/EventType.md
    /// </summary>
    public enum EventTypes
    {
        None,
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
        CommandExecutedEvent
    }
}
