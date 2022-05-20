using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiyakoBot.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MiyakoBot.Adapter
{
    public class MiraiWebSocketAdapter : IAdapter
    {
        readonly ILogger<MiraiWebSocketAdapter> _logger;
        readonly MiraiWebSocketAdapterSettings _settings;
        readonly IHttpClient _httpClient;
        readonly MiraiSession _session;
        readonly ClientWebSocket _socket;
        readonly ApplicationPartManager _applicationPartManager;

        public MiraiWebSocketAdapter(
            ILoggerFactory loggerFactory,
            ILogger<MiraiWebSocketAdapter> logger,
            IConfiguration configuration,
            IDefaultHttpClient httpClient,
            ApplicationPartManager applicationPartManager)
        {
            _logger = logger;
            _settings = configuration.GetSection(nameof(MiraiWebSocketAdapter)).Get<MiraiWebSocketAdapterSettings>();
            _httpClient = httpClient;
            _applicationPartManager = applicationPartManager;

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
        }

        Uri GetWsConnectionUri(string sessionKey)
        {
            return new Uri($"ws://{_settings.WsHost}:{_settings.WsPort}/all?verifyKey={_settings.VerifyKey}&sessionKey={sessionKey}&qq={_settings.QQ}");
        }

        void PrepareMessageHandlers()
        {
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

        async Task DispatchMessageAsync(JsonObject message)
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
                        await DispatchMessageAsync(jsonObject.AsObject());
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
