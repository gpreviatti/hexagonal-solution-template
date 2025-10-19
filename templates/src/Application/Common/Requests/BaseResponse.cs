namespace Application.Common.Requests;

public record BaseResponse(bool Success, string? Message = null);

public record BaseResponse<TData>(bool Success, TData? Data = null, string? Message = null) : BaseResponse(Success, Message)
    where TData : class;

public record BasePaginatedResponse<TData>(bool Success, int TotalPages, int TotalRecords, IEnumerable<TData>? Data = null, string? Message = null) :
    BaseResponse<IEnumerable<TData>>(Success, Data, Message);
