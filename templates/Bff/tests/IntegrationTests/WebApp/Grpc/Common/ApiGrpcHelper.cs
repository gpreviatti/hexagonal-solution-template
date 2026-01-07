using Grpc.Net.Client;

namespace IntegrationTests.WebApp.Grpc.Common;
public sealed class ApiGrpcHelper(HttpClient httpClient)
{
    public HttpClient httpClient = httpClient;

    public GrpcChannel AsGrpcClientChannel() => GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
    {
        HttpClient = httpClient
    });
}
