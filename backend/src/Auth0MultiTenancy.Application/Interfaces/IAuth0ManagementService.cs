using Auth0MultiTenancy.Domain.Entities;

namespace Auth0MultiTenancy.Application.Interfaces;

/// <summary>
/// Abstraction over the Auth0 Management API.
/// All tenant/user management functionality is expressed through this contract,
/// keeping infrastructure details out of the Application and Domain layers.
/// </summary>
public interface IAuth0ManagementService
{
    /// <summary>Creates a new Auth0 organization.</summary>
    Task<Organization> CreateOrganizationAsync(
        string name,
        string displayName,
        string? domain,
        Dictionary<string, string>? metadata,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new Auth0 user with a temporary password.</summary>
    Task<User> CreateUserAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>Adds an existing user to an organization and assigns a role.</summary>
    Task AddUserToOrganizationAsync(
        string organizationId,
        string userId,
        string roleId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a password-reset ticket and returns the reset link.</summary>
    Task<string> SendPasswordResetAsync(
        string userId,
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all members of an organization.</summary>
    Task<IReadOnlyList<User>> GetOrganizationMembersAsync(
        string organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a user from an organization.</summary>
    Task RemoveUserFromOrganizationAsync(
        string organizationId,
        string userId,
        CancellationToken cancellationToken = default);
}
