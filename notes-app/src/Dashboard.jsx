import { useAuth0 } from '@auth0/auth0-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'

function Dashboard() {
  const { user, isAuthenticated, logout, getAccessTokenWithSilent } = useAuth0()
  const [token, setToken] = useState(null)
  const [decodedToken, setDecodedToken] = useState(null)
  const [showFullToken, setShowFullToken] = useState(false)
  const [copied, setCopied] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)

  const navigate = useNavigate()

  const fetchToken = async () => {
    setLoading(true)
    setError(null)
    try {
      const accessToken = await getAccessTokenWithSilent({
        audience: import.meta.env.VITE_AUTH0_AUDIENCE
      })
      setToken(accessToken)

      // Decode and display token payload
      const payload = JSON.parse(atob(accessToken.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
      setDecodedToken(payload)
    } catch (err) {
      setError(`Failed to fetch token: ${err.message}`)
    } finally {
      setLoading(false)
    }
  }

  const copyTokenToClipboard = async () => {
    if (token) {
      try {
        await navigator.clipboard.writeText(token)
        setCopied(true)
        setTimeout(() => setCopied(false), 3000)
      } catch (err) {
        console.error('Failed to copy token:', err)
      }
    }
  }

  const handleLogout = () => {
    logout({
      logoutParams: {
        returnTo: window.location.origin
      }
    })
  }

  if (!isAuthenticated) {
    return (
      <div className="error">
        You are not authenticated. Please log in.
      </div>
    )
  }

  return (
    <div className="dashboard">
      <div className="header-actions">
        <button className="logout-button" onClick={handleLogout}>Logout</button>
      </div>

      <div className="card">
        <h2>User Information</h2>
        <div className="user-info">
          <p><strong>Name:</strong> {user?.name}</p>
          <p><strong>Email:</strong> {user?.email}</p>
          <p><strong>Email Verified:</strong> {user?.email_verified ? 'Yes' : 'No'}</p>
          <p><strong>Organization ID:</strong> {user?.['https://multi-tenant/org_id']}</p>
          <p><strong>Organization Name:</strong> {user?.['https://multi-tenant/org_display_name']}</p>
          <p><strong>Is Admin:</strong> {user?.['https://multi-tenant/roles']?.includes('Admin') ? 'Yes' : 'No'}</p>
        </div>
      </div>

      <div className="card">
        <h2>JWT Token Details</h2>
        <button onClick={fetchToken} disabled={loading}>
          {loading ? 'Fetching...' : 'Fetch Access Token'}
        </button>
        {error && <div className="error">{error}</div>}
        {decodedToken && (
          <div className="token-info">
            <h3>Decoded Token Payload</h3>
            <pre>{JSON.stringify(decodedToken, null, 2)}</pre>
          </div>
        )}
      </div>

      <div className="card">
        <h2>JWT Access Token</h2>
        <div className="token-actions">
          <button onClick={copyTokenToClipboard} disabled={!token}>
            Copy Token
          </button>
          <button onClick={() => setShowFullToken(!showFullToken)} className="secondary">
            {showFullToken ? 'Hide Full Token' : 'Show Full Token'}
          </button>
        </div>
        {showFullToken && token && (
          <textarea className="token-display" readOnly value={token} rows="10" />
        )}
        {copied && <div className="success">Token copied to clipboard!</div>}
      </div>
    </div>
  )
}

export default Dashboard