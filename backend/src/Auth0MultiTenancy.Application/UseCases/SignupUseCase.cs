using Auth0MultiTenancy.Application.DTOs;
using Auth0MultiTenancy.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth0MultiTenancy.Application.UseCases;

/// <summary>
/// Orchestrates the full signup workflow:
/// 1. Create Auth0 organization
/// 2. Create the admin user
/// 3. Add user to organization as Admin
/// 4. Trigger password-reset email
/// 5. (Optional) Send welcome email
/// </summary>
public sealed class SignupUseCase(
    IAuth0ManagementService auth0,
    IEmailService emailService,
    Auth0Settings settings,
    ILogger<SignupUseCase> logger)
{
    public async Task<SignupResponse> ExecuteAsync(
        SignupRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting signup for {Email}, org: {Org}", request.Email, request.OrganizationName);

        // 1. Derive a slug for the org from the domain or name
        var orgSlug = DeriveSlug(request.OrganizationDomain, request.OrganizationName);

        // 2. Create organization
        var org = await auth0.CreateOrganizationAsync(
            name: orgSlug,
            displayName: request.OrganizationName,
            domain: request.OrganizationDomain,
            metadata: new Dictionary<string, string> { ["created_by"] = "signup_api" },
            cancellationToken: cancellationToken);

        logger.LogInformation("Organization {OrgId} created", org.Id);

        // 3. Create user
        var user = await auth0.CreateUserAsync(
            email: request.Email,
            name: request.Name,
            cancellationToken: cancellationToken);

        logger.LogInformation("User {UserId} created", user.UserId);

        // 4. Add user to organization as Admin
        await auth0.AddUserToOrganizationAsync(
            organizationId: org.Id,
            userId: user.UserId,
            roleId: settings.AdminRoleId,
            cancellationToken: cancellationToken);

        // 5. Create password reset ticket and send email
        var resetLink = await auth0.SendPasswordResetAsync(user.UserId, request.Email, cancellationToken);

        try
        {
            await emailService.SendPasswordResetEmailAsync(
                request.Email,
                request.Name,
                resetLink,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Password reset email failed but user created for {Email}", request.Email);
        }

        // 6. Optional welcome email (does not fail the operation if SMTP is unconfigured)
        try
        {
            await emailService.SendWelcomeEmailAsync(request.Email, request.Name, request.OrganizationName, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Welcome email failed but signup completed for {Email}", request.Email);
        }

        return new SignupResponse("Organization and user created successfully. Please check your email to set your password.");
    }

    private static string DeriveSlug(string? domain, string displayName)
    {
        var source = !string.IsNullOrWhiteSpace(domain)
            ? domain.Split('.')[0]
            : displayName;

        return new string(source
            .ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray())
            .Trim('-');
    }
}
