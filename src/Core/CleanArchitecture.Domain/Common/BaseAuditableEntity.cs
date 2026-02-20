namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Base class untuk entities yang memerlukan audit trail (Created/Modified tracking)
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
