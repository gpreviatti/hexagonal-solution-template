using System.Text;
using System.Text.Json;
using Grpc.Net.Client;

namespace IntegrationTests.WebApp.Http.Common;
public sealed class ApiHelper(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void AddHeaders(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(header.Key))
            {
                HttpClient.DefaultRequestHeaders.Remove(header.Key);
            }

            HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string resourceUrl) =>
        await HttpClient.GetAsync(resourceUrl);

    public async Task<HttpResponseMessage> PostAsync(string resourceUrl, dynamic dataClass) =>
        await HttpClient.PostAsync(resourceUrl, SerializeRequest(dataClass));

    public async Task<HttpResponseMessage> PutAsync(string resourceUrl, dynamic data) =>
        await HttpClient.PutAsync(resourceUrl, SerializeRequest(data));

    public async Task<HttpResponseMessage> DeleteAsync(string resourceUrl) =>
        await HttpClient.DeleteAsync(resourceUrl);

    public static StringContent SerializeRequest(dynamic data)
    {
        var json = JsonSerializer.Serialize(data);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<T?> DeSerializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStreamAsync();

        return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions);
    }
}
