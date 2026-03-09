using Auth0MultiTenancy.Application.Interfaces;
using Auth0MultiTenancy.Application.UseCases;
using Auth0MultiTenancy.Infrastructure.Auth0;
using Auth0MultiTenancy.Infrastructure.Configuration;
using Auth0MultiTenancy.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth0MultiTenancy.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// Keeps the API project clean and unaware of concrete implementations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Options ──────────────────────────────────────────────────────────
        services.Configure<Auth0Options>(configuration.GetSection(Auth0Options.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        // ── HTTP clients ─────────────────────────────────────────────────────
        services.AddHttpClient("Auth0Token")
                .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));

        services.AddHttpClient("Auth0Management")
                .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));

        // ── Caching ──────────────────────────────────────────────────────────
        services.AddMemoryCache();

        // ── Infrastructure services ──────────────────────────────────────────
        services.AddSingleton<Auth0TokenCache>();
        services.AddScoped<IAuth0ManagementService, Auth0ManagementService>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        // ── Application use-cases ────────────────────────────────────────────
        // Register the settings POCO used by use-cases (mapped from Auth0Options)
        services.AddScoped<Auth0Settings>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Auth0Options>>().Value;
            return new Auth0Settings
            {
                Domain = opts.Domain,
                Audience = opts.Audience,
                AdminRoleId = opts.AdminRoleId,
                MemberRoleId = opts.MemberRoleId,
                AppClientId = opts.AppClientId
            };
        });

        services.AddScoped<SignupUseCase>();
        services.AddScoped<InviteUserUseCase>();

        return services;
    }
}
