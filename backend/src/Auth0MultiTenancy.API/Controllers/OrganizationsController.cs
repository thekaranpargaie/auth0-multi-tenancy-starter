using System.Security.Claims;
using Auth0MultiTenancy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth0MultiTenancy.API.Controllers;

/// <summary>
/// Read/write access to organization membership.
/// Caller must be an authenticated member of the organization.
/// </summary>
[ApiController]
[Route("api/organizations")]
[Authorize]
public sealed class OrganizationsController(IAuth0ManagementService auth0) : ControllerBase
{
    private string CallerOrgId =>
        User.FindFirstValue("org_id")
        ?? throw new UnauthorizedAccessException("Token does not contain an org_id claim.");

    /// <summary>Returns all members of the caller's organization.</summary>
    [HttpGet("{organizationId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMembers(string organizationId, CancellationToken ct)
    {
        EnsureOrgAccess(organizationId);
        var members = await auth0.GetOrganizationMembersAsync(organizationId, ct);
        return Ok(members);
    }

    /// <summary>Removes a user from the caller's organization.</summary>
    [HttpDelete("{organizationId}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveMember(string organizationId, string userId, CancellationToken ct)
    {
        EnsureOrgAccess(organizationId);
        await auth0.RemoveUserFromOrganizationAsync(organizationId, userId, ct);
        return NoContent();
    }

    private void EnsureOrgAccess(string organizationId)
    {
        if (CallerOrgId != organizationId)
            throw new UnauthorizedAccessException($"Caller does not have access to organization '{organizationId}'.");
    }
}
