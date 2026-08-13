namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Base class untuk semua domain entities
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}
