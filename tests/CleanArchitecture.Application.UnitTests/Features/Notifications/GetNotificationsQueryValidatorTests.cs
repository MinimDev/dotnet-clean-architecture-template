using CleanArchitecture.Application.Features.Notifications.Queries.GetNotifications;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Features.Notifications;

public class GetNotificationsQueryValidatorTests
{
    private readonly GetNotificationsQueryValidator _validator;

    public GetNotificationsQueryValidatorTests()
    {
        _validator = new GetNotificationsQueryValidator();
    }

    [Fact]
    public void ShouldBeValid_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetNotificationsQuery(PageNumber: 1, PageSize: 10, UnreadOnly: null);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeInvalid_WhenPageNumberIsZeroOrNegative()
    {
        // Arrange
        var query = new GetNotificationsQuery(PageNumber: 0, PageSize: 10);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetNotificationsQuery.PageNumber));
    }

    [Fact]
    public void ShouldBeInvalid_WhenPageSizeIsZeroOrNegative()
    {
        // Arrange
        var query = new GetNotificationsQuery(PageNumber: 1, PageSize: 0);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetNotificationsQuery.PageSize));
    }

    [Fact]
    public void ShouldBeInvalid_WhenPageSizeExceedsMaximum()
    {
        // Arrange
        var query = new GetNotificationsQuery(PageNumber: 1, PageSize: 101);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetNotificationsQuery.PageSize));
    }
}
