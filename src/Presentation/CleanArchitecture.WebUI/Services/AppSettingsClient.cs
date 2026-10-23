using System.Net.Http.Json;
using CleanArchitecture.WebUI.Services.Models;

namespace CleanArchitecture.WebUI.Services;

/// <summary>
/// HTTP Client untuk mengelola konfigurasi aplikasi dan Feature Flags dari WebUI.
/// </summary>
public class AppSettingsClient
{
    private readonly AuthorizedHttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;

    public AppSettingsClient(
        AuthorizedHttpClient httpClient,
        TokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
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
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
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
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
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
            using var request = new HttpRequestMessage(HttpMethod.Put, url);
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
