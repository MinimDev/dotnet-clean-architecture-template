using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Shouldly;

namespace CleanArchitecture.Application.IntegrationTests;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ─── Authorization Guard ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("api/v1/Products");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProducts_WithValidJwt_ReturnsOk()
    {
        var token = await GetAccessTokenAsync("testmember@test.com", "Member123!");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("api/v1/Products");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // ─── RBAC ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_AsMember_ReturnsForbidden()
    {
        var token = await GetAccessTokenAsync("testmember@test.com", "Member123!");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("api/v1/Products", new
        {
            Name = "Test Product",
            Price = 99.99,
            Description = "A product created in integration test"
        });

        // Members should not be able to create products (Admin only)
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateProduct_AsAdmin_ReturnsCreatedOrOk()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("api/v1/Products", new
        {
            Name = "Admin Test Product",
            Price = 149.99,
            Description = "Created by admin in integration test"
        });

        // Admin should be able to create
        ((int)response.StatusCode).ShouldBeInRange(200, 201);
    }

    // ─── Idempotency Key ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_WithSameIdempotencyKey_ReturnsSameCachedResultWithoutDuplication()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");
        var idempotencyKey = Guid.NewGuid().ToString();

        var requestPayload = new
        {
            Name = $"Idempotency-{Guid.NewGuid():N}",
            Price = 299.99,
            Description = "Testing idempotency key header"
        };

        // First request with Idempotency-Key
        using var request1 = new HttpRequestMessage(HttpMethod.Post, "api/v1/Products")
        {
            Content = JsonContent.Create(requestPayload)
        };
        request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request1.Headers.Add("Idempotency-Key", idempotencyKey);

        var response1 = await _client.SendAsync(request1);
        ((int)response1.StatusCode).ShouldBeInRange(200, 201);
        var body1 = await response1.Content.ReadFromJsonAsync<CleanArchitecture.Application.Common.Models.Result<Guid>>();
        body1.ShouldNotBeNull();
        body1.IsSuccess.ShouldBeTrue();
        var createdProductId = body1.Data;

        // Second request with THE SAME Idempotency-Key (simulating double submit)
        using var request2 = new HttpRequestMessage(HttpMethod.Post, "api/v1/Products")
        {
            Content = JsonContent.Create(requestPayload)
        };
        request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request2.Headers.Add("Idempotency-Key", idempotencyKey);

        var response2 = await _client.SendAsync(request2);
        ((int)response2.StatusCode).ShouldBeInRange(200, 201);
        var body2 = await response2.Content.ReadFromJsonAsync<CleanArchitecture.Application.Common.Models.Result<Guid>>();
        body2.ShouldNotBeNull();
        body2.IsSuccess.ShouldBeTrue();
        body2.Data.ShouldBe(createdProductId);

        // Verify only 1 product exists with this specific name
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var listResponse = await _client.GetAsync($"api/v1/Products?searchTerm={requestPayload.Name}");
        listResponse.EnsureSuccessStatusCode();
        var listBody = await listResponse.Content.ReadFromJsonAsync<CleanArchitecture.Application.Common.Models.Result<CleanArchitecture.Application.Common.Models.PaginatedList<CleanArchitecture.Application.Features.Products.Queries.GetProductsList.ProductDto>>>();
        listBody.ShouldNotBeNull();
        listBody.Data!.TotalCount.ShouldBe(1);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private async Task<string> GetAccessTokenAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("api/v1/Auth/login", new
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    private record AuthResponseDto(
        string AccessToken,
        string RefreshToken,
        string Email,
        string UserName,
        List<string> Roles);
}
