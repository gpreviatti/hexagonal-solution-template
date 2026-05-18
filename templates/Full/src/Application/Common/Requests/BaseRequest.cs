using System.ComponentModel.DataAnnotations;
using Application.Common.Attributes;

namespace Application.Common.Requests;

public record BaseRequest([NotDefault] Guid CorrelationId, string User = "", string TimezoneId = "");

public record BasePaginatedRequest(
    [NotDefault] Guid CorrelationId,
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")] int Page = 1,
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")] int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    Dictionary<string, string>? SearchByValues = null,
    string User = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, User, TimezoneId);
