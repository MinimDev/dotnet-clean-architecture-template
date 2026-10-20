using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.Notifications.DTOs;
using Shouldly;

namespace CleanArchitecture.Application.IntegrationTests;

public class NotificationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public NotificationsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetNotifications_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("api/v1/Notifications");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetNotifications_WithAuth_ReturnsOkWithNotifications()
    {
        var token = await GetAccessTokenAsync("testmember@test.com", "Member123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Notifications?pageNumber=1&pageSize=10");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<PaginatedList<NotificationDto>>>();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
        result.Data.Items.ShouldContain(n => n.Title.Contains("Welcome"));
    }

    [Fact]
    public async Task GetUnreadCount_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("api/v1/Notifications/unread-count");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUnreadCount_WithAuth_ReturnsOkCount()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Notifications/unread-count");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<int>>();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task MarkAsRead_ExistingNotification_UpdatesReadStatus()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        // 1. Ambil notifikasi pertama
        using var getReq = new HttpRequestMessage(HttpMethod.Get, "api/v1/Notifications?pageNumber=1&pageSize=10");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getRes = await _client.SendAsync(getReq);
        getRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        var listResult = await getRes.Content.ReadFromJsonAsync<Result<PaginatedList<NotificationDto>>>();
        listResult.ShouldNotBeNull();
        var targetNotification = listResult.Data!.Items.First();

        // 2. Tandai sebagai dibaca
        using var patchReq = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/Notifications/{targetNotification.Id}/read");
        patchReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var patchRes = await _client.SendAsync(patchReq);
        patchRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        var patchResult = await patchRes.Content.ReadFromJsonAsync<Result>();
        patchResult.ShouldNotBeNull();
        patchResult.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MarkAllAsRead_WithAuth_MarksAllNotificationsAsRead()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var patchReq = new HttpRequestMessage(HttpMethod.Patch, "api/v1/Notifications/read-all");
        patchReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var patchRes = await _client.SendAsync(patchReq);
        patchRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verifikasi unread-count menjadi 0
        using var countReq = new HttpRequestMessage(HttpMethod.Get, "api/v1/Notifications/unread-count");
        countReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var countRes = await _client.SendAsync(countReq);
        countRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        var countResult = await countRes.Content.ReadFromJsonAsync<Result<int>>();
        countResult.ShouldNotBeNull();
        countResult.IsSuccess.ShouldBeTrue();
        countResult.Data.ShouldBe(0);
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
        List<string> Roles);
}
