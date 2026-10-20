using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Abstraksi untuk ApplicationDbContext
/// Digunakan di Application layer tanpa coupling ke infrastructure
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<AppSetting> AppSettings { get; }
    DbSet<Notification> Notifications { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
