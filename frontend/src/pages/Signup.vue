<template>
  <div class="auth-container">
    <div class="auth-card">
      <div class="auth-header">
        <h1 class="title">Create Account</h1>
        <p class="subtitle">Start your 14-day free trial. No credit card required.</p>
      </div>

      <div v-if="success" class="alert success">
         <span class="icon">✅</span> 
         <span>Account created successfully! Please check your email to verify and set your password.</span>
      </div>

      <div v-if="error" class="alert error">
        <span class="icon">⚠️</span>
        <span>{{ error }}</span>
      </div>

      <form v-if="!success" @submit.prevent="handleSignup" class="auth-form">
        <!-- Social Sign-in -->
        <div class="social-buttons">
          <button type="button" class="btn social-btn" @click="handleGoogleSignIn" :disabled="loading">
            <svg class="social-icon" viewBox="0 0 24 24">
              <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
              <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
              <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
              <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
            </svg>
            Google
          </button>
          
          <button type="button" class="btn social-btn" @click="handleMicrosoftSignIn" :disabled="loading">
            <svg class="social-icon" viewBox="0 0 23 23">
              <rect width="10" height="10" fill="#F25022"/>
              <rect x="12" width="10" height="10" fill="#7FBA00"/>
              <rect y="12" width="10" height="10" fill="#00A4EF"/>
              <rect x="12" y="12" width="10" height="10" fill="#FFB900"/>
            </svg>
            Microsoft
          </button>
        </div>

        <div class="divider">
          <span>Or continue with email</span>
        </div>

        <div class="form-group">
          <label for="name">Full Name</label>
          <input
            id="name"
            v-model="form.name"
            type="text"
            placeholder="e.g. Jane Doe"
            class="input-field"
            required
          />
        </div>

        <div class="form-group">
          <label for="email">Work Email</label>
          <input
            id="email"
            v-model="form.email"
            type="email"
            placeholder="name@company.com"
            class="input-field"
            required
          />
        </div>

        <div class="form-group">
          <label for="orgName">Organization Name</label>
          <input
            id="orgName"
            v-model="form.organizationName"
            type="text"
            placeholder="e.g. Acme Corp"
            class="input-field"
            required
          />
        </div>

        <div class="form-group">
          <label for="orgDomain">
            Organization Domain 
            <span class="optional-badge">Optional</span>
          </label>
          <input
            id="orgDomain"
            v-model="form.organizationDomain"
            type="text"
            placeholder="acme.com"
            class="input-field"
          />
        </div>

        <button type="submit" class="btn primary-btn" :disabled="loading">
          <span v-if="loading" class="spinner"></span>
          {{ loading ? 'Creating Account...' : 'Get Started' }}
        </button>
      </form>

      <div class="auth-footer">
        <p>Already have an account? <router-link to="/login" class="link">Log in</router-link></p>
      </div>
    </div>
  </div>
</template>


<script setup>
import { reactive, ref, onMounted } from 'vue'
import { PublicClientApplication } from '@azure/msal-browser'
import { authApi } from '@/services/apiService'

const form = reactive({ name: '', email: '', organizationName: '', organizationDomain: '' })
const loading = ref(false)
const success = ref(false)
const error = ref(null)

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID
const MICROSOFT_CLIENT_ID = import.meta.env.VITE_MICROSOFT_CLIENT_ID
const MICROSOFT_TENANT_ID = import.meta.env.VITE_MICROSOFT_TENANT_ID

let msalInstance = null

onMounted(async () => {
  // Load Google Sign-In script
  if (!window.google) {
    const script = document.createElement('script')
    script.src = 'https://accounts.google.com/gsi/client'
    script.async = true
    script.defer = true
    document.head.appendChild(script)
  }

  // Initialize Microsoft MSAL
  if (MICROSOFT_CLIENT_ID && MICROSOFT_TENANT_ID) {
    msalInstance = new PublicClientApplication({
      auth: {
        clientId: MICROSOFT_CLIENT_ID,
        authority: `https://login.microsoftonline.com/${MICROSOFT_TENANT_ID}`,
        redirectUri: `${window.location.origin}/callback`
      }
    })
    // Initialize MSAL before using any other API methods
    await msalInstance.initialize()
  }
})

async function handleGoogleSignIn() {
  if (!window.google) {
    error.value = 'Google Sign-In is not loaded yet. Please try again.'
    return
  }

  window.google.accounts.id.initialize({
    client_id: GOOGLE_CLIENT_ID,
    callback: onGoogleSignInSuccess
  })

  window.google.accounts.id.renderButton(
    document.querySelector('.google-signin-section'),
    { type: 'standard', size: 'large', text: 'continue' }
  )

  // Trigger the One Tap prompt or sign in
  window.google.accounts.id.prompt((notification) => {
    if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
      // If One Tap is not shown, trigger the full sign-in flow
      window.google.accounts.id.requestCredential()
    }
  })
}

