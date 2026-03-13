import { useEffect } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth0 } from '@auth0/auth0-react'
import Callback from './Callback'
import Dashboard from './Dashboard'
import Home from './Home'

function App() {
  const { isLoading, error, isAuthenticated, loginWithRedirect } = useAuth0()

  // Auto-redirect to login if not authenticated
  useEffect(() => {
    if (!isLoading && !isAuthenticated && !error) {
      loginWithRedirect()
    }
  }, [isLoading, isAuthenticated, loginWithRedirect, error])

  if (isLoading) {
    return <div className="loading">Loading...</div>
  }

  if (error) {
    return <div className="error">{error.message}</div>
  }

  const ProtectedRoute = ({ children }) => {
    if (!isAuthenticated) {
      return <Navigate to="/" replace />
    }
    return children
  }

  return (
    <div className="App">
      <header className="header">
        <h1>Notes Application</h1>
      </header>
      <main className={isAuthenticated ? "content" : ""}>
        <Routes>
          <Route path="/callback" element={<Callback />} />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />
          <Route path="/" element={<Home />} />
        </Routes>
      </main>
    </div>
  )
}

export default App