import { Auth0Provider } from '@auth0/auth0-react'

export const AuthProvider = ({ children }) => {
  const domain = import.meta.env.VITE_AUTH0_DOMAIN
  const clientId = import.meta.env.VITE_NOTES_CLIENT_ID
  const audience = import.meta.env.VITE_AUTH0_AUDIENCE
  const redirectUri = import.meta.env.VITE_NOTES_REDIRECT_URI || 'http://localhost:5175/callback'

  return (
    <Auth0Provider
      domain={domain}
      clientId={clientId}
      authorizationParams={{
        redirect_uri: redirectUri,
        audience: audience
      }}
    >
      {children}
    </Auth0Provider>
  )
}