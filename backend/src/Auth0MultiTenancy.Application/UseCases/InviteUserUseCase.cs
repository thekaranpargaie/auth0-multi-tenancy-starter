using Auth0MultiTenancy.Application.DTOs;
using Auth0MultiTenancy.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth0MultiTenancy.Application.UseCases;

/// <summary>
/// Orchestrates the user invitation workflow:
/// 1. Create the user in Auth0
/// 2. Add user to organization with the requested role
/// 3. Trigger password-reset email
/// 4. (Optional) Send invitation welcome email
/// </summary>
public sealed class InviteUserUseCase(
    IAuth0ManagementService auth0,
    IEmailService emailService,
    Auth0Settings settings,
    ILogger<InviteUserUseCase> logger)
{
    public async Task<InviteUserResponse> ExecuteAsync(
        InviteUserRequest request,
        string callerUserId,
        string callerOrgId,
        CancellationToken cancellationToken = default)
    {
        // Guard: caller must be operating within the target organization
        if (callerOrgId != request.OrganizationId)
        {
            throw new UnauthorizedAccessException(
                $"Caller org '{callerOrgId}' does not match target org '{request.OrganizationId}'.");
        }

        logger.LogInformation("Inviting {Email} to org {OrgId} as {Role}", request.Email, request.OrganizationId, request.Role);

        var roleId = request.Role == OrganizationRole.Admin
            ? settings.AdminRoleId
            : settings.MemberRoleId;

        // 1. Create user
        var displayName = request.Email.Split('@')[0];
        var user = await auth0.CreateUserAsync(request.Email, displayName, cancellationToken);

        // 2. Add to organization
        await auth0.AddUserToOrganizationAsync(
            organizationId: request.OrganizationId,
            userId: user.UserId,
            roleId: roleId,
            cancellationToken: cancellationToken);

        // 3. Create password reset ticket and send email
        var resetLink = await auth0.SendPasswordResetAsync(user.UserId, request.Email, cancellationToken);

        try
        {
            await emailService.SendPasswordResetEmailAsync(
                request.Email,
                displayName,
                resetLink,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Password reset email failed but user created for {Email}", request.Email);
        }

        // 4. Optional invitation email
        try
        {
            await emailService.SendInvitationEmailAsync(request.Email, request.OrganizationId, request.Role.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invitation email failed but invite completed for {Email}", request.Email);
        }

        return new InviteUserResponse($"User {request.Email} invited successfully. Password reset link sent to their email.");
    }
}
