using CleanArchitecture.WebUI.Services.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace CleanArchitecture.WebUI.Services;

public class ProductsClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;
    private readonly AuthenticationStateProvider _authStateProvider;

    public ProductsClient(
        IHttpClientFactory httpClientFactory,
        TokenProvider tokenProvider,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("WebAPI");
        _tokenProvider = tokenProvider;
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Retrieves the current JWT token from <see cref="TokenProvider"/>.
    /// If the token is not yet available (e.g. after a page refresh), forces a call to
    /// <see cref="AuthenticationStateProvider.GetAuthenticationStateAsync"/> which restores
    /// the token from browser local storage before returning.
    /// </summary>
    private async Task<string?> GetTokenAsync()
    {
        if (string.IsNullOrEmpty(_tokenProvider.Token))
        {
            await _authStateProvider.GetAuthenticationStateAsync();
        }
        return _tokenProvider.Token;
    }

    /// <summary>
    /// Builds an <see cref="HttpRequestMessage"/> with the Authorization header set per-request.
    /// Using per-request headers is more reliable than <c>DefaultRequestHeaders</c>, which can
    /// cause issues when the underlying <see cref="HttpClient"/> is recycled by
    /// <see cref="IHttpClientFactory"/>.
    /// </summary>
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

    public async Task<Result<PaginatedList<ProductDto>>> GetProductsAsync(int pageNumber, int pageSize, string? searchTerm)
    {
        var queryString = $"?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm))
            queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Get, $"api/v1/Products{queryString}");
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var tokenStatus = string.IsNullOrEmpty(_tokenProvider.Token) ? "NO TOKEN" : "token present";
                return new Result<PaginatedList<ProductDto>>
                {
                    IsSuccess = false,
                    Error = $"HTTP {(int)response.StatusCode} ({tokenStatus})"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<Result<PaginatedList<ProductDto>>>();
            return result ?? new Result<PaginatedList<ProductDto>> { IsSuccess = false, Error = "Empty response" };
        }
        catch (Exception ex)
        {
            return new Result<PaginatedList<ProductDto>> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<Result<Guid>> CreateProductAsync(CreateProductCommand command)
    {
        try
        {
            using HttpRequestMessage request = await BuildRequestAsync(HttpMethod.Post, "api/v1/Products");
            request.Content = JsonContent.Create(command);
            using var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
            return result ?? new Result<Guid> { IsSuccess = false, Error = "No response" };
        }
        catch (Exception ex)
        {
            return new Result<Guid> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<Result> UpdateProductAsync(Guid id, UpdateProductDto command)
    {
        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Put, $"api/v1/Products/{id}");
            request.Content = JsonContent.Create(command);
            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) return Result.Success();
            var error = await response.Content.ReadAsStringAsync();
            return Result.Failure(error);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteProductAsync(Guid id)
    {
        try
        {
            using var request = await BuildRequestAsync(HttpMethod.Delete, $"api/v1/Products/{id}");
            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) return Result.Success();
            var error = await response.Content.ReadAsStringAsync();
            return Result.Failure(error);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
