using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiyakoBot.Http;
using MiyakoBot.Message;

namespace MiyakoBot.Adapter
{
    public class MiraiWebSocketAdapter : IAdapter
    {
        readonly ILogger<MiraiWebSocketAdapter> _logger;
        readonly MiraiWebSocketAdapterSettings _settings;
        readonly IHttpClient _httpClient;
        readonly IServiceProvider _applicationServiceProvider;
        readonly MessageHandlerTypeCollection _messageHandlers;
        readonly MiraiSession _session;
        readonly ClientWebSocket _socket;

        public MiraiWebSocketAdapter(
            ILogger<MiraiWebSocketAdapter> logger,
            IConfiguration configuration,
            MessageHandlerTypeCollection messageHandlers,
            IDefaultHttpClient httpClient,
            IServiceProvider applicationServiceProvider)
        {
            _logger = logger;
            _settings = configuration.GetSection(nameof(MiraiWebSocketAdapter)).Get<MiraiWebSocketAdapterSettings>();
            _httpClient = httpClient;
            _applicationServiceProvider = applicationServiceProvider;
            _messageHandlers = messageHandlers;

            var sessionLogger = applicationServiceProvider.GetRequiredService<ILogger<MiraiSession>>();
            var sessionSettings = new MiraiSessionSettings
            {
                Host = _settings.HttpHost,
                Port = _settings.HttpPort,
                VerifyKey = _settings.VerifyKey,
                QQ = _settings.QQ
            };
            _session = new MiraiSession(sessionLogger, _httpClient, sessionSettings);

            _socket = new ClientWebSocket();
            _socket.Options.KeepAliveInterval = TimeSpan.FromMinutes(2);
        }

        Uri GetWsConnectionUri(string sessionKey)
        {
            return new Uri($"ws://{_settings.WsHost}:{_settings.WsPort}/all?verifyKey={_settings.VerifyKey}&sessionKey={sessionKey}&qq={_settings.QQ}");
        }

        void HandleConnectionMessage(JsonObject message)
        {
            try
            {
                var code = (int?)message["code"];

                if (code == 0)
                {
                    if (message.ContainsKey("session"))
                    {
                        _logger.LogInformation("Connect to mirai successfully.");
                    }
                }
                else
                {
                    var msg = (string?)message["msg"] ?? "An unknown error has occurred.";
                    _logger.LogError("Failed to connect to mirai: {}", msg);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse message.");
            }
        }

        List<MethodInfo> LookupMessageHandlers(MessageTypes type)
        {
            var handlers = new List<MethodInfo>();

            foreach (var typeInfo in _messageHandlers)
            {
                var methods = typeInfo.GetMethods();

                foreach (var item in methods)
                {
                    var attr = item.GetCustomAttribute<MessageAttribute>();

                    if (attr?.Type == type)
                    {
                        handlers.Add(item);
                    }
                }
            }

            return handlers;
        }

        async Task DispatchMessageToHandlersAsync(MessageTypes type, JsonObject message, CancellationToken cancellationToken)
        {
            var handlers = LookupMessageHandlers(type);

            foreach (var item in handlers)
            {
                _logger.LogDebug("Invoke message handler: {}.{}", item.DeclaringType!.FullName, item.Name);

                // Get a handler instance.
                var obj = _applicationServiceProvider.GetRequiredService(item.DeclaringType);
                // Prepare parameters for method.
                var args = new object[] { _socket, message, cancellationToken };

                if (item.Invoke(obj, args) is Task task)
                {
                    await task;
                }
            }
        }

        async Task HandleMessageAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var type = message["type"];

            if (type == null)
            {
                _logger.LogError("Missing 'type' in message content.");
                return;
            }

            if (Enum.TryParse<MessageTypes>(type.ToString(), out var messageType))
            {
                if (messageType != MessageTypes.None)
                {
                    await DispatchMessageToHandlersAsync(messageType, message, cancellationToken);
                    return;
                }
            }

            _logger.LogWarning("Message was not handled. Type: {}", type);
        }

        async Task DispatchMessageAsync(JsonObject message, CancellationToken cancellationToken)
        {
            try
            {
                var syncId = message["syncId"];

                if (syncId == null)
                {
                    _logger.LogError("Missing 'syncId' in incoming message.");
                    return;
                }

                var data = message["data"];

                if (data == null)
                {
                    _logger.LogError("Missing 'data' in incoming message.");
                    return;
                }

                var messageBody = data.AsObject();

                // This message actively pushed by the server.
                if (syncId.ToString() == _settings.SyncId)
                {
                    await HandleMessageAsync(messageBody, cancellationToken);
                    return;
                }

                // If syncId is an empty string, the message is connection result.
                if (syncId.ToString() == string.Empty)
                {
                    HandleConnectionMessage(messageBody);
                    return;
                }

                _logger.LogWarning("Incoming message was not handled.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to dispatch message.");
            }
        }

        async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            var buffer = new List<byte>(ushort.MaxValue);
            var block = new byte[ushort.MaxValue];

            while (!cancellationToken.IsCancellationRequested &&
                _socket.State == WebSocketState.Open)
            {
                buffer.Clear();

                WebSocketReceiveResult result;

                do
                {
                    result = await _socket.ReceiveAsync(block, cancellationToken);

                    if (result.Count > 0)
                    {
                        buffer.AddRange(new ArraySegment<byte>(block, 0, result.Count));
                    }

                } while (!result.EndOfMessage);

                if (buffer.Count == 0)
                {
                    continue;
                }

                try
                {
                    var jsonString = Encoding.UTF8.GetString(buffer.ToArray());

                    _logger.LogDebug("Message: {}", jsonString);

                    var jsonObject = JsonNode.Parse(jsonString);

                    if (jsonObject != null)
                    {
                        await DispatchMessageAsync(jsonObject.AsObject(), cancellationToken);
                    }
                }
                catch (JsonException e)
                {
                    _logger.LogError(e, "Failed to parse incoming message.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to deserialize message.");
                }
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await _session.Open();

            try
            {
                await _session.Bind();

                await _socket.ConnectAsync(GetWsConnectionUri(_session.Key), cancellationToken);

                await ReceiveAsync(cancellationToken);

                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
            }
            finally
            {
                await _session.Close();
            }
        }
    }
}
