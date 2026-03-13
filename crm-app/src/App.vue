<template>
  <div id="app">
    <header class="header">
      <h1>CRM Application</h1>
      <div v-if="authStore.loading" class="loading">Loading...</div>
    </header>
    <main v-if="authStore.isAuthenticated" class="content">
      <router-view />
    </main>
  </div>
</template>

<script>
import { watch } from 'vue'
import { useAuthStore } from '@/stores/authStore'

export default {
  name: 'App',
  setup() {
    const authStore = useAuthStore()

    // Auto-redirect to login if not authenticated after loading complete
    watch(
      () => authStore.loading,
      (isLoading) => {
        if (!isLoading && !authStore.isAuthenticated) {
          authStore.login()
        }
      }
    )

    return {
      authStore
    }
  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.header {
  background-color: #0066cc;
  color: white;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 2rem;
}

.content {
  background-color: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

nav {
  margin-bottom: 2rem;
}

nav a {
  margin-right: 1rem;
  text-decoration: none;
  color: #0066cc;
}

nav a:hover {
  text-decoration: underline;
}

.auth-button {
  background-color: #0066cc;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  margin-right: 0.5rem;
}

.auth-button:hover {
  background-color: #0052a3;
}

.auth-button.secondary {
  background-color: #6c757d;
}

.auth-button.secondary:hover {
  background-color: #5a6268;
}

.logout-button {
  background-color: #dc3545;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
}

.logout-button:hover {
  background-color: #c82333;
}

.loading {
  text-align: center;
  padding: 2rem;
}

.error {
  color: #dc3545;
  padding: 1rem;
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.token-info {
  background-color: #f8f9fa;
  padding: 1rem;
  border-radius: 4px;
  margin-top: 1rem;
}

.token-info pre {
  background-color: #e9ecef;
  padding: 0.5rem;
  border-radius: 4px;
  overflow-x: auto;
  font-size: 0.9em;
}

.auth-prompt {
  text-align: center;
  padding: 1rem;
}

.auth-prompt p {
  margin-bottom: 1rem;
}
</style>