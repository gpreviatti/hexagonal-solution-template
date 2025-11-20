namespace Application.Common.Requests;

public record BaseResponse
{
    public BaseResponse() { }

    public BaseResponse(bool success, string? message = null)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; set; }
    public string? Message { get; set; }
}

public record BaseResponse<TData> : BaseResponse where TData : class
{
    public BaseResponse() { }

    public BaseResponse(bool success, TData? data = null, string? message = null) : base(success, message)
    {
        Data = data;
    }

    public TData? Data { get; set; }

}

public sealed record BasePaginatedResponse<TData> : BaseResponse<IEnumerable<TData>>
{
    public BasePaginatedResponse() { }

    public BasePaginatedResponse(
        bool success, int totalPages, int totalRecords,
        IEnumerable<TData>? data = null, string? message = null
    ) : base(success, data, message)
    {
        TotalPages = totalPages;
        TotalRecords = totalRecords;
    }

    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

}
