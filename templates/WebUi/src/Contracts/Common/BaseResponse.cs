namespace Contracts.Common;

/// <summary>
/// Base response structure
/// </summary>
public record BaseResponse
{
    /// <summary>
    /// Base response structure
    /// </summary>
    public BaseResponse() { }

    /// <summary>
    /// Base response structure
    /// </summary>
    /// <param name="success">Indicates whether the response is successful</param>
    /// <param name="message">Optional message providing additional information</param>
    public BaseResponse(bool success, string? message = null)
    {
        Success = success;
        Message = message;
    }

    /// <summary>
    /// Indicates whether the response is successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Optional message providing additional information
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Generic base response structure with data
/// </summary>
/// <typeparam name="TData">The type of the data included in the response</typeparam>
public record BaseResponse<TData> : BaseResponse where TData : class
{
    /// <summary>
    /// Generic base response structure with data
    /// </summary>
    public BaseResponse() { }

    /// <summary>
    /// Generic base response structure with data
    /// </summary>
    /// <param name="success">Indicates whether the response is successful</param>
    /// <param name="data">The data included in the response</param>
    /// <param name="message">Optional message providing additional information</param>
    public BaseResponse(bool success, TData? data = null, string? message = null) : base(success, message) => Data = data;

    /// <summary>
    /// The data included in the response
    /// </summary>
    public TData? Data { get; set; }

}

/// <summary>
/// Generic base paginated response structure with data
/// </summary>
/// <typeparam name="TData"></typeparam>
public sealed record BasePaginatedResponse<TData> : BaseResponse<IEnumerable<TData>>
{
    /// <summary>
    /// Generic base paginated response structure with data
    /// </summary>
    public BasePaginatedResponse() { }

    /// <summary>
    /// Generic base paginated response structure with data
    /// </summary>
    /// <param name="success">Indicates whether the response is successful</param>
    /// <param name="totalPages">The total number of pages available</param>
    /// <param name="totalRecords">The total number of records available</param>
    /// <param name="data">The data included in the response</param>
    /// <param name="message">Optional message providing additional information</param>
    public BasePaginatedResponse(
        bool success, int totalPages, int totalRecords,
        IEnumerable<TData>? data = null, string? message = null
    ) : base(success, data, message)
    {
        TotalPages = totalPages;
        TotalRecords = totalRecords;
    }

    /// <summary>
    /// The total number of pages available
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// The total number of records available
    /// </summary>
    public int TotalRecords { get; set; }

}
