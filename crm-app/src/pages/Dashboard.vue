<template>
  <div class="dashboard">
    <div class="header-actions">
      <button class="logout-button" @click="handleLogout">Logout</button>
    </div>

    <div class="card">
      <h2>User Information</h2>
      <div v-if="loading" class="loading">Loading user information...</div>
      <div v-else-if="authStore.user" class="user-info">
        <p><strong>Name:</strong> {{ authStore.user.name }}</p>
        <p><strong>Email:</strong> {{ authStore.user.email }}</p>
        <p><strong>Email Verified:</strong> {{ authStore.user.email_verified ? 'Yes' : 'No' }}</p>
        <p><strong>Organization ID:</strong> {{ authStore.orgId }}</p>
        <p><strong>Organization Name:</strong> {{ authStore.orgDisplayName }}</p>
        <p><strong>Is Admin:</strong> {{ authStore.isAdmin ? 'Yes' : 'No' }}</p>
      </div>
      <div v-else class="error">
        Not authenticated
      </div>
    </div>

    <div class="card">
      <h2>JWT Token Details</h2>
      <button @click="fetchToken" :disabled="loading">
        {{ loading ? 'Fetching...' : 'Fetch Access Token' }}
      </button>
      <div v-if="tokenLoading" class="loading">Fetching token...</div>
      <div v-else-if="decodedToken" class="token-info">
        <h3>Decoded Token Payload</h3>
        <pre>{{ JSON.stringify(decodedToken, null, 2) }}</pre>
      </div>
      <div v-else-if="tokenError" class="error">
        {{ tokenError }}
      </div>
    </div>

    <div class="card">
      <h2>JWT Access Token</h2>
      <div class="token-actions">
        <button @click="copyToken" :disabled="!authStore.accessToken">
          Copy Token
        </button>
        <button @click="showFullToken = !showFullToken" class="secondary">
          {{ showFullToken ? 'Hide Full Token' : 'Show Full Token' }}
        </button>
      </div>
      <div v-if="showFullToken && authStore.accessToken" class="token-display">
        <textarea readonly :value="authStore.accessToken" rows="10" cols="80"></textarea>
      </div>
    </div>

    <div v-if="copied" class="success">
      Token copied to clipboard!
    </div>
  </div>
</template>

<script>
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/authStore'

export default {
  name: 'Dashboard',
  setup() {
    const authStore = useAuthStore()
    const loading = ref(false)
    const tokenLoading = ref(false)
    const tokenError = ref(null)
    const decodedToken = ref(null)
    const showFullToken = ref(false)
    const copied = ref(false)

    const fetchToken = async () => {
      tokenLoading.value = true
      tokenError.value = null
      try {
        const token = await authStore.getToken()
        // Decode and display token payload
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
        decodedToken.value = payload
      } catch (err) {
        tokenError.value = `Failed to fetch token: ${err.message}`
      } finally {
        tokenLoading.value = false
      }
    }

    const copyToken = async () => {
      if (authStore.accessToken) {
        try {
          await navigator.clipboard.writeText(authStore.accessToken)
          copied.value = true
          setTimeout(() => {
            copied.value = false
          }, 3000)
        } catch (err) {
          console.error('Failed to copy token:', err)
        }
      }
    }

    const handleLogout = async () => {
      await authStore.logout()
    }

    return {
      authStore,
      loading,
      tokenLoading,
      tokenError,
      decodedToken,
      showFullToken,
      copied,
      fetchToken,
      copyToken,
      handleLogout
    }
  }
}
</script>

<style scoped>
.dashboard {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.header-actions {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 1rem;
}

.card {
  background-color: #f8f9fa;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.card h2 {
  margin-top: 0;
  color: #0066cc;
  border-bottom: 2px solid #0066cc;
  padding-bottom: 0.5rem;
}

.user-info p {
  margin: 0.5rem 0;
}

.token-info pre {
  background-color: #e9ecef;
  padding: 1rem;
  border-radius: 4px;
  overflow-x: auto;
  font-size: 0.9em;
  margin-top: 1rem;
}

.token-actions {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.token-display textarea {
  width: 100%;
  font-family: 'Courier New', monospace;
  font-size: 0.8em;
}

.success {
  color: #28a745;
  background-color: #d4edda;
  border: 1px solid #c3e6cb;
  padding: 0.5rem;
  border-radius: 4px;
  margin-top: 1rem;
}
</style>