/**
 * Auth0 configuration.
 * All values come from Vite environment variables (VITE_* prefix).
 * Values are baked into the build at build time.
 */
export const auth0Config = {
  domain: import.meta.env.VITE_AUTH0_DOMAIN,
  clientId: import.meta.env.VITE_AUTH0_CLIENT_ID,
  audience: import.meta.env.VITE_AUTH0_AUDIENCE,
  redirectUri: `${window.location.origin}/callback`,
  useRefreshTokens: true,
  cacheLocation: 'memory'
}
