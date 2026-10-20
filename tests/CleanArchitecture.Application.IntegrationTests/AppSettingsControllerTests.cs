using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.AppSettings.DTOs;
using CleanArchitecture.Application.Features.AuditLogs.Queries.GetAuditLogs;
using Shouldly;

namespace CleanArchitecture.Application.IntegrationTests;

public class AppSettingsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AppSettingsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAppSettings_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("api/v1/AppSettings");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAppSettings_AsMember_ReturnsForbidden()
    {
        var token = await GetAccessTokenAsync("testmember@test.com", "Member123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/AppSettings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppSettings_AsAdmin_ReturnsOkWithDefaultSeedSettings()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/AppSettings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<List<AppSettingDto>>>();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count.ShouldBeGreaterThanOrEqualTo(8);
        result.Data.ShouldContain(s => s.Key == "General.ApplicationName");
        result.Data.ShouldContain(s => s.Key == "Features.ProductExport.Enabled");
        result.Data.ShouldContain(s => s.Key == "System.Environment");
    }

    [Fact]
    public async Task GetAppSettings_WithGroupFilter_ReturnsOnlyMatchingGroup()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/AppSettings?group=Features");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<List<AppSettingDto>>>();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldAllBe(s => s.Group.Equals("Features", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAppSettingByKey_ValidKey_ReturnsSetting()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/AppSettings/General.ApplicationName");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<AppSettingDto>>();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Key.ShouldBe("General.ApplicationName");
        result.Data.DataType.ShouldBe("string");
    }

    [Fact]
    public async Task GetAppSettingByKey_NonExistentKey_ReturnsNotFound()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/AppSettings/Non.Existent.Key.123");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAppSetting_ValidSetting_UpdatesValueSuccessfully()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        // Toggle Features.ProductExport.Enabled to false
        using var putRequest = new HttpRequestMessage(HttpMethod.Put, "api/v1/AppSettings/Features.ProductExport.Enabled")
        {
            Content = JsonContent.Create(new { Value = "false" })
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var putResponse = await _client.SendAsync(putRequest);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify with GET
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, "api/v1/AppSettings/Features.ProductExport.Enabled");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var getResponse = await _client.SendAsync(getRequest);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<Result<AppSettingDto>>();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data!.Value.ShouldBe("false");
    }

    [Fact]
    public async Task UpdateAppSetting_ReadOnlySetting_ReturnsBadRequest()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        // System.Environment is read-only
        using var putRequest = new HttpRequestMessage(HttpMethod.Put, "api/v1/AppSettings/System.Environment")
        {
            Content = JsonContent.Create(new { Value = "Staging" })
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var putResponse = await _client.SendAsync(putRequest);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await putResponse.Content.ReadFromJsonAsync<Result>();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("read-only");
    }

    [Fact]
    public async Task UpdateAppSetting_InvalidDataType_ReturnsBadRequest()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");

        // Security.MaxFailedAccessAttempts requires a number
        using var putRequest = new HttpRequestMessage(HttpMethod.Put, "api/v1/AppSettings/Security.MaxFailedAccessAttempts")
        {
            Content = JsonContent.Create(new { Value = "not-a-number" })
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var putResponse = await _client.SendAsync(putRequest);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await putResponse.Content.ReadFromJsonAsync<Result>();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("numeric");
    }

    [Fact]
    public async Task UpdateAppSetting_AutomaticallyRecordsAuditLog()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");
        var updatedEmail = $"updated-{Guid.NewGuid():N}@onevision.local";

        // 1. Update General.SupportEmail
        using var putRequest = new HttpRequestMessage(HttpMethod.Put, "api/v1/AppSettings/General.SupportEmail")
        {
            Content = JsonContent.Create(new { Value = updatedEmail })
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var putResponse = await _client.SendAsync(putRequest);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 2. Query AuditLogs for AppSetting entity
        using var auditRequest = new HttpRequestMessage(HttpMethod.Get, "api/v1/AuditLogs?entityName=AppSetting&action=Updated");
        auditRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var auditResponse = await _client.SendAsync(auditRequest);
        auditResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var auditResult = await auditResponse.Content.ReadFromJsonAsync<Result<PaginatedList<AuditLogDto>>>();
        auditResult.ShouldNotBeNull();
        auditResult.IsSuccess.ShouldBeTrue();

        var log = auditResult.Data!.Items.FirstOrDefault(a => a.NewValues != null && a.NewValues.Contains(updatedEmail));
        log.ShouldNotBeNull();
        log.Action.ShouldBe("Updated");
        log.EntityName.ShouldBe("AppSetting");
        log.UserName.ShouldBe("testadmin@test.com");
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
