using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.AuditLogs.Queries.GetAuditLogs;
using Shouldly;

namespace CleanArchitecture.Application.IntegrationTests;

public class AuditLogsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuditLogsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuditLogs_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("api/v1/AuditLogs");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogs_AsMember_ReturnsForbidden()
    {
        var token = await GetAccessTokenAsync("testmember@test.com", "Member123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/AuditLogs");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AuditLogs_ShouldAutomaticallyCapture_WhenProductIsCreatedUpdatedAndDeleted()
    {
        var token = await GetAccessTokenAsync("testadmin@test.com", "Admin123!");
        var uniqueProductName = $"AuditProduct-{Guid.NewGuid():N}";

        // 1. CREATE PRODUCT
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/Products")
        {
            Content = JsonContent.Create(new
            {
                Name = uniqueProductName,
                Price = 99.50,
                Description = "Product for audit trail test"
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createResponse = await _client.SendAsync(createRequest);
        ((int)createResponse.StatusCode).ShouldBeInRange(200, 201);

        var createResult = await createResponse.Content.ReadFromJsonAsync<Result<Guid>>();
        createResult.ShouldNotBeNull();
        createResult.IsSuccess.ShouldBeTrue();
        var productId = createResult.Data;

        // 2. VERIFY "Created" AUDIT LOG EXISTS
        using var auditGetRequest1 = new HttpRequestMessage(HttpMethod.Get, $"api/v1/AuditLogs?entityName=Product&action=Created");
        auditGetRequest1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var auditResponse1 = await _client.SendAsync(auditGetRequest1);
        auditResponse1.StatusCode.ShouldBe(HttpStatusCode.OK);

        var auditResult1 = await auditResponse1.Content.ReadFromJsonAsync<Result<PaginatedList<AuditLogDto>>>();
        auditResult1.ShouldNotBeNull();
        auditResult1.IsSuccess.ShouldBeTrue();

        var createdLog = auditResult1.Data!.Items.FirstOrDefault(a => a.PrimaryKey == productId.ToString());
        createdLog.ShouldNotBeNull();
        createdLog.Action.ShouldBe("Created");
        createdLog.EntityName.ShouldBe("Product");
        createdLog.NewValues.ShouldNotBeNull();
        createdLog.NewValues.ShouldContain(uniqueProductName);
        createdLog.UserName.ShouldBe("testadmin@test.com");

        // 3. UPDATE PRODUCT
        var updatedProductName = $"{uniqueProductName}-Updated";
        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"api/v1/Products/{productId}")
        {
            Content = JsonContent.Create(new
            {
                Name = updatedProductName,
                Price = 149.99,
                Description = "Updated description",
                Stock = 15
            })
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateResponse = await _client.SendAsync(updateRequest);
        ((int)updateResponse.StatusCode).ShouldBeInRange(200, 204);

        // 4. VERIFY "Updated" AUDIT LOG EXISTS
        using var auditGetRequest2 = new HttpRequestMessage(HttpMethod.Get, $"api/v1/AuditLogs?entityName=Product&action=Updated");
        auditGetRequest2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var auditResponse2 = await _client.SendAsync(auditGetRequest2);
        auditResponse2.StatusCode.ShouldBe(HttpStatusCode.OK);

        var auditResult2 = await auditResponse2.Content.ReadFromJsonAsync<Result<PaginatedList<AuditLogDto>>>();
        auditResult2.ShouldNotBeNull();
        auditResult2.IsSuccess.ShouldBeTrue();

        var updatedLog = auditResult2.Data!.Items.FirstOrDefault(a => a.PrimaryKey == productId.ToString());
        updatedLog.ShouldNotBeNull();
        updatedLog.Action.ShouldBe("Updated");
        updatedLog.OldValues.ShouldNotBeNull();
        updatedLog.NewValues.ShouldNotBeNull();
        updatedLog.OldValues.ShouldContain(uniqueProductName);
        updatedLog.NewValues.ShouldContain(updatedProductName);
        updatedLog.AffectedColumns.ShouldNotBeNull();
        updatedLog.AffectedColumns.ShouldContain("Name");

        // 5. DELETE PRODUCT (Soft-delete)
        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/Products/{productId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        ((int)deleteResponse.StatusCode).ShouldBeInRange(200, 204);

        // 6. VERIFY "Deleted" AUDIT LOG EXISTS
        using var auditGetRequest3 = new HttpRequestMessage(HttpMethod.Get, $"api/v1/AuditLogs?entityName=Product&action=Deleted");
        auditGetRequest3.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var auditResponse3 = await _client.SendAsync(auditGetRequest3);
        auditResponse3.StatusCode.ShouldBe(HttpStatusCode.OK);

        var auditResult3 = await auditResponse3.Content.ReadFromJsonAsync<Result<PaginatedList<AuditLogDto>>>();
        auditResult3.ShouldNotBeNull();
        auditResult3.IsSuccess.ShouldBeTrue();

        var deletedLog = auditResult3.Data!.Items.FirstOrDefault(a => a.PrimaryKey == productId.ToString());
        deletedLog.ShouldNotBeNull();
        deletedLog.Action.ShouldBe("Deleted");
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
