using CleanArchitecture.Application.Features.AppSettings.Commands.UpdateAppSetting;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Features.AppSettings;

public class UpdateAppSettingCommandValidatorTests
{
    private readonly UpdateAppSettingCommandValidator _validator;

    public UpdateAppSettingCommandValidatorTests()
    {
        _validator = new UpdateAppSettingCommandValidator();
    }

    [Fact]
    public void ShouldBeValid_WhenCommandIsValid()
    {
        // Arrange
        var command = new UpdateAppSettingCommand("Features.ProductExport.Enabled", "true");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeInvalid_WhenKeyIsEmpty()
    {
        // Arrange
        var command = new UpdateAppSettingCommand(string.Empty, "some_value");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateAppSettingCommand.Key));
    }

    [Fact]
    public void ShouldBeInvalid_WhenKeyExceedsMaxLength()
    {
        // Arrange
        var longKey = new string('k', 201);
        var command = new UpdateAppSettingCommand(longKey, "some_value");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateAppSettingCommand.Key));
    }

    [Fact]
    public void ShouldBeInvalid_WhenValueIsNull()
    {
        // Arrange
        var command = new UpdateAppSettingCommand("Features.ProductExport.Enabled", null!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateAppSettingCommand.Value));
    }

    [Fact]
    public void ShouldBeInvalid_WhenValueExceedsMaxLength()
    {
        // Arrange
        var longValue = new string('v', 2001);
        var command = new UpdateAppSettingCommand("General.ApplicationName", longValue);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateAppSettingCommand.Value));
    }
}
