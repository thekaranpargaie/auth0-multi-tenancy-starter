using Auth0MultiTenancy.Application.DTOs;
using Auth0MultiTenancy.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Auth0MultiTenancy.API.Controllers;

/// <summary>
/// Handles new organization + admin user signups.
/// This endpoint is intentionally unauthenticated — it's the entry point for new tenants.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class SignupController(SignupUseCase signupUseCase) : ControllerBase
{
    /// <summary>
    /// Creates a new Auth0 organization and its first admin user.
    /// The user will receive an email to set their password.
    /// </summary>
    /// <param name="request">Signup details including full name, email, and organization info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(SignupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Signup(
        [FromBody] SignupRequest request,
        CancellationToken cancellationToken)
    {
        var result = await signupUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Signup), result);
    }
}
