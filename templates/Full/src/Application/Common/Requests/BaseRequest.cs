using FluentValidation;

namespace Application.Common.Requests;

public record BaseRequest(Guid CorrelationId, string User = "", string TimezoneId = "");

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

public sealed class BasePaginatedRequestValidator : AbstractValidator<BasePaginatedRequest>
{
    public BasePaginatedRequestValidator()
    {
        RuleFor(r => r.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(r => r.PageSize)
            .GreaterThan(0)

            .WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be less than or equal to 100");

        RuleFor(r => r.TimezoneId)
            .NotEmpty()
            .When(r => !string.IsNullOrEmpty(r.User))
            .WithMessage("TimezoneId is required when User is provided");
    }
}
