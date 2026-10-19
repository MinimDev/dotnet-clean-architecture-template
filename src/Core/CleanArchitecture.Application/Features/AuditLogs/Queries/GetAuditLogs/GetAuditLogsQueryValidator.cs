using FluentValidation;

namespace CleanArchitecture.Application.Features.AuditLogs.Queries.GetAuditLogs;

/// <summary>
/// Validator untuk GetAuditLogsQuery
/// </summary>
public class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize must be at least 1")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100");
    }
}
