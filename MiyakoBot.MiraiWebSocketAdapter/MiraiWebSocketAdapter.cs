using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiyakoBot.Http;
using System.Net.WebSockets;
using System.Text;

namespace MiyakoBot.Adapter
{
    public class MiraiWebSocketAdapter : IAdapter
    {
        readonly ILogger<MiraiWebSocketAdapter> _logger;
        readonly MiraiWebSocketAdapterSettings _settings;
        readonly IHttpClient _httpClient;
        readonly MiraiSession _session;
        readonly ClientWebSocket _socket;

        public MiraiWebSocketAdapter(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IDefaultHttpClient httpClient)
        {
            _logger = loggerFactory.CreateLogger<MiraiWebSocketAdapter>();
            _settings = configuration.GetSection(nameof(MiraiWebSocketAdapter)).Get<MiraiWebSocketAdapterSettings>();
            _httpClient = httpClient;

            _session = new MiraiSession(loggerFactory, _httpClient)
            {
                Host = _settings.HttpHost,
                Port = _settings.HttpPort,
                VerifyKey = _settings.VerifyKey,
                QQ = _settings.QQ,
            };

            _socket = new ClientWebSocket();
            _socket.Options.KeepAliveInterval = TimeSpan.FromMinutes(2);
        }

        Uri GetWsConnectionUri(string sessionKey)
        {
            return new Uri($"ws://{_settings.WsHost}:{_settings.WsPort}/all?verifyKey={_settings.VerifyKey}&sessionKey={sessionKey}&qq={_settings.QQ}");
        }

        async Task ReadSocket(CancellationToken cancellationToken)
        {
            var buffer = new List<byte>(ushort.MaxValue);
            var block = new byte[ushort.MaxValue];

            while (!cancellationToken.IsCancellationRequested)
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

                var jsonString = Encoding.UTF8.GetString(buffer.ToArray());

                _logger.LogInformation(jsonString);
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await _session.Open();

            try
            {
                await _session.Bind();

                await _socket.ConnectAsync(GetWsConnectionUri(_session.Key), cancellationToken);

                await ReadSocket(cancellationToken);

                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
            }
            finally
            {
                await _session.Close();
            }
        }
    }
}
