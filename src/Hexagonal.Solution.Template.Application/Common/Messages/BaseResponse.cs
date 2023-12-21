namespace Hexagonal.Solution.Template.Application.Common.Messages;

public record BaseResponse()
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
};

public record BaseResponse<TResponseData>() where TResponseData : class
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public TResponseData? Data { get; set; } = null;
};
