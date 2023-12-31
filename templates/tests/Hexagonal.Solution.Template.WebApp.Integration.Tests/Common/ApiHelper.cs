using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;
public class ApiHelper(HttpClient httpClient)
{
    public HttpClient httpClient = httpClient;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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

    public async Task<T> DeSerializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions);
    }
}