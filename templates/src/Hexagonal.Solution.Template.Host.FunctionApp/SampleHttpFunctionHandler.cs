using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Serilog;


namespace Hexagonal.Solution.Template.Host.FunctionApp;

public class SampleHttpFunctionHandler
{
    private readonly ILogger _logger;

    public SampleHttpFunctionHandler(ILogger logger)
    {
        _logger = logger;
    }

    [Function("SampleHttpFunctionHandler")]
    public string Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData request)
    {
        _logger.Information("C# HTTP trigger with Kusto Input Binding function processed a request.");
        return "Hello world";
    }
}
