using System.Net.Http.Json;
using CleanArchitecture.WebUI.Services.Models;

namespace CleanArchitecture.WebUI.Services;

public class ProductsClient
{
    private readonly AuthorizedHttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;

    public ProductsClient(
        AuthorizedHttpClient httpClient,
        TokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<PaginatedList<ProductDto>>> GetProductsAsync(int pageNumber, int pageSize, string? searchTerm)
    {
        var queryString = $"?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm))
            queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/Products{queryString}");
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
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/Products");
            request.Content = JsonContent.Create(command);
            // Idempotency-Key: unique per form submission.
            // Memastikan double-click atau retry tidak membuat data duplikat.
            request.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString());

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
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/Products/{id}");
            request.Content = JsonContent.Create(command);
            request.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString());

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
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/Products/{id}");
            request.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString());

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
