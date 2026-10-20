using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.WebAPI.Controllers;
using Shouldly;

namespace CleanArchitecture.Application.IntegrationTests;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("api/v1/Users");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_AsMember_ReturnsForbidden()
    {
        var token = await GetAccessTokenAsync("testmember@test.com", "Member123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsOkWithUsers()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.ShouldNotBeNull();
        users.Count.ShouldBeGreaterThanOrEqualTo(2);
        users.ShouldContain(u => u.Email == "testadmin@test.com");
    }

    [Fact]
    public async Task GetAllPermissions_AsAdmin_ReturnsGroupedPermissions()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Users/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var permissions = await response.Content.ReadFromJsonAsync<Dictionary<string, List<string>>>();
        permissions.ShouldNotBeNull();
        permissions.ContainsKey("Products").ShouldBeTrue();
        permissions.ContainsKey("Users").ShouldBeTrue();
        permissions.ContainsKey("Audit Logs").ShouldBeTrue();
        permissions.ContainsKey("App Settings").ShouldBeTrue();
        permissions.ContainsKey("Notifications").ShouldBeTrue();
    }

    [Fact]
    public async Task GetRolePermissions_AsAdmin_ReturnsMemberPermissions()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Users/roles/Member/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var permissions = await response.Content.ReadFromJsonAsync<List<string>>();
        permissions.ShouldNotBeNull();
        permissions.ShouldContain(Permissions.Products.View);
    }

    [Fact]
    public async Task UpdateRolePermissions_AsMember_ReturnsForbidden()
    {
        var token = await GetAccessTokenAsync("testmember@test.com", "Member123!");

        using var request = new HttpRequestMessage(HttpMethod.Put, "api/v1/Users/roles/Member/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new UpdateRolePermissionsRequest
        {
            Permissions = new List<string> { Permissions.Products.View, Permissions.Products.Create }
        });

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateRolePermissions_AsAdmin_UpdatesPermissionsSuccessfully()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        // Update Member to have Products.View and Products.Create
        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, "api/v1/Users/roles/Member/permissions");
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        updateRequest.Content = JsonContent.Create(new UpdateRolePermissionsRequest
        {
            Permissions = new List<string> { Permissions.Products.View, Permissions.Products.Create }
        });

        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify updated permissions
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, "api/v1/Users/roles/Member/permissions");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var getResponse = await _client.SendAsync(getRequest);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var permissions = await getResponse.Content.ReadFromJsonAsync<List<string>>();
        permissions.ShouldNotBeNull();
        permissions.ShouldContain(Permissions.Products.Create);
    }

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
        List<string> Roles,
        List<string> Permissions);
}
