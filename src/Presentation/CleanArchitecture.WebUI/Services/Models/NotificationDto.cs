namespace CleanArchitecture.WebUI.Services.Models;

public enum NotificationType
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
