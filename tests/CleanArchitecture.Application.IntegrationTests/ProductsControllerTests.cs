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
