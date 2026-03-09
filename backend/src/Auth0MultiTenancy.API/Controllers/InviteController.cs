using System.Security.Claims;
using Auth0MultiTenancy.Application.DTOs;
using Auth0MultiTenancy.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth0MultiTenancy.API.Controllers;

/// <summary>
/// Handles user invitations within an existing organization.
/// Requires a valid JWT with an organization context (org_id claim).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class InviteController(InviteUserUseCase inviteUseCase) : ControllerBase
{
    /// <summary>
    /// Invites a user to an organization with a specified role.
    /// Caller must belong to the target organization.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InviteUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InviteUser(
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new UnauthorizedAccessException("Cannot determine caller identity.");

        var orgId = User.FindFirstValue("org_id")
                    ?? throw new UnauthorizedAccessException("Token does not contain an org_id claim.");

        var result = await inviteUseCase.ExecuteAsync(request, userId, orgId, cancellationToken);
        return CreatedAtAction(nameof(InviteUser), result);
    }
}
