using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiyakoBot.Http;

namespace MiyakoBot.Adapter
{
    public class MiraiWebSocketAdapter : IAdapter
    {
        readonly ILogger<MiraiWebSocketAdapter> _logger;
        readonly MiraiWebSocketAdapterSettings _settings;
        readonly IHttpClient _httpClient;
        readonly MiraiSession _session;

        public MiraiWebSocketAdapter(ILogger<MiraiWebSocketAdapter> logger,
            IConfiguration configuration,
            IDefaultHttpClient httpClient)
        {
            _logger = logger;
            _settings = configuration.GetSection(nameof(MiraiWebSocketAdapter)).Get<MiraiWebSocketAdapterSettings>();
            _httpClient = httpClient;
            _session = new MiraiSession(httpClient, _settings.VerifyKey);
        }

        Uri GetHttpUri(string path)
        {
            return new Uri($"http://{_settings.HttpHost}:{_settings.HttpPort}/{path}");
        }

        Uri GetWsConnectionUri(string sessionKey)
        {
            return new Uri($"ws://{_settings.WsHost}:{_settings.WsPort}/all?verifyKey={_settings.VerifyKey}&sessionKey={sessionKey}&qq={_settings.QQ}");
        }

        public void Run()
        {
            try
            {
            }
            catch (Exception)
            {
            }
        }
    }
}
