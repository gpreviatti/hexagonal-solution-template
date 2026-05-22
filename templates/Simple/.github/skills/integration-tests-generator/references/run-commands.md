# Integration Test Commands

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj

dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~WebApp.Http

dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~WebApp.Grpc

dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~Messaging
```
