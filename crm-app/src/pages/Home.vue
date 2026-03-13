<template>
  <div class="home">
    <h2>CRM Application</h2>
    <p>Welcome to the CRM Application</p>
    <p v-if="authStore.loading">Loading authentication...</p>
    <p v-else-if="!authStore.isAuthenticated">
      You are not logged in. Please login to access the application.
      <button class="auth-button" @click="authStore.login">Login</button>
    </p>
    <p v-else>
      You are logged in as {{ authStore.user?.email }}
    </p>
  </div>
</template>

<script>
import { useAuthStore } from '@/stores/authStore'

export default {
  name: 'Home',
  setup() {
    const authStore = useAuthStore()

    return {
      authStore
    }
  }
}
</script>

<style scoped>
.home {
  text-align: center;
  padding: 2rem;
}

.auth-button {
  background-color: #0066cc;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  margin-top: 1rem;
}

.auth-button:hover {
  background-color: #0052a3;
}
</style>