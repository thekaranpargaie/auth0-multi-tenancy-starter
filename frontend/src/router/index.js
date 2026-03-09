import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const routes = [
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/signup',
    name: 'Signup',
    component: () => import('@/pages/Signup.vue'),
    meta: { requiresAuth: false, title: 'Sign Up' }
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/pages/Login.vue'),
    meta: { requiresAuth: false, title: 'Login' }
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
  },
  {
    path: '/users',
    name: 'UserManagement',
    component: () => import('@/pages/UserManagement.vue'),
    meta: { requiresAuth: true, title: 'User Management' }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/dashboard'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Navigation guard — redirect unauthenticated users to /login
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
    return { name: 'Login', query: { redirect: to.fullPath } }
  }
})

// Page title
router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} — Multi-Tenant SaaS` : 'Multi-Tenant SaaS'
})

export default router
