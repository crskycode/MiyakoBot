using System.Text.Json.Nodes;

namespace MiyakoBot.Http
{
    public class ProxyedHttpClient : IProxyedHttpClient
    {
        readonly BaseHttpClient _client;

        public ProxyedHttpClient()
        {
            // TODO: Add proxy
            _client = new BaseHttpClient(new HttpClient());
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
