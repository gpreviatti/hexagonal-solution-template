using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Http;

public class BaseHttpService(HttpClient httpClient, ILogger<BaseHttpService> logger)
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
        Dictionary<string, string>? headers = null
    ) where TRequest : class where TResponse : class
    {
        _stopwatch.Start();
        _logger.LogDebug("[BaseHttpService] | [SendAsync] | [{Method}] | [{RequestUri}] | Sending request", method, requestUri);

        HttpRequestMessage requestMessage = new(method, requestUri);

        using MemoryStream memoryStream = new();
        await JsonSerializer.SerializeAsync(memoryStream, request, _jsonSerializerOptions, cancellationToken);

        memoryStream.Seek(0, SeekOrigin.Begin);
        using var requestContent = new StreamContent(memoryStream);
        requestMessage.Content = requestContent;

        if (headers != null) foreach (var header in headers)
            requestMessage.Headers.Add(header.Key, header.Value);

        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<TResponse?>(content, _jsonSerializerOptions, cancellationToken);

        _logger.LogDebug("[BaseHttpService] | [SendAsync] | [{Method}] | [{RequestUri}] | Request completed in {ElapsedMilliseconds} ms", method, requestUri, _stopwatch.ElapsedMilliseconds);

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
        _logger.LogDebug("[BaseHttpService] | [SendAsync] | [{Method}] | [{RequestUri}] | Sending request", method, requestUri);

        var requestMessage = new HttpRequestMessage(method, requestUri);

        if (headers != null) foreach (var header in headers)
            requestMessage.Headers.Add(header.Key, header.Value);

        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<TResponse?>(content, _jsonSerializerOptions, cancellationToken);

        _logger.LogDebug("[BaseHttpService] | [SendAsync] | [{Method}] | [{RequestUri}] | Request completed in {ElapsedMilliseconds} ms", method, requestUri, _stopwatch.ElapsedMilliseconds);
        
        return result;
    }
}
