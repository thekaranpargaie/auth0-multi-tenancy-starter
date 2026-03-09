namespace Auth0MultiTenancy.Application.UseCases;

/// <summary>
/// Auth0 configuration values surfaced to the Application layer.
/// Populated from IOptions&lt;Auth0Settings&gt; and injected as a plain POCO
/// so the Application layer has no dependency on Microsoft.Extensions.*.
/// </summary>
public sealed record Auth0Settings
{
    public string Domain { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string AdminRoleId { get; init; } = string.Empty;
    public string MemberRoleId { get; init; } = string.Empty;
    public string AppClientId { get; init; } = string.Empty;
}
