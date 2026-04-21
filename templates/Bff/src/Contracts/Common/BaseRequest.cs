namespace Contracts.Common;

/// <summary>
/// Base request structure
/// </summary>
/// <param name="CorrelationId">The unique identifier for correlating requests</param>
public record BaseRequest(Guid CorrelationId);

/// <summary>
/// Base paginated request structure
/// </summary>
/// <param name="CorrelationId">The unique identifier for correlating requests</param>
/// <param name="Page">The page number to retrieve</param>
/// <param name="PageSize">The number of items per page</param>
/// <param name="SortBy">The field to sort by</param>
/// <param name="SortDescending">Indicates whether the sorting is in descending order</param>
/// <param name="SearchByValues">A dictionary of search criteria</param>
public record BasePaginatedRequest(
    Guid CorrelationId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    Dictionary<string, string>? SearchByValues = null
) : BaseRequest(CorrelationId);
