using MiyakoBot.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MiyakoBot.Adapter
{
    internal class MiraiSession
    {
        public string Key { get; set; } = string.Empty;

        readonly ILogger<MiraiSession> _logger;
        readonly IHttpClient _httpClient;
        readonly MiraiSessionSettings _settings;

        public MiraiSession(ILogger<MiraiSession> logger, IHttpClient httpClient, MiraiSessionSettings settings)
        {
            _logger = logger;
            _httpClient = httpClient;
            _settings = settings;
        }

        Uri GetHttpUri(string path)
        {
            return new Uri($"http://{_settings.Host}:{_settings.Port}/{path}");
        }

        public async Task Open()
        {
            var json = new JsonObject
            {
                ["verifyKey"] = _settings.VerifyKey
            };

            var response = await _httpClient.PostJsonAsync(GetHttpUri("verify"), json);

            var code = (int?)response["code"];

            if (code != 0)
            {
                throw new Exception("Failed to create session.");
            }

            var session = (string?)response["session"];

            if (string.IsNullOrEmpty(session))
            {
                throw new Exception("Empty session.");
            }

            Key = session;

            _logger.LogInformation("Open session successfully.");
        }

        public async Task Bind()
        {
            var json = new JsonObject
            {
                ["sessionKey"] = Key,
                ["qq"] = _settings.QQ
            };

            var response = await _httpClient.PostJsonAsync(GetHttpUri("bind"), json);

            var code = (int?)response["code"];

            if (code != 0)
            {
                throw new Exception("Failed to bind session.");
            }

            _logger.LogInformation("Bind session successfully.");
        }

        public async Task Close()
        {
            var json = new JsonObject
            {
                ["sessionKey"] = Key,
                ["qq"] = _settings.QQ
            };

            var response = await _httpClient.PostJsonAsync(GetHttpUri("release"), json);

            var code = (int?)response["code"];

            if (code != 0)
            {
                throw new Exception("Failed to close session.");
            }

            _logger.LogInformation("Close session successfully.");
        }
    }
}
