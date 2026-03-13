<template>
  <div class="callback">
    <div v-if="loading" class="loading">Processing authentication...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else class="success">
      Authentication successful! Redirecting to dashboard...
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

export default {
  name: 'Callback',
  setup() {
    const router = useRouter()
    const authStore = useAuthStore()

    const loading = ref(true)
    const error = ref(null)

    onMounted(async () => {
      try {
        // Auth is already initialized in main.js
        // Just wait for it to complete and handle any errors
        if (!authStore.isAuthenticated && authStore.error) {
          throw new Error(authStore.error)
        }
        
        // Give auth store time to finish processing if still loading
        if (authStore.loading) {
          await new Promise((resolve) => {
            const checkInterval = setInterval(() => {
              if (!authStore.loading) {
                clearInterval(checkInterval)
                resolve()
              }
            }, 100)
            setTimeout(() => clearInterval(checkInterval), 5000) // 5 second timeout
          })
        }

        // Check final auth status
        if (authStore.error) {
          throw new Error(`Authentication failed: ${authStore.error}`)
        }

        if (authStore.isAuthenticated) {
          // Redirect to dashboard after successful auth
          router.push('/dashboard')
        } else {
          throw new Error('Authentication did not complete successfully')
        }
      } catch (err) {
        error.value = err.message
        console.error('Callback error:', err)
      } finally {
        loading.value = false
      }
    })

    return {
      loading,
      error
    }
  }
}
</script>

<style scoped>
.callback {
  text-align: center;
  padding: 4rem 2rem;
}

.loading {
  font-size: 1.2rem;
  color: #0066cc;
}

.error {
  color: #dc3545;
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  padding: 1rem;
  border-radius: 4px;
  max-width: 500px;
  margin: 0 auto;
}

.success {
  color: #28a745;
  background-color: #d4edda;
  border: 1px solid #c3e6cb;
  padding: 1rem;
  border-radius: 4px;
  max-width: 500px;
  margin: 0 auto;
}
</style>