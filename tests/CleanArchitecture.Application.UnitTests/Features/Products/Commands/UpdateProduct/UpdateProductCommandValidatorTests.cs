using CleanArchitecture.Application.Features.Products.Commands.UpdateProduct;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator;

    public UpdateProductCommandValidatorTests()
    {
        _validator = new UpdateProductCommandValidator();
    }

    [Fact]
    public void ShouldBeValid_WhenCommandIsValid()
    {
        // Arrange
        var command = new UpdateProductCommandById(Guid.NewGuid())
        {
            Name = "Valid Name",
            Description = "Valid Description",
            Price = 100,
            Stock = 10
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeInvalid_WhenNameIsEmpty()
    {
        // Arrange
        var command = new UpdateProductCommandById(Guid.NewGuid())
        {
            Name = string.Empty,
            Description = "Valid Description",
            Price = 100,
            Stock = 10
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateProductCommandById.Name));
    }

    [Fact]
    public void ShouldBeInvalid_WhenNameIsTooLong()
    {
        // Arrange
        var command = new UpdateProductCommandById(Guid.NewGuid())
        {
            Name = new string('a', 201), // 201 chars
            Description = "Valid Description",
            Price = 100,
            Stock = 10
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateProductCommandById.Name));
    }

    [Fact]
    public void ShouldBeInvalid_WhenPriceIsNegative()
    {
        // Arrange
        var command = new UpdateProductCommandById(Guid.NewGuid())
        {
            Name = "Valid Name",
            Description = "Valid Description",
            Price = -1,
            Stock = 10
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateProductCommandById.Price));
    }

    [Fact]
    public void ShouldBeInvalid_WhenStockIsNegative()
    {
        // Arrange
        var command = new UpdateProductCommandById(Guid.NewGuid())
        {
            Name = "Valid Name",
            Description = "Valid Description",
            Price = 100,
            Stock = -1
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateProductCommandById.Stock));
    }
}
