/**
 * Auth0 configuration for CRM application.
 * All values come from Vite environment variables (VITE_* prefix).
 * Values are baked into the build at build time.
 */
export const auth0Config = {
  domain: import.meta.env.VITE_AUTH0_DOMAIN,
  clientId: import.meta.env.VITE_CRM_CLIENT_ID,
  audience: import.meta.env.VITE_AUTH0_AUDIENCE,
  redirectUri: import.meta.env.VITE_CRM_REDIRECT_URI || 'http://localhost:5174/callback',
  useRefreshTokens: true,
  cacheLocation: 'memory'
}