using System.Text.Json.Nodes;

namespace MiyakoBot.Message
{
    public class MessageHandlerBase
    {
        public Func<JsonObject, CancellationToken, Task<JsonObject>>? SocketSendAsync { get; set; }

        public MessageHandlerBase()
        {
        }

        private Task<JsonObject> InternalSendAsync(JsonObject message, CancellationToken cancellationToken)
        {
            if (SocketSendAsync == null)
            {
                throw new InvalidOperationException();
            }

            return SocketSendAsync(message, cancellationToken);
        }

        /// <summary>
        /// Get the list of friends.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        protected async Task<JsonObject> GetFriendListAsync(CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "friendList",
                ["subCommand"] = null,
                ["content"] = null
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Get the list of groups.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        protected async Task<JsonObject> GetGroupListAsync(CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "groupList",
                ["subCommand"] = null,
                ["content"] = null
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Get the list of members of a group.
        /// </summary>
        /// <param name="groupId">Group number.</param>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        /// <returns></returns>
        protected async Task<JsonObject> GetGroupMemberListAsync(string groupId, CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "memberList",
                ["subCommand"] = null,
                ["content"] = new JsonObject
                {
                    ["target"] = uint.Parse(groupId)
                }
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Get the profile of the bot.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        /// <returns></returns>
        protected async Task<JsonObject> GetBotProfileAsync(CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "botProfile",
                ["subCommand"] = null,
                ["content"] = null
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Get a friend's profile.
        /// </summary>
        /// <param name="targetId">QQ number.</param>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        /// <returns></returns>
        protected async Task<JsonObject> GetFriendProfileAsync(string targetId, CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "friendProfile",
                ["subCommand"] = null,
                ["content"] = new JsonObject
                {
                    ["target"] = uint.Parse(targetId)
                }
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Get the profile of group member.
        /// </summary>
        /// <param name="groupId">QQ number.</param>
        /// <param name="memberId">QQ number of the group member.</param>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        /// <returns></returns>
        protected async Task<JsonObject> GetGroupMemberProfileAsync(string groupId, string memberId, CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "memberProfile",
                ["subCommand"] = null,
                ["content"] = new JsonObject
                {
                    ["target"] = uint.Parse(groupId),
                    ["memberId"] = uint.Parse(memberId)
                }
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Get the profile of user.
        /// </summary>
        /// <param name="targetId">QQ number.</param>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        /// <returns></returns>
        protected async Task<JsonObject> GetUserProfileAsync(string targetId, CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "userProfile",
                ["subCommand"] = null,
                ["content"] = new JsonObject
                {
                    ["target"] = uint.Parse(targetId)
                }
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Send message to a friend.
        /// </summary>
        /// <param name="targetId">QQ number.</param>
        /// <param name="message">Message content.</param>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        /// <returns></returns>
        protected async Task<JsonObject> SendFriendMessageAsync(string targetId, MessageChain message, CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "sendFriendMessage",
                ["subCommand"] = null,
                ["content"] = new JsonObject
                {
                    ["target"] = uint.Parse(targetId),
                    ["messageChain"] = message.ToJson()
                }
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Send message to a group.
        /// </summary>
        /// <param name="groupId">Group number.</param>
        /// <param name="message">Message content.</param>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        /// <returns></returns>
        protected async Task<JsonObject> SendGroupMessageAsync(string groupId, MessageChain message, CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "sendGroupMessage",
                ["subCommand"] = null,
                ["content"] = new JsonObject
                {
                    ["target"] = uint.Parse(groupId),
                    ["messageChain"] = message.ToJson()
                }
            };

            return await InternalSendAsync(obj, cancellationToken);
        }

        /// <summary>
        /// Send temporary session message.
        /// </summary>
        /// <param name="groupId">Group number.</param>
        /// <param name="memberId">QQ number of the group member.</param>
        /// <param name="message">Message content.</param>
        /// <param name="cancellationToken">A token to cancel asynchronous operation.</param>
        /// <returns></returns>
        protected async Task<JsonObject> SendTempMessageAsync(string groupId, string memberId, MessageChain message, CancellationToken cancellationToken)
        {
            var obj = new JsonObject
            {
                ["command"] = "sendTempMessage",
                ["subCommand"] = null,
                ["content"] = new JsonObject
                {
                    ["group"] = uint.Parse(groupId),
                    ["qq"] = uint.Parse(memberId),
                    ["messageChain"] = message.ToJson()
                }
            };

            return await InternalSendAsync(obj, cancellationToken);
        }
    }
}
