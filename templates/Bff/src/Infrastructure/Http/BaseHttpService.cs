using System.Net;
using System.Text.Json;
using Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Http;

public class BaseHttpService(HttpClient httpClient, ILogger<BaseHttpService> logger, int httpProtocolVersion = 2)
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

    public async Task<TResponse?> SendAsync<TRequest, TResponse>(
        string requestUri,
        HttpMethod httpMethod,
        TRequest request,
        Dictionary<string, string>? headers = null,
        string contentType = "application/json",
        CancellationToken cancellationToken = default
    ) where TRequest : class where TResponse : class
    {
        Logs.StartingOperation(Logger);

        HttpRequestMessage requestMessage = new(httpMethod, requestUri)
        {
            Version = GetHttpVersion(HttpProtocolVersion),
            VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
        };

        using MemoryStream memoryStream = new();
        await JsonSerializer.SerializeAsync(memoryStream, request, JsonSerializerOptions, cancellationToken);

        memoryStream.Seek(0, SeekOrigin.Begin);
        using var requestContent = new StreamContent(memoryStream);
        requestContent.Headers.ContentType = new(contentType);
        requestMessage.Content = requestContent;

        if (headers != null) foreach (var header in headers)
            requestMessage.Headers.Add(header.Key, header.Value);

        using var response = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            Logs.FailedOperation(Logger, $"{httpMethod} {requestUri} failed: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<TResponse?>(content, JsonSerializerOptions, cancellationToken);

        Logs.FinishedOperation(Logger);

        return result;
    }
    
    public async Task<TResponse?> SendAsync<TResponse>(
        string requestUri,
        HttpMethod httpMethod,
        CancellationToken cancellationToken,
        Dictionary<string, string>? headers = null
    ) where TResponse : class
    {
        Logs.StartingOperation(Logger);

        var requestMessage = new HttpRequestMessage(httpMethod, requestUri)
        {
            Version = GetHttpVersion(HttpProtocolVersion),
            VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
        };

        if (headers != null) foreach (var header in headers)
            requestMessage.Headers.Add(header.Key, header.Value);

        using var response = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            Logs.FailedOperation(Logger, $"{httpMethod} {requestUri} failed: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }
        
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<TResponse?>(content, JsonSerializerOptions, cancellationToken);

        Logs.FinishedOperation(Logger);
        
        return result;
    }
}
