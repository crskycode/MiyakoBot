using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MiyakoBot.Http
{
    internal class BaseHttpClient
    {
        readonly HttpClient _client;

        public BaseHttpClient(HttpClient client)
        {
            _client = client;
        }

        public Task<byte[]> GetByteArrayAsync(Uri uri, CancellationToken cancellationToken)
        {
            return _client.GetByteArrayAsync(uri, cancellationToken);
        }

        public async Task<JsonObject> GetJsonAsync(Uri uri, CancellationToken cancellationToken)
        {
            var response = await _client.GetStringAsync(uri, cancellationToken);
            var json = JsonNode.Parse(response) ?? new JsonObject();
            return json.AsObject();
        }

        public async Task<byte[]> PostByteArrayAsync(Uri uri, JsonObject json, CancellationToken cancellationToken)
        {
            var jsonString = json.ToJsonString();
            var content = new StringContent(jsonString);
            var response = await _client.PostAsync(uri, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        public async Task<JsonObject> PostJsonAsync(Uri uri, JsonObject json, CancellationToken cancellationToken)
        {
            var jsonString = json.ToJsonString();
            var content = new StringContent(jsonString);
            var response = await _client.PostAsync(uri, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JsonNode.Parse(responseString) ?? new JsonObject();
            return responseJson.AsObject();
        }
    }
}
