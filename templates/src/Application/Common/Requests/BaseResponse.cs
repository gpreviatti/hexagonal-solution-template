namespace Application.Common.Requests;

public record BaseResponse
{
    public BaseResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }
    public BaseResponse() {}

    public bool Success { get; set; }
    public string Message { get; set; }
}

public record BaseResponse<TData> : BaseResponse where TData : class
{
    public BaseResponse(){}

    public BaseResponse(TData data, bool success = false, string message = "") : base(success, message)
    {
        Data = data;
    }

    public TData Data { get; set; }
}

public sealed record BasePaginatedResponse<TData> : BaseResponse<IEnumerable<TData>> where TData : class
{
    public BasePaginatedResponse() {}

    public BasePaginatedResponse(
        int totalPages = 0, int totalRecords = 0,
        IEnumerable<TData> data = null,
        bool success = true, string message = ""
    ) : base(data, success, message)
    {
        TotalPages = totalPages;
        TotalRecords = totalRecords;
    }

    public int TotalPages { get; set; } = 0;
    public int TotalRecords { get; set; } = 0;
}
