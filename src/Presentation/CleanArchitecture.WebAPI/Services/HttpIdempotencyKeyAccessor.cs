using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.WebAPI.Services;

/// <summary>
/// Implementasi <see cref="IIdempotencyKeyAccessor"/> untuk WebAPI.
/// Membaca nilai header HTTP <c>Idempotency-Key</c> dari request yang masuk via <see cref="IHttpContextAccessor"/>.
/// </summary>
public class HttpIdempotencyKeyAccessor : IIdempotencyKeyAccessor
{
    public const string HeaderName = "Idempotency-Key";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpIdempotencyKeyAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetIdempotencyKey()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return null;

        if (httpContext.Request.Headers.TryGetValue(HeaderName, out var keyValues))
        {
            var key = keyValues.FirstOrDefault();
            return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
        }

        return null;
    }
}
