namespace Hexagonal.Solution.Template.Application.Common.Messages;

public record BaseResponse()
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
};

public record BaseResponse<TResponseData>() where TResponseData : class
{
    private bool _success;

    public string Message { get; set; } = string.Empty;
    public TResponseData? Data { get; set; } = null;
    public bool Success { get => _success; private set => _success = Data is not null; }
};
