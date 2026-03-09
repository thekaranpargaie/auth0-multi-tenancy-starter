using System.Net;
using System.Net.Mail;
using Auth0MultiTenancy.Application.Interfaces;
using Auth0MultiTenancy.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth0MultiTenancy.Infrastructure.Email;

/// <summary>
/// SMTP implementation of <see cref="IEmailService"/>.
/// If credentials are not configured the service logs a warning and skips sending.
/// </summary>
public sealed class SmtpEmailService(
    IOptions<SmtpOptions> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpOptions _opts = options.Value;

    public async Task SendWelcomeEmailAsync(
        string toEmail, string toName, string organizationName, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured()) return;

        var body = $"""
            Hi {toName},

            Welcome to {organizationName}! Your account has been created.

            Please check your inbox for a separate email to set your password,
            then visit the application to get started.

            Best regards,
            The {organizationName} Team
            """;

        await SendAsync(toEmail, toName, $"Welcome to {organizationName}", body, cancellationToken);
    }

    public async Task SendInvitationEmailAsync(
        string toEmail, string organizationName, string role, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured()) return;

        var body = $"""
            Hello,

            You have been invited to join {organizationName} as a {role}.

            You will receive a separate email to set your password.
            Once done, you can log in and access the organization dashboard.

            Best regards,
            The {organizationName} Team
            """;

        await SendAsync(toEmail, toEmail, $"You're invited to {organizationName}", body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail, string toName, string resetLink, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured()) return;

        var body = $"""
            Hi {toName},

            Click the link below to set your password:

            {resetLink}

            This link expires in 24 hours.

            If you didn't request a password reset, please ignore this email.

            Best regards,
            The Team
            """;

        await SendAsync(toEmail, toName, "Set Your Password", body, cancellationToken);
    }

    // ── Internal ────────────────────────────────────────────────────────────

    private bool IsConfigured()
    {
        if (!string.IsNullOrEmpty(_opts.Username)) return true;
        logger.LogWarning("SMTP not configured — skipping email send.");
        return false;
    }

    private async Task SendAsync(
        string toEmail, string toName, string subject, string body, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient(_opts.Server, _opts.Port)
            {
                Credentials = new NetworkCredential(_opts.Username, _opts.Password),
                EnableSsl = _opts.UseTls
            };

            var message = new MailMessage
            {
                From = new MailAddress(_opts.FromEmail, _opts.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}
