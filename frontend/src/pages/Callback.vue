<template>
  <div class="callback-container">
    <div class="loading-wrapper">
      <div class="spinner"></div>
      <h2 class="loading-title">Completing Sign In</h2>
      <p class="loading-subtitle">Please wait while we secure your session...</p>
    </div>
  </div>
</template>

<script setup>
import { onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = useRouter()
const authStore = useAuthStore()

onMounted(() => {
  const checkAndRedirect = () => {
    if (!authStore.loading) {
      router.replace(authStore.isAuthenticated ? '/dashboard' : '/login')
    }
  }

  if (authStore.loading) {
    const unwatch = watch(() => authStore.loading, (newLoading) => {
      if (!newLoading) {
        checkAndRedirect()
        unwatch()
      }
    })
  } else {
    checkAndRedirect()
  }
})
</script>

<style scoped>
.callback-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f8fafc;
}

.loading-wrapper {
  text-align: center;
  animation: fadeIn 0.4s ease-out;
}

.spinner {
  width: 3rem;
  height: 3rem;
  border: 4px solid #e2e8f0;
  border-top-color: #2563eb;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  margin: 0 auto 1.5rem;
}

.loading-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #0f172a;
  margin: 0 0 0.5rem;
}

.loading-subtitle {
  color: #64748b;
  font-size: 0.95rem;
  margin: 0;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
