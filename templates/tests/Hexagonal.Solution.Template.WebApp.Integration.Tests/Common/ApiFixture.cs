using System.Text;
using System.Text.Json;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;
public class ApiFixture
{
    protected static string _hostApi;
    protected readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiFixture(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _hostApi = httpClient.BaseAddress.Host;
    }

    public async Task<HttpResponseMessage> GetAsync(string url) =>
        await _httpClient.GetAsync(_hostApi + url);

    public async Task<HttpResponseMessage> PostAsync(string url, dynamic dataClass) =>
        await _httpClient.PostAsync(_hostApi + url, SerializeRequest(dataClass));

    public async Task<HttpResponseMessage> PutAsync(string url, dynamic data) =>
        await _httpClient.PutAsync(_hostApi + url, SerializeRequest(data));

    public async Task<HttpResponseMessage> DeleteAsync(string url) =>
        await _httpClient.DeleteAsync(_hostApi + url);

    public StringContent SerializeRequest(dynamic data)
    {
        var json = JsonSerializer.Serialize(data);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions);
    }
}