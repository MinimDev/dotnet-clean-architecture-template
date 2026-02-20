using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Features.Products.Commands.UpdateProduct;
using CleanArchitecture.Domain.Entities;
using Shouldly;
using Moq;

namespace CleanArchitecture.Application.UnitTests.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Product>> _mockRepository;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IRepository<Product>>();

        _mockUnitOfWork.Setup(uow => uow.Repository<Product>())
            .Returns(_mockRepository.Object);

        _handler = new UpdateProductCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommandById(productId)
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 200,
            Stock = 20
        };

        var existingProduct = Product.Create("Original Name", "Original Description", 100, 10);
        // We can't set Id directly as it's private set, but usually it's set by EF Core or constructor if needed.
        // For unit testing purposes, if we need specific ID matching, we might need a way to set it or rely on repository returning *this instance* for *that ID*.
        // Since we mock GetByIdAsync, we control what is returned regardless of ID.

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        existingProduct.Name.ShouldBe(command.Name);
        existingProduct.Description.ShouldBe(command.Description);
        existingProduct.Price.ShouldBe(command.Price);
        existingProduct.Stock.ShouldBe(command.Stock);

        _mockRepository.Verify(r => r.Update(existingProduct), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommandById(productId)
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 200,
            Stock = 20
        };

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain($"Product with ID {productId} not found");

        _mockRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
