using Grpc.Net.Client;

namespace Infrastructure.Grpc;

public abstract class BaseGrpcService(string baseAddress)
{
    protected GrpcChannel Channel { get; } = GrpcChannel.ForAddress(baseAddress);
}

