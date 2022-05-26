using Microsoft.Extensions.Logging;
using MiyakoBot.Message;
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
        public async Task OnGroupMessage(JsonObject dataObject, CancellationToken cancellationToken)
        {
            var groupId = dataObject["sender"]!["group"]!["id"]!.ToString();
            var messageContent = new MessageChain(dataObject["messageChain"]!.AsArray());
            var messageString = messageContent.ToString();

            if (messageString == "发张涩图" || messageString == "不够涩")
            {
                var files = Directory.GetFiles(@"E:\Workflow\Library");

                if (files.Length > 0)
                {
                    var r = new Random();
                    var i = r.NextInt64(0, files.LongLength - 1);

                    var message = new MessageChainBuilder()
                        .AddLocalImage(files[i])
                        .Build();

                    await SendGroupMessageAsync(groupId, message, cancellationToken);
                }
            }
        }
    }
}

