using System.Text;
using System.Text.Json;

namespace IntegrationTests.WebApp.Common;
public sealed class ApiHelper(HttpClient httpClient)
{
    public HttpClient httpClient = httpClient;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void AddHeaders(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            if (httpClient.DefaultRequestHeaders.Contains(header.Key))
            {
                httpClient.DefaultRequestHeaders.Remove(header.Key);
            }

            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string resourceUrl) =>
        await httpClient.GetAsync(resourceUrl);

    public async Task<HttpResponseMessage> PostAsync(string resourceUrl, dynamic dataClass) =>
        await httpClient.PostAsync(resourceUrl, SerializeRequest(dataClass));

    public async Task<HttpResponseMessage> PutAsync(string resourceUrl, dynamic data) =>
        await httpClient.PutAsync(resourceUrl, SerializeRequest(data));

    public async Task<HttpResponseMessage> DeleteAsync(string resourceUrl) =>
        await httpClient.DeleteAsync(resourceUrl);

    public StringContent SerializeRequest(dynamic data)
    {
        var json = JsonSerializer.Serialize(data);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public async Task<T?> DeSerializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions);
    }
}
