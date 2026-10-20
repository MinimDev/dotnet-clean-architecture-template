using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Events;

namespace CleanArchitecture.Domain.Entities;

/// <summary>
/// Entity untuk menyimpan konfigurasi aplikasi dan feature flags dinamis di database.
/// Mendukung berbagai tipe data (string, boolean, number) dan grouping.
/// </summary>
public class AppSetting : BaseAuditableEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string DataType { get; private set; } = "string"; // "string" | "boolean" | "number"
    public string? Description { get; private set; }
    public string Group { get; private set; } = "General";   // "General" | "Features" | "Security" | "System"
    public bool IsReadOnly { get; private set; }             // Jika true, tidak bisa diedit melalui UI

    // Private constructor untuk EF Core
    private AppSetting() { }

    public static AppSetting Create(
        string key,
        string value,
        string dataType = "string",
        string? description = null,
        string group = "General",
        bool isReadOnly = false)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Setting key cannot be empty.", nameof(key));

        var setting = new AppSetting
        {
            Id = Guid.NewGuid(),
            Key = key.Trim(),
            Value = value,
            DataType = dataType.ToLowerInvariant(),
            Description = description,
            Group = string.IsNullOrWhiteSpace(group) ? "General" : group.Trim(),
            IsReadOnly = isReadOnly
        };

        return setting;
    }

    /// <summary>
    /// Memperbarui nilai konfigurasi. Melempar InvalidOperationException jika setting berstatus IsReadOnly.
    /// </summary>
    public void UpdateValue(string newValue)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException($"Setting '{Key}' is read-only and cannot be modified.");
        }

        if (Value != newValue)
        {
            Value = newValue;
            AddDomainEvent(new AppSettingChangedEvent(this));
        }
    }

    /// <summary>
    /// Memperbarui deskripsi setting.
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description;
    }
}
