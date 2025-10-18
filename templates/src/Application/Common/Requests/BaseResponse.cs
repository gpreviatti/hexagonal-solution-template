namespace Application.Common.Requests;

public record BaseResponse
{
    public BaseResponse(bool success, string? message)
    {
        Success = success;
        Message = message;
    }
    public BaseResponse(bool success) => Success = success;
    public BaseResponse() {}

    public bool Success { get; set; }
    public string? Message { get; set; }
}

public record BaseResponse<TData> : BaseResponse where TData : class
{
    public BaseResponse(){}

    public BaseResponse(TData? data, bool success, string? message) : base(success, message) => Data = data;

    public BaseResponse(TData? data, bool success) : base(success) => Data = data;

    public TData? Data { get; set; }
}

public record BasePaginatedResponse<TData> : BaseResponse<IEnumerable<TData>> where TData : class
{
    public BasePaginatedResponse() {}

    public BasePaginatedResponse(
        int totalPages,
        int totalRecords,
        IEnumerable<TData>? data,
        bool success,
        string? message
    ) : base(data, success, message)
    {
        TotalPages = totalPages;
        TotalRecords = totalRecords;
    }

    public BasePaginatedResponse(
        int totalPages,
        int totalRecords,
        IEnumerable<TData>? data,
        bool success
    ) : base(data, success)
    {
        TotalPages = totalPages;
        TotalRecords = totalRecords;
    }

    public int? TotalPages { get; set; }
    public int? TotalRecords { get; set; }
}
