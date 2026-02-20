namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Base class untuk semua domain entities
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}
