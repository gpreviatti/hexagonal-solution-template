using System.Diagnostics;
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
    private static readonly Action<ILogger, HttpMethod, string, HttpStatusCode, string?, Exception?> HttpCallFailedLog =
        LoggerMessage.Define<HttpMethod, string, HttpStatusCode, string?>(
            LogLevel.Warning,
            new EventId(2001, nameof(HttpCallFailedLog)),
            "HTTP call failed: {Method} {Uri} - {StatusCode} {ReasonPhrase}");

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
        using var activity = DefaultConfigurations.ActivitySource.StartActivity(requestUri, ActivityKind.Client);

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
        activity?.SetTag("http.status_code", (int) response.StatusCode);
        activity?.SetTag("http.response_content_length", response.Content.Headers.ContentLength ?? 0);
        activity?.SetTag("http.response_content_type", response.Content.Headers.ContentType?.ToString() ?? "unknown");
        activity?.SetTag("http.response_headers", string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(";", h.Value)}")));

        if (!response.IsSuccessStatusCode)
        {
            HttpCallFailedLog(Logger, httpMethod, requestUri, response.StatusCode, response.ReasonPhrase, null);

            return null;
        }

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<TResponse?>(content, JsonSerializerOptions, cancellationToken);
    }
}
