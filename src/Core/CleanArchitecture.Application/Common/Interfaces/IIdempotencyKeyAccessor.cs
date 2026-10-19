namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Abstraksi untuk membaca Idempotency-Key dari konteks request saat ini.
/// Mengikuti prinsip Clean Architecture: Application layer tidak memiliki ketergantungan
/// langsung pada ASP.NET Core / HttpContext, melainkan melalui abstraksi ini.
/// Implementasi nyata (<c>HttpIdempotencyKeyAccessor</c>) disediakan oleh layer Presentasi (WebAPI).
/// </summary>
public interface IIdempotencyKeyAccessor
{
    /// <summary>
    /// Mengambil idempotency key dari header request atau konteks saat ini.
    /// Mengembalikan null jika key tidak disediakan atau bukan HTTP request.
    /// </summary>
    string? GetIdempotencyKey();
}

/// <summary>
/// Default fallback implementation saat tidak ada context HTTP (misal: console runner, unit test, background worker).
/// </summary>
public class NullIdempotencyKeyAccessor : IIdempotencyKeyAccessor
{
    public string? GetIdempotencyKey() => null;
}
