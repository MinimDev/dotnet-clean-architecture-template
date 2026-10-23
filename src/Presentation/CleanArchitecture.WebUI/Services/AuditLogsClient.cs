using System.Net.Http.Json;
using CleanArchitecture.WebUI.Services.Models;

namespace CleanArchitecture.WebUI.Services;

public class AuditLogsClient
{
    private readonly AuthorizedHttpClient _httpClient;
    private readonly TokenProvider _tokenProvider;

    public AuditLogsClient(
        AuthorizedHttpClient httpClient,
        TokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<PaginatedList<AuditLogDto>>> GetAuditLogsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? entityName = null,
        string? action = null,
        string? userId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var queryParams = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(entityName))
            queryParams.Add($"entityName={Uri.EscapeDataString(entityName)}");

        if (!string.IsNullOrWhiteSpace(action))
            queryParams.Add($"action={Uri.EscapeDataString(action)}");

        if (!string.IsNullOrWhiteSpace(userId))
            queryParams.Add($"userId={Uri.EscapeDataString(userId)}");

        if (dateFrom.HasValue)
            queryParams.Add($"dateFrom={Uri.EscapeDataString(dateFrom.Value.ToString("o"))}");

        if (dateTo.HasValue)
            queryParams.Add($"dateTo={Uri.EscapeDataString(dateTo.Value.ToString("o"))}");

        var url = $"api/v1/AuditLogs?{string.Join("&", queryParams)}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var tokenStatus = string.IsNullOrEmpty(_tokenProvider.Token) ? "NO TOKEN" : "token present";
                return new Result<PaginatedList<AuditLogDto>>
                {
                    IsSuccess = false,
                    Error = $"HTTP {(int)response.StatusCode} ({tokenStatus})"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<Result<PaginatedList<AuditLogDto>>>();
            return result ?? new Result<PaginatedList<AuditLogDto>> { IsSuccess = false, Error = "Empty response" };
        }
        catch (Exception ex)
        {
            return new Result<PaginatedList<AuditLogDto>> { IsSuccess = false, Error = ex.Message };
        }
    }
}