async function onGoogleSignInSuccess(response) {
  try {
    loading.value = true
    error.value = null

    // Decode JWT token (Google ID token)
    const token = response.credential
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    )
    const googleUser = JSON.parse(jsonPayload)

    // Auto-fill form with Google profile data
    form.name = googleUser.name || ''
    form.email = googleUser.email || ''

    // Extract domain from email (e.g., taazaa.com from karan.pargaie@taazaa.com)
    if (googleUser.email) {
      const emailDomain = googleUser.email.split('@')[1]
      form.organizationDomain = emailDomain || ''

      // Auto-fill organization name from domain (e.g., "Taazaa" from "taazaa.com")
      if (emailDomain) {
        const domainName = emailDomain.split('.')[0]
        form.organizationName = domainName.charAt(0).toUpperCase() + domainName.slice(1)
      }
    }

    // Focus on organization name field for user to verify/edit
    setTimeout(() => {
      document.getElementById('orgName')?.focus()
      document.getElementById('orgName')?.scrollIntoView({ behavior: 'smooth' })
    }, 100)

    loading.value = false
  } catch (err) {
    error.value = 'Failed to process Google sign-in. ' + err.message
    loading.value = false
  }
}

async function handleMicrosoftSignIn() {
  if (!msalInstance) {
    error.value = 'Microsoft Sign-In is not configured. Please try another option.'
    return
  }

  try {
    loading.value = true
    error.value = null

    const response = await msalInstance.loginPopup({
      scopes: ['openid', 'profile', 'email', 'offline_access']
    })

    // Get the account info
    const account = response.account
    const accessToken = response.accessToken

    // Extract user info from the account
    const microsoftUser = {
      name: account.name || '',
      email: account.username || account.localAccountId || ''
    }

    // Auto-fill form with Microsoft profile data
    form.name = microsoftUser.name || ''
    form.email = microsoftUser.email || ''

    // Extract domain from email (e.g., taazaa.com from karan.pargaie@taazaa.com)
    if (microsoftUser.email) {
      const emailDomain = microsoftUser.email.split('@')[1]
      form.organizationDomain = emailDomain || ''

      // Auto-fill organization name from domain (e.g., "Taazaa" from "taazaa.com")
      if (emailDomain) {
        const domainName = emailDomain.split('.')[0]
        form.organizationName = domainName.charAt(0).toUpperCase() + domainName.slice(1)
      }
    }

    // Focus on organization name field for user to verify/edit
    setTimeout(() => {
      document.getElementById('orgName')?.focus()
      document.getElementById('orgName')?.scrollIntoView({ behavior: 'smooth' })
    }, 100)

    loading.value = false
  } catch (err) {
    error.value = 'Failed to process Microsoft sign-in. ' + err.message
    loading.value = false
  }
}

async function handleSignup() {
  error.value = null
  loading.value = true
  try {
    await authApi.signup({
      name: form.name,
      email: form.email,
      organizationName: form.organizationName,
      organizationDomain: form.organizationDomain || undefined
    })
    success.value = true
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f8fafc;
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
  padding: 1.5rem;
}

.auth-card {
  width: 100%;
  max-width: 440px;
  background: white;
  padding: 2.5rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  border: 1px solid #e2e8f0;
}

.auth-header {
  text-align: center;
  margin-bottom: 2rem;
}

.title {
  font-size: 1.875rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  letter-spacing: -0.025em;
}

.subtitle {
  color: #64748b;
  font-size: 0.95rem;
  margin-top: 0.5rem;
}

.social-buttons {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.social-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  background: white;
  border: 1px solid #e2e8f0;
  color: #1e293b;
  font-weight: 500;
  padding: 0.75rem;
  border-radius: 0.5rem;
  transition: all 0.2s;
}

.social-btn:hover:not(:disabled) {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.social-icon {
  width: 1.25rem;
  height: 1.25rem;
}

.divider {
  display: flex;
  align-items: center;
  text-align: center;
  margin: 1.5rem 0;
  color: #94a3b8;
  font-size: 0.875rem;
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid #e2e8f0;
}

.divider span {
  padding: 0 0.75rem;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: #334155;
  margin-bottom: 0.375rem;
}

.input-field {
  width: 100%;
  padding: 0.625rem 0.875rem;
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 0.5rem;
  color: #0f172a;
  font-size: 0.95rem;
  transition: all 0.2s;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}

.input-field:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.input-field::placeholder {
  color: #94a3b8;
}

.optional-badge {
  font-size: 0.75rem;
  color: #94a3b8;
  font-weight: 400;
  margin-left: 0.25rem;
}

.btn {
  width: 100%;
  cursor: pointer;
  font-family: inherit;
  font-size: 0.95rem;
}

.primary-btn {
  background: #2563eb;
  color: white;
  border: none;
  padding: 0.75rem;
  border-radius: 0.5rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  transition: background 0.2s;
  box-shadow: 0 1px 2px rgba(0,0,0,0.1);
}

.primary-btn:hover:not(:disabled) {
  background: #1d4ed8;
}

.primary-btn:disabled {
  background: #93c5fd;
  cursor: not-allowed;
}

.auth-footer {
  margin-top: 1.5rem;
  text-align: center;
  font-size: 0.9rem;
  color: #64748b;
}

.link {
  color: #2563eb;
  text-decoration: none;
  font-weight: 600;
}

.link:hover {
  text-decoration: underline;
}

.alert {
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1.5rem;
  display: flex;
  gap: 0.75rem;
  font-size: 0.9rem;
  align-items: flex-start;
}

.alert.success {
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #dcfce7;
}

.alert.error {
  background: #fef2f2;
  color: #991b1b;
  border: 1px solid #fee2e2;
}

.spinner {
  width: 1rem;
  height: 1rem;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 640px) {
  .auth-container { padding: 1rem; }
  .auth-card { padding: 1.5rem; }
}
</style>
