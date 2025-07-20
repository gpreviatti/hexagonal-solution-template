namespace Application.Common.Messages;

public static class DefaultApplicationMessages
{
    public const string DefaultApplicationMessage = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | ";
    public const string StartToExecuteUseCase = DefaultApplicationMessage + "Start to execute use case";
    public const string FinishedExecutingUseCase = DefaultApplicationMessage + "Finished executing use case with success";
    public const string ValidationErrors = DefaultApplicationMessage + "Validation errors: [{errors}]";
}
