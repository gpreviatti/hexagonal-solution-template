namespace Application.Common.Messages;

public record BaseResponse()
{
    public bool Success { get; set; } = false;

    public string Message { get; set; } = string.Empty;
}

public record BaseResponse<TData>() where TData : class
{
    public string Message { get; set; } = string.Empty;

    public bool Success { get; set; } = false;

    public TData Data { get; set; }

    public void SetBusinessErrorMessage(string className, string methodName, Guid correlationId, string message) => Message = $"[{className}] | [{methodName}] | [{correlationId}] | [BusinessError] | " + message;
    public void SetRequestValidationErrorMessage(string className, string methodName, Guid correlationId, string message) => Message = $"[{className}] | [{methodName}] | [{correlationId}] | [RequestValidationError] " + message;
    public void SetSystemErrorMessage(string className, string methodName, Guid correlationId, string message) => Message = $"[{className}] | [{methodName}] | [{correlationId}] | [SystemError] " + message;

    public void SetData(TData value)
    {
        Data = value;
        Success = true;
    }
}
