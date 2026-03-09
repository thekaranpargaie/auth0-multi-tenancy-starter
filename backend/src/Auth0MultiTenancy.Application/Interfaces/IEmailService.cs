namespace Auth0MultiTenancy.Application.Interfaces;

/// <summary>
/// Abstraction for sending transactional emails.
/// Implementing classes are infrastructure concerns (SMTP, SendGrid, etc.).
/// </summary>
public interface IEmailService
{
    Task SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        string organizationName,
        CancellationToken cancellationToken = default);

    Task SendInvitationEmailAsync(
        string toEmail,
        string organizationName,
        string role,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetLink,
        CancellationToken cancellationToken = default);
}
