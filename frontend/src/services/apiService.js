import axios from 'axios'
import { useAuthStore } from '@/stores/authStore'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:8000',
  headers: { 'Content-Type': 'application/json' }
})

// Request interceptor — attach JWT before every authenticated request
apiClient.interceptors.request.use(async (config) => {
  if (config._skipAuth) return config

  try {
    const authStore = useAuthStore()
    if (authStore.isAuthenticated) {
      const token = await authStore.getToken()
      config.headers.Authorization = `Bearer ${token}`
    }
  } catch (err) {
    console.warn('[API] Could not attach token:', err.message)
  }
  return config
})

// Response interceptor — normalise error shape
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const message =
      error.response?.data?.detail ||
      error.response?.data?.message ||
      error.message ||
      'An unexpected error occurred.'
    return Promise.reject(new Error(message))
  }
)

// ── API methods ──────────────────────────────────────────────────────────────

export const authApi = {
  /** Public signup — no JWT required */
  signup(payload) {
    return apiClient.post('/api/signup', payload, { _skipAuth: true })
  },

  /** Invite a user (requires JWT) */
  inviteUser(payload) {
    return apiClient.post('/api/invite', payload)
  },

  /** Get members of an organization */
  getMembers(organizationId) {
    return apiClient.get(`/api/organizations/${organizationId}/members`)
  },

  /** Remove a member from an organization */
  removeMember(organizationId, userId) {
    return apiClient.delete(`/api/organizations/${organizationId}/members/${userId}`)
  },

  /** Health check */
  health() {
    return apiClient.get('/health', { _skipAuth: true })
  }
}
