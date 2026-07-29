using System.ComponentModel.DataAnnotations;
using Application.Common.Attributes;

namespace Application.Common.Requests;

public record BaseRequest([property: NotDefault] Guid CorrelationId, string User = "", string TimezoneId = "");

public record BasePaginatedRequest(
    Guid CorrelationId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")] int Page = 1,
    [property: Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")] int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    Dictionary<string, string>? SearchByValues = null,
    string User = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, User, TimezoneId);

/// <summary>
/// Represents a request for paginated data with full-text search capabilities.
/// </summary>
/// <param name="CorrelationId"></param>
/// <param name="Page"></param>
/// <param name="PageSize"></param>
/// <param name="SortBy"></param>
/// <param name="SortDescending"></param>
/// <param name="SearchQuery">The full-text search query e.g. "Column1", "Column2 & Column3"</param>
/// <param name="SearchValue">The value to search for within the full-text index e.g. "search term"</param>
/// <param name="SearchLanguage">The language to use for the full-text search</param>
/// <param name="User"></param>
/// <param name="TimezoneId"></param>
public record BaseFullTextSearchPaginatedRequest(
    Guid CorrelationId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")] int Page = 1,
    [property: Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")] int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    string SearchQuery = "",
    string SearchValue = "",
    string SearchLanguage = "english",
    string User = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, User, TimezoneId);
