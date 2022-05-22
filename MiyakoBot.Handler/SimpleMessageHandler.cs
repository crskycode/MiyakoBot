﻿using Microsoft.Extensions.Logging;
using MiyakoBot.Message;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;

namespace MiyakoBot.Handler
{
    [MessageHandler]
    public class SimpleMessageHandler : MessageHandlerBase
    {
        readonly ILogger<SimpleMessageHandler> _logger;

        public SimpleMessageHandler(ILogger<SimpleMessageHandler> logger)
        {
            _logger = logger;
        }

        [Message(MessageTypes.GroupMessage)]
        public async Task OnGroupMessage(Func<JsonObject,CancellationToken,Task<JsonObject>> funcSendAsync, JsonObject dataObject, CancellationToken cancellationToken)
        {
            var groupId = (uint?)dataObject["sender"]?["group"]?["id"];

            var message = new MessageChainBuilder()
                .AddPlain("123")
                .AddPlain("abc")
                .Build();

            var contentObject = new JsonObject {
                ["command"] = "sendGroupMessage",
                ["subCommand"] = null,
                ["content"] = new JsonObject {
                    ["target"] = groupId,
                    ["messageChain"] = message.ToJson()
                }
            };

            var result = await funcSendAsync(contentObject, cancellationToken);

            _logger.LogDebug("Get Message Id: {}", result["messageId"] );
        }
    }
}