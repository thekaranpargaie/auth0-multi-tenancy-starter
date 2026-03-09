namespace Auth0MultiTenancy.Domain.Entities;

/// <summary>
/// Represents a user managed through Auth0.
/// </summary>
public sealed class User
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }

    private User() { }

    public static User Create(
        string userId,
        string email,
        string name,
        bool emailVerified = true) =>
        new()
        {
            UserId = userId,
            Email = email,
            Name = name,
            EmailVerified = emailVerified
        };
}
