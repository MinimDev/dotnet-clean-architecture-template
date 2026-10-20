using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration untuk entity Notification
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.UserId)
            .HasMaxLength(450);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.Type)
            .IsRequired();

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => new { n.UserId, n.IsRead });

        // Seed default initial broadcast notification
        var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                UserId = (string?)null, // Broadcast ke semua user
                Title = "Welcome to OneVision Clean Architecture!",
                Message = "Sistem telah diperbarui ke v1.7.0 dengan fitur Idempotency, Audit Trail, App Settings, dan In-App Notifications.",
                Type = NotificationType.Success,
                IsRead = false,
                ActionUrl = "/settings",
                CreatedAt = seedTime,
                ReadAt = (DateTime?)null
            }
        );
    }
}
