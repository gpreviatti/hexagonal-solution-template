namespace Contracts.Common;
public record BaseRequest(Guid CorrelationId);

public record BasePaginatedRequest(
    Guid CorrelationId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    Dictionary<string, string>? SearchByValues = null
) : BaseRequest(CorrelationId);