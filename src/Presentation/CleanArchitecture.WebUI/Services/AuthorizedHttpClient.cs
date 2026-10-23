using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanArchitecture.WebUI.Services.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CleanArchitecture.WebUI.Services;

/// <summary>
/// HTTP Client terpusat dengan penanganan Bearer token otomatis,
/// silent token refresh transparan saat 401 Unauthorized,
/// dan auto-logout saat sesi / refresh token telah berakhir.
/// </summary>
public class AuthorizedHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenProvider _tokenProvider;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly NavigationManager _navigationManager;

    // Menghindari refresh token ditembak berulang kali secara paralel dari request berbeda
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    public AuthorizedHttpClient(
        IHttpClientFactory httpClientFactory,
        TokenProvider tokenProvider,
        ProtectedLocalStorage localStorage,
        AuthenticationStateProvider authStateProvider,
        NavigationManager navigationManager)
    {
        _httpClientFactory = httpClientFactory;
        _httpClient = httpClientFactory.CreateClient("WebAPI");
        _tokenProvider = tokenProvider;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Memastikan access token aktif tersedia (memulihkan dari local storage jika belum dimuat)
    /// </summary>
    public async Task<string?> EnsureTokenAsync()
    {
        if (string.IsNullOrEmpty(_tokenProvider.Token))
        {
            await _authStateProvider.GetAuthenticationStateAsync();
        }
        return _tokenProvider.Token;
    }

    /// <summary>
    /// Mengirimkan HTTP request dengan penyisipan Bearer token dan auto-retry silent refresh saat 401
    /// </summary>
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var token = await EnsureTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

        // Abaikan endpoint Auth untuk mencegah circular refresh
        var path = request.RequestUri?.ToString() ?? string.Empty;
        if (path.Contains("/api/v1/Auth/", StringComparison.OrdinalIgnoreCase))
        {
            return response;
        }

        // Jika 401 Unauthorized, lakukan silent refresh dan retry
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (string.IsNullOrEmpty(_tokenProvider.RefreshToken))
            {
                await HandleSessionExpiredAsync();
                return response;
            }

            var oldToken = _tokenProvider.Token;
            var refreshSuccess = await TryRefreshTokenAsync(oldToken, cancellationToken);

            if (refreshSuccess && !string.IsNullOrEmpty(_tokenProvider.Token))
            {
                // Clone request untuk dikirim ulang dengan access token baru
                using var retryRequest = await CloneRequestAsync(request);
                retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.Token);
                return await _httpClient.SendAsync(retryRequest, cancellationToken);
            }
            else
            {
                // Refresh token gagal atau expired -> Sesi berakhir
                await HandleSessionExpiredAsync();
                return response;
            }
        }

        return response;
    }

    /// <summary>
    /// Mencoba silent refresh token ke backend API dengan lock proteksi race-condition
    /// </summary>
    public async Task<bool> TryRefreshTokenAsync(string? failedToken = null, CancellationToken cancellationToken = default)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            // Jika token sudah diperbarui oleh request lain yang selesai duluan
            if (!string.IsNullOrEmpty(_tokenProvider.Token) && failedToken != null && _tokenProvider.Token != failedToken)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_tokenProvider.RefreshToken))
            {
                return false;
            }

            var rawClient = _httpClientFactory.CreateClient("WebAPI.Raw");
            var refreshResponse = await rawClient.PostAsJsonAsync(
                "api/v1/Auth/refresh",
                new { RefreshToken = _tokenProvider.RefreshToken },
                cancellationToken);

            if (!refreshResponse.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>(cancellationToken: cancellationToken);
            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {
                return false;
            }

            // Perbarui token di memori TokenProvider
            _tokenProvider.UpdateAccessToken(result.AccessToken, result.RefreshToken);

            // Perbarui ProtectedLocalStorage agar saat browser di-refresh token baru tetap dipakai
            try
            {
                var storedData = await _localStorage.GetAsync<AuthResponse>("userInfo");
                if (storedData.Success && storedData.Value != null)
                {
                    storedData.Value.AccessToken = result.AccessToken;
                    storedData.Value.RefreshToken = result.RefreshToken;
                    await _localStorage.SetAsync("userInfo", storedData.Value);
                }
            }
            catch
            {
                // Abaikan kesalahan interop jika circuit sedang background / disconnect
            }

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    /// <summary>
    /// Menangani sesi yang kedaluwarsa: membersihkan token, local storage, dan redirect ke login
    /// </summary>
    public async Task HandleSessionExpiredAsync()
    {
        _tokenProvider.ClearToken();

        try
        {
            await _localStorage.DeleteAsync("userInfo");
        }
        catch
        {
        }

        try
        {
            _navigationManager.NavigateTo("login?sessionExpired=true", forceLoad: false);
        }
        catch
        {
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);

        if (original.Content != null)
        {
            var body = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(body);
            foreach (var header in original.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }

    private class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
