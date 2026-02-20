using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Shared.Services;

/// <summary>
/// SMTP email service implementation using System.Net.Mail.
/// Configure via appsettings.json under "Email" section.
/// </summary>
public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _settings = configuration.GetSection("Email").Get<EmailSettings>()
            ?? throw new InvalidOperationException("Email settings not configured.");
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
        => await SendToManyAsync([to], subject, body, isHtml, cancellationToken);

    public async Task SendToManyAsync(
        IEnumerable<string> recipients,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        using var client = new System.Net.Mail.SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new System.Net.NetworkCredential(_settings.UserName, _settings.Password)
        };

        using var message = new System.Net.Mail.MailMessage
        {
            From = new System.Net.Mail.MailAddress(_settings.From, _settings.DisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        foreach (var recipient in recipients)
            message.To.Add(recipient);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {Recipients}", string.Join(", ", recipients));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", recipients));
            throw;
        }
    }
}

/// <summary>
/// Email configuration settings
/// </summary>
public sealed record EmailSettings
{
    public string Host { get; init; } = "smtp.gmail.com";
    public int Port { get; init; } = 587;
    public bool EnableSsl { get; init; } = true;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string DisplayName { get; init; } = "Clean Architecture App";
}
