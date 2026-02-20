namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Abstraksi untuk mendapatkan informasi user yang sedang login
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
