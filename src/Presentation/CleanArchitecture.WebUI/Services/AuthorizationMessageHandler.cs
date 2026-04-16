using System.Net.Http.Json;
using CleanArchitecture.WebUI.Services.Models;

namespace CleanArchitecture.WebUI.Services;

/// <summary>
/// DelegatingHandler yang otomatis menyisipkan Bearer token ke setiap HTTP request.
/// Jika mendapat 401, akan mencoba silent refresh token sebelum retry.
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly TokenProvider _tokenProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    // Prevent concurrent refresh calls
    private static readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

    public AuthorizationMessageHandler(TokenProvider tokenProvider, IHttpClientFactory httpClientFactory)
    {
        _tokenProvider = tokenProvider;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_tokenProvider.Token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.Token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If 401 and we have a refresh token, attempt silent refresh
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
            && !string.IsNullOrEmpty(_tokenProvider.RefreshToken))
        {
            var refreshed = await TryRefreshTokenAsync(cancellationToken);
            if (refreshed)
            {
                // Clone and retry request with new access token
                var retryRequest = await CloneRequestAsync(request);
                retryRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.Token!);
                return await base.SendAsync(retryRequest, cancellationToken);
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            var refreshResponse = await client.PostAsJsonAsync(
                "api/v1/Auth/refresh",
                new { RefreshToken = _tokenProvider.RefreshToken },
                cancellationToken);

            if (!refreshResponse.IsSuccessStatusCode)
                return false;

            var result = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>(
                cancellationToken: cancellationToken);

            if (result == null)
                return false;

            _tokenProvider.UpdateAccessToken(result.AccessToken, result.RefreshToken);
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
