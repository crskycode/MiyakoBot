using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;
using System.Net.Http;
using System.Net;

namespace MiyakoBot.Http
{
    public class ProxyedHttpClient : IProxyedHttpClient
    {
        readonly IConfiguration _configuration;
        readonly BaseHttpClient _client;

        public ProxyedHttpClient(IConfiguration configuration)
        {
            _configuration = configuration.GetSection(nameof(ProxyedHttpClient));

            var proxyUrl = _configuration["Proxy"];

            HttpClient httpClient;

            if (!string.IsNullOrEmpty(proxyUrl))
            {
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxyUrl),
                    UseProxy = true
                };

                httpClient = new HttpClient(httpClientHandler);
            }
            else
            {
                httpClient = new HttpClient();
            }

            _client = new BaseHttpClient(httpClient);
        }

        public Task<byte[]> GetByteArrayAsync(Uri uri)
        {
            return _client.GetByteArrayAsync(uri, CancellationToken.None);
        }

        public Task<byte[]> GetByteArrayAsync(Uri uri, CancellationToken cancellationToken)
        {
            return _client.GetByteArrayAsync(uri, cancellationToken);
        }

        public Task<JsonObject> GetJsonAsync(Uri uri)
        {
            return _client.GetJsonAsync(uri, CancellationToken.None);
        }

        public Task<JsonObject> GetJsonAsync(Uri uri, CancellationToken cancellationToken)
        {
            return _client.GetJsonAsync(uri, cancellationToken);
        }

        public Task<byte[]> PostByteArrayAsync(Uri uri, JsonObject json)
        {
            return _client.PostByteArrayAsync(uri, json, CancellationToken.None);
        }

        public Task<byte[]> PostByteArrayAsync(Uri uri, JsonObject json, CancellationToken cancellationToken)
        {
            return _client.PostByteArrayAsync(uri, json, cancellationToken);
        }

        public Task<JsonObject> PostJsonAsync(Uri uri, JsonObject json)
        {
            return _client.PostJsonAsync(uri, json, CancellationToken.None);
        }

        public Task<JsonObject> PostJsonAsync(Uri uri, JsonObject json, CancellationToken cancellationToken)
        {
            return _client.PostJsonAsync(uri, json, cancellationToken);
        }
    }
}
