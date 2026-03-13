import { useAuth0 } from '@auth0/auth0-react'

function Home() {
  const { isLoading, error, isAuthenticated, loginWithRedirect } = useAuth0()

  if (isLoading) {
    return <div className="loading">Loading...</div>
  }

  if (error) {
    return <div className="error">{error.message}</div>
  }

  return (
    <div className="home">
      <h2>Welcome to Notes App</h2>
      <p>This is a test application to demonstrate Auth0 SSO between multiple applications.</p>
      {!isAuthenticated && (
        <div>
          <p>You are not logged in. Please login to access the application.</p>
          <button className="auth-button" onClick={() => loginWithRedirect()}>
            Login
          </button>
        </div>
      )}
      {isAuthenticated && (
        <div>
          <p>You are logged in! You can now access the dashboard.</p>
          <a href="/dashboard" className="auth-button">Go to Dashboard</a>
        </div>
      )}
    </div>
  )
}

export default Home