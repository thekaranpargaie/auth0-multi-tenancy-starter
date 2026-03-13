import { useAuth0 } from '@auth0/auth0-react'
import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'

function Callback() {
  const { isLoading, error, isAuthenticated } = useAuth0()
  const navigate = useNavigate()

  useEffect(() => {
    if (!isLoading) {
      if (isAuthenticated) {
        navigate('/dashboard')
      }
    }
  }, [isLoading, isAuthenticated, navigate])

  if (isLoading) {
    return <div className="loading">Processing authentication...</div>
  }

  if (error) {
    return <div className="error">Authentication failed: {error.message}</div>
  }

  if (isAuthenticated) {
    return <div className="success">Authentication successful! Redirecting to dashboard...</div>
  }

  return null
}

export default Callback