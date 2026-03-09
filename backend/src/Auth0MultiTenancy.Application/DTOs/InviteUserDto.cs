using System.ComponentModel.DataAnnotations;

namespace Auth0MultiTenancy.Application.DTOs;

/// <summary>
/// Roles a user can be invited as within an organization.
/// </summary>
public enum OrganizationRole { Member, Admin }

/// <summary>
/// Request payload for inviting a user to an existing organization.
/// </summary>
public sealed record InviteUserRequest
{
    [Required]
    public string OrganizationId { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    public OrganizationRole Role { get; init; } = OrganizationRole.Member;
}

/// <summary>
/// Response returned after a successful user invitation.
/// </summary>
public sealed record InviteUserResponse(string Message);
