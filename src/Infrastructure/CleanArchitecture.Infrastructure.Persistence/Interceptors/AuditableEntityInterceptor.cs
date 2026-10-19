using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitecture.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core Interceptor untuk auto-populate audit fields (CreatedAt, ModifiedAt, dll)
/// dan mencatat riwayat perubahan (Audit Trail) ke tabel AuditLogs.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AuditableEntityInterceptor(ICurrentUserService currentUserService, IDateTime dateTime)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        CreateAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        CreateAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = _currentUserService.UserName ?? "System";
                entry.Entity.CreatedAt = _dateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Entity.ModifiedBy = _currentUserService.UserName ?? "System";
                entry.Entity.ModifiedAt = _dateTime.UtcNow;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = _dateTime.UtcNow;
                entry.Entity.DeletedBy = _currentUserService.UserName ?? "System";

                foreach (var ownedEntry in entry.References.Where(r => r.TargetEntry != null && r.TargetEntry.Metadata.IsOwned()))
                {
                    ownedEntry.TargetEntry!.State = EntityState.Unchanged;
                }
            }
        }
    }

    private void CreateAuditLogs(DbContext? context)
    {
        if (context == null) return;

        var auditEntries = new List<AuditLog>();
        // Snapshot entries to avoid collection modified exceptions when adding AuditLogs
        var entries = context.ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            // Jangan pernah mengaudit tabel AuditLog sendiri atau owned types
            if (entry.Entity is AuditLog || entry.Entity is not BaseEntity || entry.Metadata.IsOwned())
            {
                continue;
            }

            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            // Tentukan jenis aksi (Created, Updated, Deleted)
            string action;
            if (entry.Entity is ISoftDeletable softDeletable &&
                softDeletable.IsDeleted &&
                entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified &&
                (entry.Property(nameof(ISoftDeletable.IsDeleted)).OriginalValue as bool?) != true)
            {
                action = "Deleted";
            }
            else if (entry.State == EntityState.Added)
            {
                action = "Created";
            }
            else if (entry.State == EntityState.Deleted)
            {
                action = "Deleted";
            }
            else if (entry.State == EntityState.Modified)
            {
                action = "Updated";
            }
            else
            {
                continue;
            }

            // Dapatkan Primary Key
            var keyProps = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();
            var primaryKey = keyProps.Count > 0
                ? string.Join(",", keyProps.Select(p => p.CurrentValue ?? p.OriginalValue))
                : string.Empty;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var affectedColumns = new List<string>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                {
                    continue;
                }

                string propertyName = property.Metadata.Name;

                switch (action)
                {
                    case "Created":
                        newValues[propertyName] = property.CurrentValue;
                        affectedColumns.Add(propertyName);
                        break;

                    case "Deleted":
                        oldValues[propertyName] = property.OriginalValue;
                        affectedColumns.Add(propertyName);
                        break;

                    case "Updated":
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                            affectedColumns.Add(propertyName);
                        }
                        break;
                }
            }

            // Jika update tapi tidak ada kolom yang berubah, lewati
            if (action == "Updated" && affectedColumns.Count == 0)
            {
                continue;
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entry.Metadata.ClrType.Name,
                Action = action,
                PrimaryKey = primaryKey,
                OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues, _jsonSerializerOptions) : null,
                NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues, _jsonSerializerOptions) : null,
                AffectedColumns = affectedColumns.Count > 0 ? affectedColumns.ToArray() : null,
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName ?? "System",
                Timestamp = _dateTime.UtcNow
            };

            auditEntries.Add(auditLog);
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
