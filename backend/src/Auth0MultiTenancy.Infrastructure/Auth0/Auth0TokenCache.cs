using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Auth0MultiTenancy.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth0MultiTenancy.Infrastructure.Auth0;

/// <summary>
/// Fetches and caches a Machine-to-Machine access token
/// for the Auth0 Management API using the client_credentials grant.
/// Implements the Singleton pattern safely for token reuse with early expiry buffer.
/// </summary>
public sealed class Auth0TokenCache(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IOptions<Auth0Options> options,
    ILogger<Auth0TokenCache> logger)
{
    private const string CacheKey = "auth0_m2m_token";
    private const int ExpiryBufferSeconds = 60;

    private readonly Auth0Options _opts = options.Value;

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
            return cached;

        logger.LogInformation("Fetching new M2M token from Auth0");

        using var client = httpClientFactory.CreateClient("Auth0Token");
        var tokenUrl = $"https://{_opts.Domain}/oauth/token";

        var payload = new
        {
            client_id = _opts.M2MClientId,
            client_secret = _opts.M2MClientSecret,
            audience = $"https://{_opts.Domain}/api/v2/",
            grant_type = "client_credentials"
        };

        var response = await client.PostAsJsonAsync(tokenUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
                            ?? throw new InvalidOperationException("Empty token response from Auth0.");

        var expiry = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - ExpiryBufferSeconds);
        cache.Set(CacheKey, tokenResponse.AccessToken, expiry);

        return tokenResponse.AccessToken;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
