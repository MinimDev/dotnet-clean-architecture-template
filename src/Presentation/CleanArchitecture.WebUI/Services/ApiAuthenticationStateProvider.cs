using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using CleanArchitecture.WebUI.Services.Models;

namespace CleanArchitecture.WebUI.Services;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly TokenProvider _tokenProvider;
    private readonly ProtectedLocalStorage _localStorage;

    public ApiAuthenticationStateProvider(TokenProvider tokenProvider, ProtectedLocalStorage localStorage)
    {
        _tokenProvider = tokenProvider;
        _localStorage = localStorage;
        _tokenProvider.OnChange += StateChanged;
    }

    public void Dispose()
    {
        _tokenProvider.OnChange -= StateChanged;
    }

    private void StateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Try to load from local storage if token is missing
        if (string.IsNullOrEmpty(_tokenProvider.Token))
        {
            try
            {
                var storedData = await _localStorage.GetAsync<AuthResponse>("userInfo");
                if (storedData.Success && storedData.Value != null)
                {
                    _tokenProvider.SetToken(storedData.Value.Token, storedData.Value.UserName, storedData.Value.Email);
                }
            }
            catch
            {
                // Ignore exceptions (e.g. pre-rendering)
            }
        }

        var identity = new ClaimsIdentity();

        if (!string.IsNullOrEmpty(_tokenProvider.Token))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _tokenProvider.UserName ?? ""),
                new Claim(ClaimTypes.Email, _tokenProvider.Email ?? "")
            };
            identity = new ClaimsIdentity(claims, "jwt");
        }

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }
}
