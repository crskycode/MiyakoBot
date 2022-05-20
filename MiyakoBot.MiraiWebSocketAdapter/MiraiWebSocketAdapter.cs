using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiyakoBot.Http;
using MiyakoBot.Message;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MiyakoBot.Adapter
{
    public class MiraiWebSocketAdapter : IAdapter
    {
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<MiraiWebSocketAdapter> _logger;
        readonly MiraiWebSocketAdapterSettings _settings;
        readonly IHttpClient _httpClient;
        readonly MiraiSession _session;
        readonly ClientWebSocket _socket;
        readonly ApplicationPartManager _applicationPartManager;
        readonly List<object> _messageHandlers;
        readonly Dictionary<string, EventTypes> _stringToEventTypeDict;
        readonly Dictionary<string, MessageTypes> _stringToMessageTypeDict;

        public MiraiWebSocketAdapter(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            ILogger<MiraiWebSocketAdapter> logger,
            IConfiguration configuration,
            IDefaultHttpClient httpClient,
            ApplicationPartManager applicationPartManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = configuration.GetSection(nameof(MiraiWebSocketAdapter)).Get<MiraiWebSocketAdapterSettings>();
            _httpClient = httpClient;
            _applicationPartManager = applicationPartManager;
            _messageHandlers = new();
            _stringToEventTypeDict = new();
            _stringToMessageTypeDict = new();

            // Create session object
            var sessionLogger = loggerFactory.CreateLogger<MiraiSession>();
            var sessionSettings = new MiraiSessionSettings
            {
                Host = _settings.HttpHost,
                Port = _settings.HttpPort,
                VerifyKey = _settings.VerifyKey,
                QQ = _settings.QQ
            };
            _session = new MiraiSession(sessionLogger, httpClient, sessionSettings);

            // Create WebSocket object
            _socket = new ClientWebSocket();
            _socket.Options.KeepAliveInterval = TimeSpan.FromMinutes(2);

            // Find all message handlers and set up mappings
            PrepareMessageHandlers();
            PrepareMessageTypes();
        }

        Uri GetWsConnectionUri(string sessionKey)
        {
            return new Uri($"ws://{_settings.WsHost}:{_settings.WsPort}/all?verifyKey={_settings.VerifyKey}&sessionKey={sessionKey}&qq={_settings.QQ}");
        }

        void PrepareMessageHandlers()
        {
            var feature = new MessageHandlerFeature();

            _applicationPartManager.PopulateFeature(feature);

            foreach (var type in feature.Handlers)
            {
                var handler = _serviceProvider.GetService(type);

                if (handler != null)
                {
                    _messageHandlers.Add(handler);
                }
            }
        }

        void PrepareMessageTypes()
        {
            var eventTypeInfo = typeof(EventTypes);

            if (eventTypeInfo.IsEnum)
            {
                var fields = eventTypeInfo.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var field in fields)
                {
                    if (field.IsLiteral)
                    {
                        var value = (EventTypes)field.GetRawConstantValue()!;
                        _stringToEventTypeDict.Add(field.Name, value);
                    }
                }
            }

            var messageTypeInfo = typeof(MessageTypes);

            if (messageTypeInfo.IsEnum)
            {
                var fields = messageTypeInfo.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var field in fields)
                {
                    if (field.IsLiteral)
                    {
                        var value = (MessageTypes)field.GetRawConstantValue()!;
                        _stringToMessageTypeDict.Add(field.Name, value);
                    }
                }
            }
        }

        EventTypes GetEventTypeFromString(string type)
        {
            if (_stringToEventTypeDict.TryGetValue(type, out EventTypes eventType))
            {
                return eventType;
            }

            return EventTypes.None;
        }

        MessageTypes GetMessageTypeFromString(string type)
        {
            if (_stringToMessageTypeDict.TryGetValue(type, out MessageTypes messageType))
            {
                return messageType;
            }

            return MessageTypes.None;
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

        void HandleBotMessage(JsonObject dataObject, CancellationToken cancellationToken)
        {
            var botMessageType = (string?)dataObject["type"];

            if (string.IsNullOrEmpty(botMessageType))
            {
                _logger.LogError("Message type is empty.");
                return;
            }

            var eventType = GetEventTypeFromString(botMessageType);

            if (eventType != EventTypes.None)
            {
                foreach (var handler in _messageHandlers)
                {
                    var type = handler.GetType();
                    var methods = type.GetMethods();

                    foreach (var method in methods)
                    {
                        var attribute = method.GetCustomAttribute<EventAttribute>();

                        if (attribute != null && attribute.Type == eventType)
                        {
                            _logger.LogDebug("Invoke event handler: {}.{}", method.ReflectedType!.FullName, method.Name);

                            Task.Run(() => {
                                try
                                {
                                    var parameters = new object[] { dataObject, cancellationToken };
                                    method.Invoke(handler, parameters);
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Failed to invoke event handler.");
                                }
                            }, cancellationToken);
                        }
                    }
                }

                return;
            }

            var messageType = GetMessageTypeFromString(botMessageType);

            if (messageType != MessageTypes.None)
            {
                foreach (var handler in _messageHandlers)
                {
                    var type = handler.GetType();
                    var methods = type.GetMethods();

                    foreach (var method in methods)
                    {
                        var attribute = method.GetCustomAttribute<MessageAttribute>();

                        if (attribute != null && attribute.Type == messageType)
                        {
                            _logger.LogDebug("Invoke message handler: {}.{}", method.ReflectedType!.FullName, method.Name);

                            Task.Run(() => {
                                try
                                {
                                    var parameters = new object[] { dataObject, cancellationToken };
                                    method.Invoke(handler, parameters);
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Failed to invoke message handler.");
                                }
                            }, cancellationToken);
                        }
                    }
                }

                return;
            }
        }

        void DispatchMessageAsync(JsonObject message, CancellationToken cancellationToken)
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
                    HandleBotMessage(dataObject, cancellationToken);
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to dispatch message.");
            }
        }

        async Task ReadSocketAsync(CancellationToken cancellationToken)
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
                        DispatchMessageAsync(jsonObject.AsObject(), cancellationToken);
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

                await ReadSocketAsync(cancellationToken);

                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
            }
            finally
            {
                await _session.Close();
            }
        }
    }
}
