namespace Application.Common.Constants;

public static class DefaultApplicationMessages
{
    public const string StartToExecuteUseCase = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Start to execute use case";
    public const string FinishedExecutingUseCase = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] |  Elapsed time: {ElapsedMilliseconds} ms | Finished executing use case";
    public const string ValidationErrors = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Validation errors: [{errors}]";
}
