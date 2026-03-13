import { defineStore } from 'pinia'
import { createAuth0Client } from '@auth0/auth0-spa-js'
import { auth0Config } from '@/config/auth'

/**
 * Central store for all Auth0 authentication state.
 * Exposes: isAuthenticated, user, orgId, orgDisplayName, loading.
 * Actions: init, login, logout, getToken.
 */
export const useAuthStore = defineStore('auth', {
  state: () => ({
    /** @type {import('@auth0/auth0-spa-js').Auth0Client | null} */
    client: null,
    isAuthenticated: false,
    /** @type {object | null} */
    user: null,
    orgId: /** @type {string | null} */ (null),
    orgDisplayName: /** @type {string | null} */ (null),
    /** @type {string[]} */
    permissions: [],
    /** @type {string | null} */
    accessToken: null,
    loading: true,
    error: /** @type {string | null} */ (null),
    /** Track if callback has been processed to avoid double-processing */
    callbackProcessed: false
  }),

  getters: {
    isAdmin: (state) => state.user?.['https://multi-tenant/roles']?.includes('Admin') ?? false,
    userEmail: (state) => state.user?.email ?? '',
    userName: (state) => state.user?.name ?? ''
  },

  actions: {
    async init() {
      try {
        this.client = await createAuth0Client({
          domain: auth0Config.domain,
          clientId: auth0Config.clientId,
          authorizationParams: {
            redirect_uri: auth0Config.redirectUri,
            audience: auth0Config.audience
          },
          useRefreshTokens: auth0Config.useRefreshTokens,
          cacheLocation: auth0Config.cacheLocation
        })

        // Handle redirect callback only once
        const hasAuthCode = window.location.search.includes('code=')
        const hasState = window.location.search.includes('state=')
        
        if (hasAuthCode && hasState && !this.callbackProcessed) {
          try {
            await this.client.handleRedirectCallback()
            this.callbackProcessed = true
            // Clean up URL after successful callback handling
            window.history.replaceState({}, document.title, window.location.pathname)
          } catch (callbackErr) {
            console.error('[AuthStore] Callback error:', callbackErr)
            this.error = `Callback error: ${callbackErr.message}`
            this.callbackProcessed = true // Mark as processed even on error to prevent retries
          }
        }

        this.isAuthenticated = await this.client.isAuthenticated()

        if (this.isAuthenticated) {
          this.user = await this.client.getUser()
          const claims = await this.client.getIdTokenClaims()
          this.orgId = claims?.org_id ?? null
          this.orgDisplayName = claims?.org_display_name ?? null

          // Fetch access token and decode permissions claim
          try {
            const token = await this.client.getTokenSilently({
              authorizationParams: { audience: auth0Config.audience }
            })
            this.accessToken = token
            const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
            this.permissions = Array.isArray(payload.permissions) ? payload.permissions : []
          } catch (tokenErr) {
            console.warn('[AuthStore] Could not fetch access token:', tokenErr)
          }
        }
      } catch (err) {
        this.error = err.message
        console.error('[AuthStore] Init error:', err)
      } finally {
        this.loading = false
      }
    },

    async login() {
      if (!this.client) return
      await this.client.loginWithRedirect()
    },

    async logout() {
      if (!this.client) return
      await this.client.logout({
        logoutParams: { returnTo: window.location.origin }
      })
    },

    async getToken() {
      if (!this.client) throw new Error('Auth0 client not initialized')
      return await this.client.getTokenSilently({
        authorizationParams: { audience: auth0Config.audience }
      })
    }
  }
})