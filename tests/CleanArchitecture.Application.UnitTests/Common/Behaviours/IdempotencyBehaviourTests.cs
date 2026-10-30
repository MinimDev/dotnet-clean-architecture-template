using System.Text.Json;
using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Common.Behaviours;

public class IdempotencyBehaviourTests
{
    private readonly Mock<IIdempotencyService> _idempotencyServiceMock;
    private readonly Mock<IIdempotencyKeyAccessor> _keyAccessorMock;
    private readonly Mock<ILogger<IdempotencyBehaviour<TestCommand, Result<Guid>>>> _loggerMock;
    private readonly IdempotencyBehaviour<TestCommand, Result<Guid>> _behaviour;

    public IdempotencyBehaviourTests()
    {
        _idempotencyServiceMock = new Mock<IIdempotencyService>();
        _keyAccessorMock = new Mock<IIdempotencyKeyAccessor>();
        _loggerMock = new Mock<ILogger<IdempotencyBehaviour<TestCommand, Result<Guid>>>>();

        _behaviour = new IdempotencyBehaviour<TestCommand, Result<Guid>>(
            _idempotencyServiceMock.Object,
            _keyAccessorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithoutIdempotencyKey_CallsNextDirectlyWithoutTouchingCache()
    {
        // Arrange
        _keyAccessorMock.Setup(k => k.GetIdempotencyKey()).Returns((string?)null);

        var expectedResult = Result<Guid>.Success(Guid.NewGuid());
        var nextCalled = false;
        MessageHandlerDelegate<TestCommand, Result<Guid>> next = (msg, ct) =>
        {
            nextCalled = true;
            return ValueTask.FromResult(expectedResult);
        };

        // Act
        var result = await _behaviour.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.Data.ShouldBe(expectedResult.Data);
        _idempotencyServiceMock.Verify(s => s.GetCachedResponseAsync(It.IsAny<string>()), Times.Never);
        _idempotencyServiceMock.Verify(s => s.CacheResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUncachedKey_ExecutesHandlerAndCachesResult()
    {
        // Arrange
        const string key = "test-key-123";
        _keyAccessorMock.Setup(k => k.GetIdempotencyKey()).Returns(key);
        _idempotencyServiceMock.Setup(s => s.GetCachedResponseAsync(key))
            .ReturnsAsync((IdempotencyResult?)null);

        var expectedResult = Result<Guid>.Success(Guid.NewGuid());
        MessageHandlerDelegate<TestCommand, Result<Guid>> next = (msg, ct) => ValueTask.FromResult(expectedResult);

        // Act
        var result = await _behaviour.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        result.Data.ShouldBe(expectedResult.Data);
        _idempotencyServiceMock.Verify(s => s.GetCachedResponseAsync(key), Times.Once);
        _idempotencyServiceMock.Verify(s => s.CacheResponseAsync(
            key,
            It.Is<string>(json => json.Contains(expectedResult.Data.ToString())),
            It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCachedKey_ReturnsCachedResponseWithoutCallingNext()
    {
        // Arrange
        const string key = "cached-key-456";
        var cachedGuid = Guid.NewGuid();
        var cachedResponse = Result<Guid>.Success(cachedGuid);
        var serialized = JsonSerializer.Serialize(cachedResponse);

        _keyAccessorMock.Setup(k => k.GetIdempotencyKey()).Returns(key);
        _idempotencyServiceMock.Setup(s => s.GetCachedResponseAsync(key))
            .ReturnsAsync(new IdempotencyResult(serialized, DateTime.UtcNow.AddMinutes(-5)));

        var nextCalled = false;
        MessageHandlerDelegate<TestCommand, Result<Guid>> next = (msg, ct) =>
        {
            nextCalled = true;
            return ValueTask.FromResult(Result<Guid>.Failure("Should not be called"));
        };

        // Act
        var result = await _behaviour.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeFalse();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBe(cachedGuid);
        _idempotencyServiceMock.Verify(s => s.CacheResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    public record TestCommand : IRequest<Result<Guid>>;
}
