using CleanArchitecture.WebUI.Services.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace CleanArchitecture.WebUI.Services;

public class AuthClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;
    private readonly ProtectedLocalStorage _localStorage;

    public AuthClient(IHttpClientFactory httpClientFactory, TokenProvider tokenProvider, ProtectedLocalStorage localStorage)
    {
        _httpClient = httpClientFactory.CreateClient("WebAPI");
        _tokenProvider = tokenProvider;
        _localStorage = localStorage;
    }

    public async Task<Result> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    _tokenProvider.SetToken(authResponse.Token, authResponse.UserName, authResponse.Email);
                    await _localStorage.SetAsync("userInfo", authResponse);
                    return Result.Success();
                }
            }

            return Result.Failure("Login failed");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task Logout()
    {
        _tokenProvider.ClearToken();
        await _localStorage.DeleteAsync("userInfo");
    }
}
