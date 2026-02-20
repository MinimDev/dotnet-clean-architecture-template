using MediatR;

namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Marker interface untuk domain events
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
