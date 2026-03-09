<template>
  <div class="auth-container">
    <div class="auth-card">
      <div class="auth-header">
        <div class="logo-icon">🔑</div>
        <h1 class="title">Welcome Back</h1>
        <p class="subtitle">Sign in to access your organization workspace.</p>
      </div>

      <div v-if="error" class="alert error">
        <span class="icon">⚠️</span>
        {{ error }}
      </div>

      <button class="btn primary-btn" @click="handleLogin" :disabled="loading">
        <span v-if="loading" class="spinner"></span>
        {{ loading ? 'Connecting to Secure Server...' : 'Sign In' }}
      </button>

      <div class="auth-footer">
        <p>Don't have an account? <router-link to="/signup" class="link">Create Organization</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const authStore = useAuthStore()
const loading = ref(false)
const error = ref(null)

async function handleLogin() {
  loading.value = true
  error.value = null
  try {
    await authStore.login()
  } catch (e) {
    error.value = e.message
    loading.value = false
  }
}
</script>

<style scoped>
/* Reusing the design system from Signup */
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
  max-width: 400px;
  background: white;
  padding: 2.5rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  border: 1px solid #e2e8f0;
  text-align: center;
}

.auth-header {
  margin-bottom: 2rem;
}

.logo-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  display: inline-block;
  background: #f1f5f9;
  width: 5rem;
  height: 5rem;
  line-height: 5rem;
  border-radius: 50%;
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

.btn {
  width: 100%;
  cursor: pointer;
  font-family: inherit;
  font-size: 1rem;
}

.primary-btn {
  background: #2563eb;
  color: white;
  border: none;
  padding: 0.875rem;
  border-radius: 0.5rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  transition: all 0.2s;
  box-shadow: 0 1px 2px rgba(0,0,0,0.1);
}

.primary-btn:hover:not(:disabled) {
  background: #1d4ed8;
  transform: translateY(-1px);
}

.primary-btn:disabled {
  background: #93c5fd;
  cursor: not-allowed;
}

.alert.error {
  background: #fef2f2;
  color: #991b1b;
  border: 1px solid #fee2e2;
  padding: 0.75rem;
  border-radius: 0.5rem;
  margin-bottom: 1.5rem;
  font-size: 0.9rem;
}

.auth-footer {
  margin-top: 2rem;
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
</style>
