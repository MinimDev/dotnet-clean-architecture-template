using CleanArchitecture.WebUI.Services.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CleanArchitecture.WebUI.Services;

public class AuthClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthClient(
        IHttpClientFactory httpClientFactory,
        TokenProvider tokenProvider,
        ProtectedLocalStorage localStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("WebAPI");
        _tokenProvider = tokenProvider;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    private async Task AddAuthorizationHeaderAsync()
    {
        if (string.IsNullOrEmpty(_tokenProvider.Token))
        {
            await _authStateProvider.GetAuthenticationStateAsync();
        }

        if (!string.IsNullOrEmpty(_tokenProvider.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _tokenProvider.Token);
        }
    }

    public async Task<Result> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    _tokenProvider.SetToken(authResponse.Token, authResponse.UserName, authResponse.Email, authResponse.Roles);
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

    public async Task<Result<List<UserDto>>> GetUsersAsync()
    {
        await AddAuthorizationHeaderAsync();
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/v1/Users");
            return Result<List<UserDto>>.Success(response ?? new List<UserDto>());
        }
        catch (Exception ex)
        {
            return Result<List<UserDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateUserRoleAsync(string email, string newRole)
    {
        await AddAuthorizationHeaderAsync();
        try
        {
            var request = new UpdateUserRoleRequest { Role = newRole };
            var response = await _httpClient.PutAsJsonAsync($"api/v1/Users/{Uri.EscapeDataString(email)}/roles", request);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            return Result.Failure("Failed to update user role");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
