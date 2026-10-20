using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanArchitecture.WebUI.Services.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace CleanArchitecture.WebUI.Services;

/// <summary>
/// HTTP Client untuk mengelola notifikasi in-app dari WebUI
/// </summary>
public class NotificationsClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;
    private readonly AuthenticationStateProvider _authStateProvider;

    public NotificationsClient(
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

    public async Task<Result<PaginatedList<NotificationDto>>> GetNotificationsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        bool? unreadOnly = null)
    {
        var url = $"api/v1/Notifications?pageNumber={pageNumber}&pageSize={pageSize}";
        if (unreadOnly.HasValue)
        {
            url += $"&unreadOnly={unreadOnly.Value.ToString().ToLowerInvariant()}";
        }

        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new Result<PaginatedList<NotificationDto>>
                {
                    IsSuccess = false,
                    Error = $"HTTP {(int)response.StatusCode}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<Result<PaginatedList<NotificationDto>>>();
            return result ?? new Result<PaginatedList<NotificationDto>> { IsSuccess = false, Error = "Empty response" };
        }
        catch (Exception ex)
        {
            return new Result<PaginatedList<NotificationDto>> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<Result<int>> GetUnreadCountAsync()
    {
        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Get, "api/v1/Notifications/unread-count");
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new Result<int> { IsSuccess = false, Error = $"HTTP {(int)response.StatusCode}" };
            }

            var result = await response.Content.ReadFromJsonAsync<Result<int>>();
            return result ?? new Result<int> { IsSuccess = false, Error = "Empty response" };
        }
        catch (Exception ex)
        {
            return new Result<int> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<Result> MarkAsReadAsync(Guid id)
    {
        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Patch, $"api/v1/Notifications/{id}/read");
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = await response.Content.ReadAsStringAsync();
            return Result.Failure(error);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> MarkAllAsReadAsync()
    {
        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Patch, "api/v1/Notifications/read-all");
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = await response.Content.ReadAsStringAsync();
            return Result.Failure(error);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
