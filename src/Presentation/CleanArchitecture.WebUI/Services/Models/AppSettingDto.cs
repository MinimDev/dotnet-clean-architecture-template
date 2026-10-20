namespace CleanArchitecture.WebUI.Services.Models;

/// <summary>
/// Model DTO untuk konfigurasi aplikasi di WebUI.
/// </summary>
public class AppSettingDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public string? Description { get; set; }
    public string Group { get; set; } = "General";
    public bool IsReadOnly { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}

/// <summary>
/// Model request untuk memperbarui nilai AppSetting melalui API.
/// </summary>
public class UpdateAppSettingRequest
{
    public string Value { get; set; } = string.Empty;
}
