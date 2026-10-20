namespace CleanArchitecture.WebUI.Services;

public class TokenProvider
{
    public string? Token { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public IList<string> Roles { get; private set; } = new List<string>();
    public IList<string> Permissions { get; private set; } = new List<string>();

    public event Action? OnChange;

    public void SetToken(
        string token,
        string refreshToken,
        string userName,
        string email,
        IList<string> roles,
        IList<string>? permissions = null)
    {
        Token = token;
        RefreshToken = refreshToken;
        UserName = userName;
        Email = email;
        Roles = roles ?? new List<string>();
        Permissions = permissions ?? new List<string>();
        OnChange?.Invoke();
    }

    public void UpdateAccessToken(string newAccessToken, string newRefreshToken)
    {
        Token = newAccessToken;
        RefreshToken = newRefreshToken;
        OnChange?.Invoke();
    }

    public void ClearToken()
    {
        Token = null;
        RefreshToken = null;
        UserName = null;
        Email = null;
        Roles = new List<string>();
        Permissions = new List<string>();
        OnChange?.Invoke();
    }

    /// <summary>
    /// Memeriksa apakah pengguna memiliki hak akses (Permission) tertentu, atau merupakan Admin
    /// </summary>
    public bool HasPermission(string permission)
    {
        if (Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
