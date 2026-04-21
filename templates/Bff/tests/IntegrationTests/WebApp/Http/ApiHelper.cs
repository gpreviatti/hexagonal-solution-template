using System.Text;
using System.Text.Json;
using Grpc.Net.Client;

namespace IntegrationTests.WebApp.Http;

public sealed class ApiHelper(HttpClient httpClient)
{
    readonly HttpClient _httpClient = httpClient;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void AddHeaders(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(header.Key))
            {
                _httpClient.DefaultRequestHeaders.Remove(header.Key);
            }

            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string resourceUrl) =>
        await _httpClient.GetAsync(resourceUrl);

    public async Task<HttpResponseMessage> PostAsync(string resourceUrl, dynamic dataClass) =>
        await _httpClient.PostAsync(resourceUrl, SerializeRequest(dataClass));

    public async Task<HttpResponseMessage> PutAsync(string resourceUrl, dynamic data) =>
        await _httpClient.PutAsync(resourceUrl, SerializeRequest(data));

    public async Task<HttpResponseMessage> DeleteAsync(string resourceUrl) =>
        await _httpClient.DeleteAsync(resourceUrl);

    public static StringContent SerializeRequest(dynamic data)
    {
        var json = JsonSerializer.Serialize(data);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<T?> DeSerializeResponse<T>(HttpResponseMessage response)
    {
        if (response.Content == null)
            return default;

        var content = await response.Content.ReadAsStreamAsync();

        return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions);
    }

    public GrpcChannel AsGrpcClientChannel() => GrpcChannel.ForAddress(_httpClient.BaseAddress!, new GrpcChannelOptions
    {
        HttpClient = _httpClient
    });
}
