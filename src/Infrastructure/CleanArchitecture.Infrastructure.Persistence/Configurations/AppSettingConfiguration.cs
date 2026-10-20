using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration untuk entity AppSetting
/// </summary>
public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.DataType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Group)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.HasIndex(s => s.Group);

        // Seed default initial application settings
        var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                Key = "General.ApplicationName",
                Value = "OneVision Clean Architecture",
                DataType = "string",
                Description = "Application public display title",
                Group = "General",
                IsReadOnly = false,
                CreatedAt = seedTime,
                CreatedBy = "System"
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                Key = "General.SupportEmail",
                Value = "support@onevision.local",
                DataType = "string",
                Description = "Default customer support and contact email",
                Group = "General",
                IsReadOnly = false,
                CreatedAt = seedTime,
                CreatedBy = "System"
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                Key = "Features.ProductExport.Enabled",
                Value = "true",
                DataType = "boolean",
                Description = "Toggle product catalog data export to Excel / CSV",
                Group = "Features",
                IsReadOnly = false,
                CreatedAt = seedTime,
                CreatedBy = "System"
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111104"),
                Key = "Features.AuditLogging.Enabled",
                Value = "true",
                DataType = "boolean",
                Description = "Enable system-wide database transaction audit trail",
                Group = "Features",
                IsReadOnly = false,
                CreatedAt = seedTime,
                CreatedBy = "System"
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111105"),
                Key = "Features.Registration.Enabled",
                Value = "true",
                DataType = "boolean",
                Description = "Allow new public user accounts to self-register",
                Group = "Features",
                IsReadOnly = false,
                CreatedAt = seedTime,
                CreatedBy = "System"
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111106"),
                Key = "Security.MaxFailedAccessAttempts",
                Value = "5",
                DataType = "number",
                Description = "Maximum consecutive login failures before temporary account lockout",
                Group = "Security",
                IsReadOnly = false,
                CreatedAt = seedTime,
                CreatedBy = "System"
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111107"),
                Key = "Security.SessionTimeoutMinutes",
                Value = "60",
                DataType = "number",
                Description = "Default JWT and UI user session timeout duration in minutes",
                Group = "Security",
                IsReadOnly = false,
                CreatedAt = seedTime,
                CreatedBy = "System"
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111108"),
                Key = "System.Environment",
                Value = "Production",
                DataType = "string",
                Description = "System hosting runtime profile (read-only system configuration)",
                Group = "System",
                IsReadOnly = true,
                CreatedAt = seedTime,
                CreatedBy = "System"
            }
        );
    }
}
