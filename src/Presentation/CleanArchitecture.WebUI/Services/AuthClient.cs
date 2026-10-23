using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanArchitecture.WebUI.Services.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CleanArchitecture.WebUI.Services;

public class AuthClient
{
    private readonly HttpClient _rawHttpClient;
    private readonly AuthorizedHttpClient _authorizedClient;
    private readonly TokenProvider _tokenProvider;
    private readonly ProtectedLocalStorage _localStorage;

    public AuthClient(
        IHttpClientFactory httpClientFactory,
        AuthorizedHttpClient authorizedClient,
        TokenProvider tokenProvider,
        ProtectedLocalStorage localStorage)
    {
        _rawHttpClient = httpClientFactory.CreateClient("WebAPI.Raw");
        _authorizedClient = authorizedClient;
        _tokenProvider = tokenProvider;
        _localStorage = localStorage;
    }

    public async Task<Result> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _rawHttpClient.PostAsJsonAsync("api/v1/Auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    _tokenProvider.SetToken(
                        authResponse.AccessToken,
                        authResponse.RefreshToken,
                        authResponse.UserName,
                        authResponse.Email,
                        authResponse.Roles,
                        authResponse.Permissions);

                    try
                    {
                        await _localStorage.SetAsync("userInfo", authResponse);
                    }
                    catch
                    {
                    }

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

    public async Task<bool> RefreshTokenAsync()
    {
        return await _authorizedClient.TryRefreshTokenAsync();
    }

    public async Task LogoutAsync()
    {
        // Revoke refresh token on server
        if (!string.IsNullOrEmpty(_tokenProvider.RefreshToken))
        {
            try
            {
                using var revokeRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/Auth/revoke");
                revokeRequest.Content = JsonContent.Create(new { RefreshToken = _tokenProvider.RefreshToken });
                if (!string.IsNullOrEmpty(_tokenProvider.Token))
                {
                    revokeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.Token);
                }
                await _rawHttpClient.SendAsync(revokeRequest);
            }
            catch
            {
                // Best-effort — always clear local state
            }
        }

        await _authorizedClient.HandleSessionExpiredAsync();
    }

    public async Task<Result<List<UserDto>>> GetUsersAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Users");
            using var response = await _authorizedClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return Result<List<UserDto>>.Failure($"HTTP {(int)response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            return Result<List<UserDto>>.Success(result ?? new List<UserDto>());
        }
        catch (Exception ex)
        {
            return Result<List<UserDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateUserRoleAsync(string email, string newRole)
    {
        try
        {
            var requestObj = new UpdateUserRoleRequest { Role = newRole };
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/Users/{Uri.EscapeDataString(email)}/roles");
            request.Content = JsonContent.Create(requestObj);

            using var response = await _authorizedClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return Result.Success();

            return Result.Failure("Failed to update user role");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<Dictionary<string, List<string>>>> GetAllPermissionsAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Users/permissions");
            using var response = await _authorizedClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return Result<Dictionary<string, List<string>>>.Failure($"HTTP {(int)response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, List<string>>>();
            return Result<Dictionary<string, List<string>>>.Success(result ?? new Dictionary<string, List<string>>());
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, List<string>>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<string>>> GetRolePermissionsAsync(string roleName)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/Users/roles/{Uri.EscapeDataString(roleName)}/permissions");
            using var response = await _authorizedClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return Result<List<string>>.Failure($"HTTP {(int)response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            return Result<List<string>>.Success(result ?? new List<string>());
        }
        catch (Exception ex)
        {
            return Result<List<string>>.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateRolePermissionsAsync(string roleName, List<string> permissions)
    {
        try
        {
            var requestObj = new UpdateRolePermissionsRequest { Permissions = permissions };
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/Users/roles/{Uri.EscapeDataString(roleName)}/permissions");
            request.Content = JsonContent.Create(requestObj);

            using var response = await _authorizedClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return Result.Success();

            return Result.Failure("Failed to update role permissions");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
