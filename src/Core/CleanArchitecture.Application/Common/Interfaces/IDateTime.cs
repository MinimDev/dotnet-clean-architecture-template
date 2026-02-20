namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Abstraksi untuk DateTime service
/// Memudahkan testing dengan datetime yang bisa di-mock
/// </summary>
public interface IDateTime
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
