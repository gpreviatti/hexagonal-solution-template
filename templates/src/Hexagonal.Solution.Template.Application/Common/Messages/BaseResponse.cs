namespace Hexagonal.Solution.Template.Application.Common.Messages;

public record BaseResponse()
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
}

public record BaseResponse<TData>() where TData : class
{
    public string Message { get; private set; } = string.Empty;

    public bool Success { get; private set; } = false;
    public TData Data { get; private set; }

    public void SetBusinessErrorMessage(string message) => Message = message;
    public void SetRequestValidationErrorMessage(string message) => Message = "[RequestValidationError] " + message;
    public void SetSystemErrorMessage(string message) => Message = "[SystemError] " + message;

    public void SetData(TData value)
    {
        Data = value;
        Success = true;
    }
}
