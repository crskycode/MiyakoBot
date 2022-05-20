﻿using Microsoft.Extensions.Logging;
using MiyakoBot.Message;
using System.Text.Json.Nodes;

namespace MiyakoBot.Handler
{
    [MessageHandler]
    public class SimpleMessageHandler
    {
        readonly ILogger<SimpleMessageHandler> _logger;

        public SimpleMessageHandler(ILogger<SimpleMessageHandler> logger)
        {
            _logger = logger;
        }

        [Message(MessageTypes.GroupMessage)]
        public void OnGroupMessage(JsonObject dataObject, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Test on group message!!!");
        }
    }
}