using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Domain.Events;

/// <summary>
/// Domain event yang dipicu ketika product dibuat
/// </summary>
public class ProductCreatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public DateTime OccurredOn { get; }

    public ProductCreatedEvent(Guid productId, string productName)
    {
        ProductId = productId;
        ProductName = productName;
        OccurredOn = DateTime.UtcNow;
    }
}
