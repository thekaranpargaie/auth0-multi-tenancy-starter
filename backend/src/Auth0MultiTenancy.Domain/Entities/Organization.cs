namespace Auth0MultiTenancy.Domain.Entities;

/// <summary>
/// Represents an Auth0 Organization (tenant).
/// </summary>
public sealed class Organization
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;        // Internal slug name (e.g. "acme")
    public string DisplayName { get; init; } = string.Empty; // Human-readable (e.g. "Acme Corp")
    public string? Domain { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = [];

    private Organization() { }

    public static Organization Create(
        string id,
        string name,
        string displayName,
        string? domain = null,
        Dictionary<string, string>? metadata = null) =>
        new()
        {
            Id = id,
            Name = name,
            DisplayName = displayName,
            Domain = domain,
            Metadata = metadata ?? []
        };
}
