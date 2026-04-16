using System.Net;
using System.Net.Http.Json;
using Shouldly;

namespace CleanArchitecture.Application.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ─── Register ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithTokens()
    {
        var response = await _client.PostAsJsonAsync("api/v1/Auth/register", new
        {
            Email = "newuser@test.com",
            Password = "Test123!",
            FullName = "New User"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.ShouldNotBeNull();
        body!.AccessToken.ShouldNotBeNullOrEmpty();
        body.RefreshToken.ShouldNotBeNullOrEmpty();
        body.Email.ShouldBe("newuser@test.com");
        body.Roles.ShouldContain("Member");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // First registration
        await _client.PostAsJsonAsync("api/v1/Auth/register", new
        {
            Email = "duplicate@test.com",
            Password = "Test123!",
            FullName = "First User"
        });

        // Duplicate
        var response = await _client.PostAsJsonAsync("api/v1/Auth/register", new
        {
            Email = "duplicate@test.com",
            Password = "Test123!",
            FullName = "Second User"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // ─── Login ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        var response = await _client.PostAsJsonAsync("api/v1/Auth/login", new
        {
            Email = "testadmin@test.com",
            Password = "Admin123!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.ShouldNotBeNull();
        body!.AccessToken.ShouldNotBeNullOrEmpty();
        body.RefreshToken.ShouldNotBeNullOrEmpty();
        body.Roles.ShouldContain("Admin");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("api/v1/Auth/login", new
        {
            Email = "testadmin@test.com",
            Password = "WrongPassword!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("api/v1/Auth/login", new
        {
            Email = "nobody@nowhere.com",
            Password = "Test123!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // ─── Refresh ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNewTokens()
    {
        // Login to get tokens
        var loginResponse = await _client.PostAsJsonAsync("api/v1/Auth/login", new
        {
            Email = "testmember@test.com",
            Password = "Member123!"
        });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        var originalRefreshToken = loginBody!.RefreshToken;

        // Refresh
        var refreshResponse = await _client.PostAsJsonAsync("api/v1/Auth/refresh", new
        {
            RefreshToken = originalRefreshToken
        });

        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<RefreshResponseDto>();
        refreshBody.ShouldNotBeNull();
        refreshBody!.AccessToken.ShouldNotBeNullOrEmpty();
        refreshBody.RefreshToken.ShouldNotBeNullOrEmpty();
        // New refresh token should differ (rotation)
        refreshBody.RefreshToken.ShouldNotBe(originalRefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("api/v1/Auth/refresh", new
        {
            RefreshToken = "this-is-not-a-valid-token"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_AfterTokenRotation_OldTokenIsInvalid()
    {
        // Login
        var loginBody = (await (await _client.PostAsJsonAsync("api/v1/Auth/login", new
        {
            Email = "testmember@test.com",
            Password = "Member123!"
        })).Content.ReadFromJsonAsync<AuthResponseDto>())!;

        var originalRefreshToken = loginBody.RefreshToken;

        // First refresh — valid
        var firstRefresh = await _client.PostAsJsonAsync("api/v1/Auth/refresh", new
        {
            RefreshToken = originalRefreshToken
        });
        firstRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Second refresh with the ORIGINAL (now rotated) token — should fail
        var secondRefresh = await _client.PostAsJsonAsync("api/v1/Auth/refresh", new
        {
            RefreshToken = originalRefreshToken
        });
        secondRefresh.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // ─── DTOs ─────────────────────────────────────────────────────────────────────

    private record AuthResponseDto(
        string AccessToken,
        string RefreshToken,
        string Email,
        string UserName,
        List<string> Roles);

    private record RefreshResponseDto(
        string AccessToken,
        string RefreshToken);
}
