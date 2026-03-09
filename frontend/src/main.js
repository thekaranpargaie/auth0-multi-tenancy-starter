import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/authStore'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

// Start Auth0 init in the background — the router guard and Callback.vue
// both poll authStore.loading, so they will wait for this to finish.
useAuthStore().init()

app.mount('#app')
