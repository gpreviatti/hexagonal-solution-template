using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Http;

public partial class BaseHttpService(HttpClient httpClient, ILogger<BaseHttpService> logger)
{
    public HttpClient HttpClient { get; } = httpClient;
    public ILogger<BaseHttpService> Logger { get; } = logger;
    public JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web);
    public Stopwatch Stopwatch { get; } = new();

    public async Task<TResponse?> SendAsync<TRequest, TResponse>(
        string requestUri,
        HttpMethod method,
        TRequest request,
        Dictionary<string, string>? headers = null,
        string contentType = "application/json",
        CancellationToken cancellationToken = default
    ) where TRequest : class where TResponse : class
    {
        Stopwatch.Start();
        SendingRequest(Logger, method, requestUri);

        HttpRequestMessage requestMessage = new(method, requestUri);

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
            RequestFailed(Logger, method, requestUri, response.ReasonPhrase, response.StatusCode, Stopwatch.ElapsedMilliseconds);
            return null;
        }

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<TResponse?>(content, JsonSerializerOptions, cancellationToken);

        RequestCompleted(Logger, method, requestUri, Stopwatch.ElapsedMilliseconds);

        return result;
    }
    
    public async Task<TResponse?> SendAsync<TResponse>(
        string requestUri,
        HttpMethod method,
        CancellationToken cancellationToken,
        Dictionary<string, string>? headers = null
    ) where TResponse : class
    {
        Stopwatch.Start();
        SendingRequest(Logger, method, requestUri);

        var requestMessage = new HttpRequestMessage(method, requestUri);

        if (headers != null) foreach (var header in headers)
            requestMessage.Headers.Add(header.Key, header.Value);

        using var response = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            RequestFailed(Logger, method, requestUri, response.ReasonPhrase, response.StatusCode, Stopwatch.ElapsedMilliseconds);
            return null;
        }
        
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<TResponse?>(content, JsonSerializerOptions, cancellationToken);

        RequestCompleted(Logger, method, requestUri, Stopwatch.ElapsedMilliseconds);
        
        return result;
    }

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "[BaseHttpService] | [SendAsync] | [{Method}] | [{RequestUri}] | Sending request"
    )]
    private static partial void SendingRequest(ILogger logger, HttpMethod method, string requestUri);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "[BaseHttpService] | [SendAsync] | [{Method}] | [{RequestUri}] | [{Message}] | {StatusCode} | Request failed with status in {ElapsedMilliseconds} ms"
    )]
    private static partial void RequestFailed(ILogger logger, HttpMethod method, string requestUri, string? message, HttpStatusCode statusCode, long elapsedMilliseconds);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "[BaseHttpService] | [SendAsync] | [{Method}] | [{RequestUri}] | Request completed in {ElapsedMilliseconds} ms"
    )]
    private static partial void RequestCompleted(ILogger logger, HttpMethod method, string requestUri, long elapsedMilliseconds);
}
