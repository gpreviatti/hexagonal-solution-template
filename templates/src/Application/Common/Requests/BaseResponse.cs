namespace Application.Common.Requests;

public record BaseResponse(bool Success = false, string Message = "");

public record BaseResponse<TData>(TData Data, bool Success = false, string Message = "") : BaseResponse(Success, Message) where TData : class;

public sealed record BasePaginatedResponse<TData>(
    int TotalPages = 0, int TotalRecords = 0,
    IEnumerable<TData> Data = null,
    bool Success = true, string Message = ""
) : BaseResponse<IEnumerable<TData>>(Data, Success, Message)
    where TData : class;
