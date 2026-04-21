using Grpc.Net.Client;

namespace IntegrationTests.WebApp.Grpc.Common;

public sealed class ApiGrpcHelper(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;

    public GrpcChannel AsGrpcClientChannel() => GrpcChannel.ForAddress(HttpClient.BaseAddress!, new GrpcChannelOptions
    {
        HttpClient = HttpClient
    });
}
