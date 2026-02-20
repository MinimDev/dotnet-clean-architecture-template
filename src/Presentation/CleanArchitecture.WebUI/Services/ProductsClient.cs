using CleanArchitecture.WebUI.Services.Models;
using System.Net.Http.Json;

namespace CleanArchitecture.WebUI.Services;

public class ProductsClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;

    public ProductsClient(IHttpClientFactory httpClientFactory, TokenProvider tokenProvider)
    {
        _httpClient = httpClientFactory.CreateClient("WebAPI");
        _tokenProvider = tokenProvider;
    }

    private void AddAuthorizationHeader()
    {
        if (!string.IsNullOrEmpty(_tokenProvider.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.Token);
        }
    }

    public async Task<Result<PaginatedList<ProductDto>>> GetProductsAsync(int pageNumber, int pageSize, string? searchTerm)
    {
        AddAuthorizationHeader();
        var queryString = $"?pageNumber={pageNumber}&pageSize={pageSize}"; // Simplified query string builder
        if (!string.IsNullOrEmpty(searchTerm))
        {
            queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        }

        try 
        {
            var response = await _httpClient.GetFromJsonAsync<Result<PaginatedList<ProductDto>>>($"api/v1/Products{queryString}");
            return response ?? new Result<PaginatedList<ProductDto>> { IsSuccess = false, Error = "No response" };
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
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/v1/Products", command);
            var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
            return result ?? new Result<Guid> { IsSuccess = false, Error = "No response" };
        }
        catch (Exception ex)
        {
            return new Result<Guid> { IsSuccess = false, Error = ex.Message };
        }
    }
}
