namespace CleanArchitecture.WebUI.Services;

public class TokenProvider
{
    public string? Token { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }

    public event Action? OnChange;

    public void SetToken(string token, string userName, string email)
    {
        Token = token;
        UserName = userName;
        Email = email;
        OnChange?.Invoke();
    }

    public void ClearToken()
    {
        Token = null;
        UserName = null;
        Email = null;
        OnChange?.Invoke();
    }
}
