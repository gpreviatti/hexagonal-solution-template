using System.Net;
using System.Text.Json;
using Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Http;

public interface IBaseHttpService
{
    Task<TResponse?> SendAsync<TResponse>(
        string requestUri,
        HttpMethod httpMethod,
        CancellationToken cancellationToken,
        Dictionary<string, string>? headers = null
    ) where TResponse : class;
}

public sealed class BaseHttpService(HttpClient httpClient, ILogger<BaseHttpService> logger, int httpProtocolVersion = 2) : IBaseHttpService
{
    public HttpClient HttpClient { get; } = httpClient;

    public ILogger<BaseHttpService> Logger { get; } = logger;

    public int HttpProtocolVersion { get; } = httpProtocolVersion;

    public JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    private static Version GetHttpVersion(int httpVersion) => httpVersion switch
    {
        1 => HttpVersion.Version11,
        3 => HttpVersion.Version30,
        2 or _ => HttpVersion.Version20
    };

    public async Task<TResponse?> SendAsync<TResponse>(
        string requestUri,
        HttpMethod httpMethod,
        CancellationToken cancellationToken,
        Dictionary<string, string>? headers = null
    ) where TResponse : class
    {
        var requestMessage = new HttpRequestMessage(httpMethod, requestUri)
        {
            Version = GetHttpVersion(HttpProtocolVersion),
            VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
        };

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        using var response = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            Logs.FailedOperation(Logger, $"HTTP call failed: {httpMethod} {requestUri} - {response.StatusCode} {response.ReasonPhrase}");
            return null;
        }

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<TResponse?>(content, JsonSerializerOptions, cancellationToken);
    }
}
