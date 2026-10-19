using System.Text.Json;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration untuk entity AuditLog
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.PrimaryKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.UserId)
            .HasMaxLength(100);

        builder.Property(a => a.UserName)
            .HasMaxLength(100);

        // Simpan AffectedColumns sebagai JSON string untuk kompatibilitas lintas provider (SQL Server & SQLite)
        builder.Property(a => a.AffectedColumns)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null));

        // Index untuk optimasi pencarian dan filter
        builder.HasIndex(a => a.EntityName);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.UserId);
    }
}
