namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Interface untuk entities yang support soft delete
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
