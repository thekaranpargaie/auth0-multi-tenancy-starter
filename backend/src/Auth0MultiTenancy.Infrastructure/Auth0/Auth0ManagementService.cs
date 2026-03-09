using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Auth0MultiTenancy.Application.Interfaces;
using Auth0MultiTenancy.Domain.Entities;
using Auth0MultiTenancy.Domain.Exceptions;
using Auth0MultiTenancy.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth0MultiTenancy.Infrastructure.Auth0;

/// <summary>
/// Production implementation of <see cref="IAuth0ManagementService"/>.
/// All Auth0 Management API calls are made here; the rest of the application
/// never knows about HTTP or Auth0-specific JSON payloads.
/// </summary>
public sealed class Auth0ManagementService(
    IHttpClientFactory httpClientFactory,
    Auth0TokenCache tokenCache,
    IOptions<Auth0Options> options,
    ILogger<Auth0ManagementService> logger) : IAuth0ManagementService
{
    private readonly Auth0Options _opts = options.Value;
    private static readonly JsonSerializerOptions JsonOpts = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    // ── Helper ──────────────────────────────────────────────────────────────

    private async Task<HttpClient> CreateMgmtClientAsync(CancellationToken ct)
    {
        var token = await tokenCache.GetTokenAsync(ct);
        var client = httpClientFactory.CreateClient("Auth0Management");
        client.BaseAddress = new Uri($"https://{_opts.Domain}/api/v2/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string context)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();
        throw response.StatusCode switch
        {
            HttpStatusCode.Conflict => new UserAlreadyExistsException(context),
            HttpStatusCode.NotFound => new OrganizationNotFoundException(context),
            _ => new HttpRequestException($"Auth0 API error [{response.StatusCode}] {context}: {body}")
        };
    }

    // ── IAuth0ManagementService ──────────────────────────────────────────────

    public async Task<Organization> CreateOrganizationAsync(
        string name, string displayName, string? domain,
        Dictionary<string, string>? metadata, CancellationToken ct = default)
    {
        var client = await CreateMgmtClientAsync(ct);

        var payload = new
        {
            name,
            display_name = displayName,
            metadata = metadata ?? new Dictionary<string, string>()
        };

        var response = await client.PostAsJsonAsync("organizations", payload, JsonOpts, ct);
        await EnsureSuccessAsync(response, name);

        var org = await response.Content.ReadFromJsonAsync<Auth0Organization>(ct)
                  ?? throw new InvalidOperationException("Null organization response.");

        logger.LogDebug("Created organization {Id}", org.Id);

        // Attach the database connection to the new organization so users can log in
        if (!string.IsNullOrEmpty(_opts.ConnectionId))
        {
            var connPayload = new
            {
                connection_id = _opts.ConnectionId,
                assign_membership_on_login = false
            };
            var connResponse = await client.PostAsJsonAsync(
                $"organizations/{org.Id}/enabled_connections", connPayload, JsonOpts, ct);
            await EnsureSuccessAsync(connResponse, org.Id);
            logger.LogDebug("Enabled connection {ConnectionId} on organization {OrgId}", _opts.ConnectionId, org.Id);
        }
        else
        {
            logger.LogWarning("No ConnectionId configured — skipping connection attachment for org {OrgId}", org.Id);
        }

        return Organization.Create(org.Id, org.Name, org.DisplayName, domain);
    }

    public async Task<User> CreateUserAsync(string email, string name, CancellationToken ct = default)
    {
        var client = await CreateMgmtClientAsync(ct);

        var payload = new
        {
            email,
            name,
            password = GenerateTemporaryPassword(),
            connection = _opts.Connection,
            email_verified = true
        };

        var response = await client.PostAsJsonAsync("users", payload, JsonOpts, ct);
        await EnsureSuccessAsync(response, email);

        var user = await response.Content.ReadFromJsonAsync<Auth0User>(ct)
                   ?? throw new InvalidOperationException("Null user response.");

        logger.LogDebug("Created user {UserId}", user.UserId);

        return User.Create(user.UserId, email, name, emailVerified: true);
    }

    public async Task AddUserToOrganizationAsync(
        string organizationId, string userId, string roleId, CancellationToken ct = default)
    {
        var client = await CreateMgmtClientAsync(ct);

        // Step 1: add member
        var addMemberPayload = new { members = new[] { userId } };
        var memberResponse = await client.PostAsJsonAsync(
            $"organizations/{organizationId}/members", addMemberPayload, JsonOpts, ct);
        await EnsureSuccessAsync(memberResponse, organizationId);

        // Step 2: assign role
        var rolePayload = new { roles = new[] { roleId } };
        var roleResponse = await client.PostAsJsonAsync(
            $"organizations/{organizationId}/members/{userId}/roles", rolePayload, JsonOpts, ct);
        await EnsureSuccessAsync(roleResponse, $"{organizationId}/{userId}");

        logger.LogDebug("User {UserId} added to org {OrgId} with role {RoleId}", userId, organizationId, roleId);
    }

    public async Task<string> SendPasswordResetAsync(string userId, string email, CancellationToken ct = default)
    {
        var client = await CreateMgmtClientAsync(ct);

        var payload = new
        {
            user_id = userId,
            client_id = string.IsNullOrEmpty(_opts.AppClientId) ? null : _opts.AppClientId,
            ttl_sec = 86400,
            mark_email_as_verified = true
        };

        var response = await client.PostAsJsonAsync("tickets/password-change", payload, JsonOpts, ct);
        await EnsureSuccessAsync(response, email);

        var ticketResponse = await response.Content.ReadFromJsonAsync<Auth0PasswordResetTicket>(ct)
                             ?? throw new InvalidOperationException("Null password reset ticket response.");

        logger.LogDebug("Password reset ticket created for {Email}", email);

        return ticketResponse.Ticket;
    }

    public async Task<IReadOnlyList<User>> GetOrganizationMembersAsync(
        string organizationId, CancellationToken ct = default)
    {
        var client = await CreateMgmtClientAsync(ct);

        var response = await client.GetAsync($"organizations/{organizationId}/members?include_totals=false", ct);
        await EnsureSuccessAsync(response, organizationId);

        var result = await response.Content.ReadFromJsonAsync<Auth0OrgMemberPage>(ct);
        return result?.Members.Select(m => User.Create(m.UserId, m.Email, m.Name)).ToList().AsReadOnly()
               ?? (IReadOnlyList<User>)[];
    }

    public async Task RemoveUserFromOrganizationAsync(
        string organizationId, string userId, CancellationToken ct = default)
    {
        var client = await CreateMgmtClientAsync(ct);

        var payload = new { members = new[] { userId } };
        var request = new HttpRequestMessage(HttpMethod.Delete, $"organizations/{organizationId}/members")
        {
            Content = JsonContent.Create(payload, options: JsonOpts)
        };

        var response = await client.SendAsync(request, ct);
        await EnsureSuccessAsync(response, $"{organizationId}/{userId}");

        logger.LogDebug("User {UserId} removed from org {OrgId}", userId, organizationId);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string GenerateTemporaryPassword()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"Tmp!{guid[..12]}Aa1";
    }

    // ── Internal response DTOs ────────────────────────────────────────────────

    private sealed record Auth0Organization(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("display_name")] string DisplayName);

    private sealed record Auth0User(
        [property: JsonPropertyName("user_id")] string UserId,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string Name);

    private sealed record Auth0OrgMember(
        [property: JsonPropertyName("user_id")] string UserId,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string Name);

    private sealed record Auth0OrgMemberPage(
        [property: JsonPropertyName("members")] List<Auth0OrgMember> Members);

    private sealed record Auth0PasswordResetTicket(
        [property: JsonPropertyName("ticket")] string Ticket);
}
