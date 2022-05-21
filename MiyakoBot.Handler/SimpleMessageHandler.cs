using Microsoft.Extensions.Logging;
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
        public async Task OnGroupMessage(ClientWebSocket socket, JsonObject dataObject, CancellationToken cancellationToken)
        {
            var groupId = (uint?)dataObject["sender"]?["group"]?["id"];

            if (groupId != 201614125)
            {
                return;
            }

            var message = new MessageChainBuilder()
                .AddPlain("123")
                .AddPlain("abc")
                .Build();

            var contentObject = new JsonObject {
                ["syncId"] = Guid.NewGuid().ToString(),
                ["command"] = "sendGroupMessage",
                ["subCommand"] = null,
                ["content"] = new JsonObject {
                    ["target"] = groupId,
                    ["messageChain"] = message.ToJson()
                }
            };

            var jsonString = contentObject.ToJsonString();
            var jsonData = Encoding.UTF8.GetBytes(jsonString);
            await socket.SendAsync(jsonData, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}