using System.Text.Json.Nodes;

namespace MiyakoBot.Http
{
    public interface IHttpClient
    {
        Task<byte[]> GetByteArrayAsync(Uri uri);
        Task<byte[]> GetByteArrayAsync(Uri uri, CancellationToken cancellationToken);
        Task<JsonObject> GetJsonAsync(Uri uri);
        Task<JsonObject> GetJsonAsync(Uri uri, CancellationToken cancellationToken);
        Task<byte[]> PostByteArrayAsync(Uri uri, JsonObject json);
        Task<byte[]> PostByteArrayAsync(Uri uri, JsonObject json, CancellationToken cancellationToken);
        Task<JsonObject> PostJsonAsync(Uri uri, JsonObject json);
        Task<JsonObject> PostJsonAsync(Uri uri, JsonObject json, CancellationToken cancellationToken);
    }
}
