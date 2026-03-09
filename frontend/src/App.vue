<template>
  <div id="app-root">
    <nav v-if="authStore.isAuthenticated" class="navbar">
      <div class="nav-container">
        <div class="navbar-brand">
          <span class="brand-icon">🏢</span>
          <span class="brand-text">SaaS Platform</span>
        </div>
        
        <div class="navbar-links">
          <router-link to="/dashboard" class="nav-link">Dashboard</router-link>
          <router-link to="/users" class="nav-link">Users</router-link>
        </div>

        <div class="navbar-actions">
          <div class="org-badge" v-if="authStore.orgDisplayName">
            <span class="org-icon">🏢</span>
            {{ authStore.orgDisplayName }}
          </div>
          <button @click="authStore.logout()" class="btn-logout">Sign Out</button>
        </div>
      </div>
    </nav>

    <main class="main-content">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/authStore'
const authStore = useAuthStore()
</script>

<style>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

:root {
  --primary-color: #2563eb;
  --primary-hover: #1d4ed8;
  --bg-color: #f8fafc;
  --text-main: #0f172a;
  --text-muted: #64748b;
  --border-color: #e2e8f0;
}

*, *::before, *::after { 
  box-sizing: border-box; 
}

body {
  margin: 0;
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
  background-color: var(--bg-color);
  color: var(--text-main);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

#app-root {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

/* Navbar */
.navbar {
  background: white;
  border-bottom: 1px solid var(--border-color);
  position: sticky;
  top: 0;
  z-index: 50;
  box-shadow: 0 1px 2px rgba(0,0,0,0.03);
}

.nav-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 2rem;
  height: 4rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.navbar-brand {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 700;
  font-size: 1.125rem;
  color: var(--text-main);
  margin-right: 2rem;
}

.brand-icon { font-size: 1.25rem; }

.navbar-links {
  display: flex;
  gap: 0.5rem;
  flex: 1;
}

.nav-link {
  color: var(--text-muted);
  text-decoration: none;
  font-weight: 500;
  padding: 0.5rem 0.75rem;
  border-radius: 0.375rem;
  transition: all 0.2s;
  font-size: 0.95rem;
}

.nav-link:hover {
  color: var(--primary-color);
  background: #eff6ff;
}

.nav-link.router-link-active {
  color: var(--primary-color);
  background: #eff6ff;
  font-weight: 600;
}

.navbar-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.org-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0.75rem;
  background: #f1f5f9;
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 500;
  color: #475569;
  border: 1px solid var(--border-color);
}

.org-icon { font-size: 0.875rem; }

.btn-logout {
  background: transparent;
  border: 1px solid var(--border-color);
  padding: 0.375rem 0.875rem;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
}

.btn-logout:hover {
  background: #fef2f2;
  color: #ef4444;
  border-color: #fee2e2;
}

.main-content {
  flex: 1;
  width: 100%;
}

/* Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
