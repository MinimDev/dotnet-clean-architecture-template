namespace CleanArchitecture.WebUI.Services;

public class TokenProvider
{
    public string? Token { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public IList<string> Roles { get; private set; } = new List<string>();

    public event Action? OnChange;

    public void SetToken(string token, string userName, string email, IList<string> roles)
    {
        Token = token;
        UserName = userName;
        Email = email;
        Roles = roles ?? new List<string>();
        OnChange?.Invoke();
    }

    public void ClearToken()
    {
        Token = null;
        UserName = null;
        Email = null;
        Roles = new List<string>();
        OnChange?.Invoke();
    }
}
