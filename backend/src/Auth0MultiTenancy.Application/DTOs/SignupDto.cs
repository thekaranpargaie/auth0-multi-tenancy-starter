using System.ComponentModel.DataAnnotations;

namespace Auth0MultiTenancy.Application.DTOs;

/// <summary>
/// Request payload for creating a new organization and its first admin user.
/// </summary>
public sealed record SignupRequest
{
    [Required, MinLength(2), MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(2), MaxLength(100)]
    public string OrganizationName { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? OrganizationDomain { get; init; }
}

/// <summary>
/// Response returned after a successful signup.
/// </summary>
public sealed record SignupResponse(string Message);
