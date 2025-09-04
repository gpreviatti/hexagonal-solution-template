namespace Application.Common.Requests;

public record BaseResponse()
{
    public bool Success { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public void SetSuccess(bool? value) => Success = value ?? false;
    public void SetMessage(string message, bool? success = null)
    {
        Message = message ?? string.Empty;
        SetSuccess(success);
    }
}

public record BaseResponse<TData>() : BaseResponse() where TData : class 
{
    public TData Data { get; set; }

    public virtual void SetData(TData value, string message = null, bool? success = null)
    {
        Data = value;
        SetMessage(message);
        SetSuccess(success);
    }
}

public sealed record BasePaginatedResponse<TData>() : BaseResponse<IEnumerable<TData>>()
    where TData : class
{
    public int TotalPages { get; set; } = 0;
    public int TotalRecords { get; set; } = 0;

    public void SetData(
        int totalRecords,
        int pageSize,
        IEnumerable<TData> data,
        string message = null,
        bool? success = null
    )
    {
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        SetData(data, message, success);
    }
};
