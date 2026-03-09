namespace Auth0MultiTenancy.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed configuration section for Auth0.
/// Binds to the "Auth0" section of appsettings.json / environment variables.
/// </summary>
public sealed class Auth0Options
{
    public const string SectionName = "Auth0";

    public string Domain { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // M2M (backend-to-Auth0 Management API)
    public string M2MClientId { get; set; } = string.Empty;
    public string M2MClientSecret { get; set; } = string.Empty;

    // SPA Client (used by backend when building org-login URLs, etc.)
    public string AppClientId { get; set; } = string.Empty;

    // Organization roles
    public string AdminRoleId { get; set; } = string.Empty;
    public string MemberRoleId { get; set; } = string.Empty;

    // Auth0 database connection name
    public string Connection { get; set; } = "Username-Password-Authentication";

    // Auth0 database connection ID (e.g., con_xyz...)
    public string ConnectionId { get; set; } = string.Empty;
}

/// <summary>
/// Strongly-typed email/SMTP configuration.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@example.com";
    public string FromName { get; set; } = "Auth0 Multi-Tenant";
    public bool UseTls { get; set; } = true;
}
