namespace Hexagonal.Solution.Template.Application.Common.Messages;

public record BaseResponse()
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
}

public record BaseResponse<TData>() where TData : class
{
    public string Message { get; set; } = string.Empty;

    public bool Success { get; private set; } = false;
    public TData? Data { get; private set; }

    public void SetData(TData value)
    {
        Data = value;
        Success = true;
    }
}
