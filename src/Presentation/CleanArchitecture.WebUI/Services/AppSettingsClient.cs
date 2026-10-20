using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanArchitecture.WebUI.Services.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace CleanArchitecture.WebUI.Services;

/// <summary>
/// HTTP Client untuk mengelola konfigurasi aplikasi dan Feature Flags dari WebUI.
/// </summary>
public class AppSettingsClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AppSettingsClient(
        IHttpClientFactory httpClientFactory,
        TokenProvider tokenProvider,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("WebAPI");
        _tokenProvider = tokenProvider;
        _authStateProvider = authStateProvider;
    }

    private async Task<string?> GetTokenAsync()
    {
        if (string.IsNullOrEmpty(_tokenProvider.Token))
        {
            await _authStateProvider.GetAuthenticationStateAsync();
        }
        return _tokenProvider.Token;
    }

    private async Task<HttpRequestMessage> BuildRequestAsync(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return request;
    }

    public async Task<Result<List<AppSettingDto>>> GetAppSettingsAsync(string? group = null)
    {
        var url = "api/v1/AppSettings";
        if (!string.IsNullOrWhiteSpace(group))
        {
            url += $"?group={Uri.EscapeDataString(group)}";
        }

        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var tokenStatus = string.IsNullOrEmpty(_tokenProvider.Token) ? "NO TOKEN" : "token present";
                return new Result<List<AppSettingDto>>
                {
                    IsSuccess = false,
                    Error = $"HTTP {(int)response.StatusCode} ({tokenStatus})"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<Result<List<AppSettingDto>>>();
            return result ?? new Result<List<AppSettingDto>> { IsSuccess = false, Error = "Empty response" };
        }
        catch (Exception ex)
        {
            return new Result<List<AppSettingDto>> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<Result<AppSettingDto>> GetAppSettingByKeyAsync(string key)
    {
        var url = $"api/v1/AppSettings/{Uri.EscapeDataString(key)}";

        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new Result<AppSettingDto>
                {
                    IsSuccess = false,
                    Error = $"HTTP {(int)response.StatusCode}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<Result<AppSettingDto>>();
            return result ?? new Result<AppSettingDto> { IsSuccess = false, Error = "Empty response" };
        }
        catch (Exception ex)
        {
            return new Result<AppSettingDto> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<Result> UpdateAppSettingAsync(string key, string value)
    {
        var url = $"api/v1/AppSettings/{Uri.EscapeDataString(key)}";

        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Put, url);
            request.Content = JsonContent.Create(new UpdateAppSettingRequest { Value = value });
            request.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString());

            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result>();
                return result ?? Result.Success();
            }

            var errorObj = await response.Content.ReadFromJsonAsync<Result>();
            if (errorObj != null && !string.IsNullOrEmpty(errorObj.Error))
            {
                return Result.Failure(errorObj.Error);
            }

            var rawError = await response.Content.ReadAsStringAsync();
            return Result.Failure(string.IsNullOrWhiteSpace(rawError) ? $"HTTP {(int)response.StatusCode}" : rawError);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
