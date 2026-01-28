using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Http;

public partial class BaseHttpService(HttpClient httpClient, ILogger<BaseHttpService> logger)
{
    protected readonly HttpClient _httpClient = httpClient;
    protected readonly ILogger<BaseHttpService> _logger = logger;
    protected readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);
    protected readonly Stopwatch _stopwatch = new();

    public async Task<TResponse?> SendAsync<TRequest, TResponse>(
        string requestUri,
        HttpMethod method,
        CancellationToken cancellationToken,
        TRequest request,
        Dictionary<string, string>? headers = null,
        string contentType = "application/json"
    ) where TRequest : class where TResponse : class
    {
        _stopwatch.Start();
        SendingRequest(_logger, method, requestUri);

        HttpRequestMessage requestMessage = new(method, requestUri);

        using MemoryStream memoryStream = new();
        await JsonSerializer.SerializeAsync(memoryStream, request, _jsonSerializerOptions, cancellationToken);

        memoryStream.Seek(0, SeekOrigin.Begin);
        using var requestContent = new StreamContent(memoryStream);
        requestContent.Headers.ContentType = new(contentType);
        requestMessage.Content = requestContent;

        if (headers != null) foreach (var header in headers)
            requestMessage.Headers.Add(header.Key, header.Value);

        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            RequestFailed(_logger, method, requestUri, response.ReasonPhrase, response.StatusCode, _stopwatch.ElapsedMilliseconds);
            return null;
        }

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<TResponse?>(content, _jsonSerializerOptions, cancellationToken);

        RequestCompleted(_logger, method, requestUri, _stopwatch.ElapsedMilliseconds);

        return result;
    }
    
    public async Task<TResponse?> SendAsync<TResponse>(
        string requestUri,
        HttpMethod method,
        CancellationToken cancellationToken,
        Dictionary<string, string>? headers = null
    ) where TResponse : class
    {
        _stopwatch.Start();
        SendingRequest(_logger, method, requestUri);

        var requestMessage = new HttpRequestMessage(method, requestUri);

        if (headers != null) foreach (var header in headers)
            requestMessage.Headers.Add(header.Key, header.Value);

        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            RequestFailed(_logger, method, requestUri, response.ReasonPhrase, response.StatusCode, _stopwatch.ElapsedMilliseconds);
            return null;
        }
        
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<TResponse?>(content, _jsonSerializerOptions, cancellationToken);

        RequestCompleted(_logger, method, requestUri, _stopwatch.ElapsedMilliseconds);
        
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
