import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/pages/Home.vue'),
    meta: { requiresAuth: false, title: 'Home' }
  },
  {
    path: '/callback',
    name: 'Callback',
    component: () => import('@/pages/Callback.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/pages/Dashboard.vue'),
    meta: { requiresAuth: true, title: 'Dashboard' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Navigation guard — redirect unauthenticated users to login
router.beforeEach(async (to) => {
  const authStore = useAuthStore()

  // Wait for auth to finish loading
  if (authStore.loading) {
    await new Promise((resolve) => {
      const unwatch = setInterval(() => {
        if (!authStore.loading) {
          clearInterval(unwatch)
          resolve()
        }
      }, 50)
    })
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'Callback' }
  }

  // Redirect authenticated users from home to dashboard
  if (to.path === '/' && authStore.isAuthenticated) {
    return { path: '/dashboard' }
  }
})

// Page title
router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} — CRM App` : 'CRM App'
})

export default router