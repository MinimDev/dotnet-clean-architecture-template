using CleanArchitecture.Application.Features.AuditLogs.Queries.GetAuditLogs;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Features.AuditLogs;

public class GetAuditLogsQueryValidatorTests
{
    private readonly GetAuditLogsQueryValidator _validator;

    public GetAuditLogsQueryValidatorTests()
    {
        _validator = new GetAuditLogsQueryValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenParametersAreValid()
    {
        var query = new GetAuditLogsQuery
        {
            PageNumber = 1,
            PageSize = 20,
            EntityName = "Product",
            Action = "Created"
        };

        var result = _validator.Validate(query);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPageNumberIsLessThanOne()
    {
        var query = new GetAuditLogsQuery
        {
            PageNumber = 0,
            PageSize = 10
        };

        var result = _validator.Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAuditLogsQuery.PageNumber));
    }

    [Fact]
    public void Validate_ShouldFail_WhenPageSizeExceedsMaximum()
    {
        var query = new GetAuditLogsQuery
        {
            PageNumber = 1,
            PageSize = 101
        };

        var result = _validator.Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAuditLogsQuery.PageSize));
    }
}
