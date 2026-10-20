using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Events;

/// <summary>
/// Domain event yang dipicu saat nilai konfigurasi AppSetting diperbarui
/// </summary>
public class AppSettingChangedEvent : IDomainEvent
{
    public AppSetting Setting { get; }
    public DateTime OccurredOn { get; }

    public AppSettingChangedEvent(AppSetting setting)
    {
        Setting = setting;
        OccurredOn = DateTime.UtcNow;
    }
}
