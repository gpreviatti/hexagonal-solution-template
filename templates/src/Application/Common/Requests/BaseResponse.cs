namespace Application.Common.Requests;

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

    public void SetMessage(string message) => Message = message;

    public void SetData(TData value)
    {
        Data = value;
        Success = true;
    }
}
