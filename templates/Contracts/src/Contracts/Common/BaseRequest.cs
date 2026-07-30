namespace Contracts.Common;

/// <summary>
/// Base request structure
/// </summary>
/// <param name="CorrelationId">The unique identifier for correlating requests</param>
/// <param name="User">The user making the request</param>
/// <param name="TimezoneId">The timezone identifier for the request</param>
public record BaseRequest(Guid CorrelationId, string User = "", string TimezoneId = "");

/// <summary>
/// Base paginated request structure
/// </summary>
/// <param name="CorrelationId">The unique identifier for correlating requests</param>
/// <param name="Page">The page number to retrieve</param>
/// <param name="PageSize">The number of items per page</param>
/// <param name="SortBy">The field to sort by</param>
/// <param name="SortDescending">Indicates whether the sorting is in descending order</param>
/// <param name="SearchByValues">A dictionary of search criteria</param>
/// <param name="User">The user making the request</param>
/// <param name="TimezoneId">The timezone identifier for the request</param>
public record BasePaginatedRequest(
    Guid CorrelationId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    Dictionary<string, string>? SearchByValues = null,
    string User = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, User, TimezoneId);

/// <summary>
/// Represents a request for paginated data with full-text search capabilities.
/// </summary>
/// <param name="CorrelationId">The unique identifier for correlating requests</param>
/// <param name="Page">The page number to retrieve</param>
/// <param name="PageSize">The number of items per page</param>
/// <param name="SortBy">The field to sort by</param>
/// <param name="SortDescending">Indicates whether the sorting is in descending order</param>
/// <param name="SearchValue">The value to search for within the full-text index e.g. "search term"</param>
/// <param name="User">The user making the request</param>
/// <param name="TimezoneId">The timezone identifier for the request</param>
public record BaseFullTextSearchPaginatedRequest(
    Guid CorrelationId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    string SearchValue = "",
    string User = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, User, TimezoneId);
