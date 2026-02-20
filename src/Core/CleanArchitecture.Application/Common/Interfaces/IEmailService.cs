namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Contract for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends an email asynchronously.</summary>
    Task SendAsync(
        string to,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default);

    /// <summary>Sends an email to multiple recipients asynchronously.</summary>
    Task SendToManyAsync(
        IEnumerable<string> recipients,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default);
}
