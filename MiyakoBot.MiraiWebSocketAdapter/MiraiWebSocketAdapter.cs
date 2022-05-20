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

        void HandleConnectionMessage(JsonObject dataObject)
        {
            var code = (int?)dataObject["code"];
            var session = (string?)dataObject["session"];
            var msg = (string?)dataObject["msg"];

            if (code == 0 && session != null)
            {
                _logger.LogInformation("Connect to mirai successfully.");
            }
            else
            {
                _logger.LogError("Failed to connect to mirai: {}", msg ?? "An unknown error has occurred.");
            }
        }

        List<MethodInfo> LookupMessageHandlers(MessageTypes type)
        {
            var handlers = new List<MethodInfo>();

            foreach (var handlerType in _messageHandlers)
            {
                var methods = handlerType.GetMethods();

                foreach (var item in methods)
                {
                    var attribute = item.GetCustomAttribute<MessageAttribute>();

                    if (attribute != null && attribute.Type == type)
                    {
                        handlers.Add(item);
                    }
                }
            }

            return handlers;
        }

        void DispatchBotMessage(MessageTypes type, JsonObject dataObject, CancellationToken cancellationToken)
        {
            var handlers = LookupMessageHandlers(type);

            foreach (var item in handlers)
            {
                _logger.LogDebug("Invoke message handler: {}.{}", item.DeclaringType!.FullName, item.Name);

                Task.Run(() => {
                    try
                    {
                        var obj = _applicationServiceProvider.GetRequiredService(item.DeclaringType);
                        var args = new object[] { dataObject, cancellationToken };
                        item.Invoke(obj, args);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to invoke message handler.");
                    }
                }, cancellationToken);
            }
        }

        void HandleMessage(JsonObject dataObject, CancellationToken cancellationToken)
        {
            var typeString = (string?)dataObject["type"];

            if (string.IsNullOrEmpty(typeString))
            {
                _logger.LogError("Missing value of message type.");
                return;
            }

            if (Enum.TryParse<MessageTypes>(typeString, out var messageType))
            {
                if (messageType != MessageTypes.None)
                {
                    DispatchBotMessage(messageType, dataObject, cancellationToken);
                }
                return;
            }

            _logger.LogWarning("Unhandled message type: {}", typeString);
        }

        void DispatchMessage(JsonObject message, CancellationToken cancellationToken)
        {
            try
            {
                if (!message.ContainsKey("syncId"))
                {
                    _logger.LogError("Missing 'syncId' in incoming message.");
                    return;
                }

                var dataNode = message["data"];

                if (dataNode == null)
                {
                    _logger.LogError("Missing 'data' in incoming message.");
                    return;
                }

                var dataObject = dataNode.AsObject();

                if (dataObject.ContainsKey("code"))
                {
                    if (dataObject.ContainsKey("session") || dataObject.ContainsKey("msg"))
                    {
                        HandleConnectionMessage(dataObject);
                        return;
                    }
                }

                if (dataObject.ContainsKey("type"))
                {
                    HandleMessage(dataObject, cancellationToken);
                    return;
                }
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

                try
                {
                    if (buffer.Count == 0)
                    {
                        continue;
                    }

                    var jsonString = Encoding.UTF8.GetString(buffer.ToArray());

                    _logger.LogDebug("Message: {}", jsonString);

                    var jsonObject = JsonNode.Parse(jsonString);

                    if (jsonObject != null)
                    {
                        DispatchMessage(jsonObject.AsObject(), cancellationToken);
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
