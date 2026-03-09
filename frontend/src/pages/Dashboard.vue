<template>
  <div class="dashboard-container">
    <!-- Header Section -->
    <header class="dashboard-header">
      <div class="header-content">
        <h1 class="title">Dashboard</h1>
        <p class="subtitle">Secure Session Overview</p>
      </div>
      <div class="header-actions">
        <div class="status-badge" :class="{ active: isValidSession }">
          <span class="status-dot"></span>
          {{ isValidSession ? 'Session Active' : 'Session Inactive' }}
        </div>
      </div>
    </header>

    <div v-if="!authStore.accessToken" class="empty-state">
      <div class="empty-icon">🔐</div>
      <h3>Authentication Required</h3>
      <p>Please log in to view your secure session details.</p>
    </div>

    <div v-else class="dashboard-grid">
      <!-- Primary metrics from Token -->
      <section class="metrics-row">
        <div class="metric-card user-card">
          <div class="card-icon">👤</div>
          <div class="card-content">
            <label>User Identity (Subject)</label>
            <div class="value" :title="tokenData.sub">{{ tokenData.sub || 'N/A' }}</div>
          </div>
        </div>

        <div class="metric-card org-card">
          <div class="card-icon">🏢</div>
          <div class="card-content">
            <label>Organization Context</label>
            <div class="value">{{ tokenData.org_id || 'No Organization Context' }}</div>
          </div>
        </div>

        <div class="metric-card time-card">
          <div class="card-icon">⏳</div>
          <div class="card-content">
            <label>Session Expiry</label>
            <div class="value">{{ timeRemaining }}</div>
            <small class="meta">Expires at {{ formatTime(tokenData.exp) }}</small>
          </div>
        </div>
      </section>

      <!-- Permissions & Scopes Area -->
      <section class="details-grid">
        <!-- Permissions Panel -->
        <div class="panel permissions-panel">
          <div class="panel-header">
            <h3>Granted Permissions</h3>
            <span class="count-badge">{{ permissionsList.length }}</span>
          </div>
          <div class="panel-body">
            <div v-if="permissionsList.length > 0" class="badges-container">
              <span 
                v-for="perm in permissionsList" 
                :key="perm" 
                class="badge permission-badge"
              >
                {{ perm }}
              </span>
            </div>
            <p v-else class="empty-text">No specific permissions granted in this token.</p>
          </div>
        </div>

        <!-- Scopes Panel -->
        <div class="panel scopes-panel">
          <div class="panel-header">
            <h3>Authorized Scopes</h3>
            <span class="count-badge">{{ scopesList.length }}</span>
          </div>
          <div class="panel-body">
            <div v-if="scopesList.length > 0" class="badges-container">
              <span 
                v-for="scope in scopesList" 
                :key="scope" 
                class="badge scope-badge"
              >
                {{ scope }}
              </span>
            </div>
            <p v-else class="empty-text">No scopes found.</p>
          </div>
        </div>
      </section>

      <!-- Technical Token Details -->
      <section class="technical-details">
        <div class="panel">
          <div class="panel-header" @click="toggleRaw" style="cursor: pointer; user-select: none;">
            <h3>Technical Metadata (JWT)</h3>
            <span class="toggle-icon">{{ showRaw ? '−' : '+' }}</span>
          </div>
          
          <div v-if="showRaw" class="details-content">
            <div class="metadata-grid">
              <div class="meta-item">
                <span class="meta-label">Issuer (iss)</span>
                <span class="meta-value">{{ tokenData.iss }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">Audience (aud)</span>
                <span class="meta-value" :title="tokenData.aud">{{ formatAudience(tokenData.aud) }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">Issued At (iat)</span>
                <span class="meta-value">{{ formatTime(tokenData.iat) }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">Authorized Party (azp)</span>
                <span class="meta-value">{{ tokenData.azp }}</span>
              </div>
            </div>

            <div class="raw-token-box">
              <div class="box-header">
                <span>Raw Access Token</span>
                <button class="copy-btn" @click="copyToken">
                  {{ copied ? 'Copied!' : 'Copy Token' }}
                </button>
              </div>
              <pre><code>{{ authStore.accessToken }}</code></pre>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const authStore = useAuthStore()
const tokenData = ref({})
const showRaw = ref(true)
const copied = ref(false)
const now = ref(Date.now() / 1000)

setInterval(() => {
  now.value = Date.now() / 1000
}, 60000)

const isValidSession = computed(() => {
  return tokenData.value.exp && tokenData.value.exp > now.value
})

const timeRemaining = computed(() => {
  if (!tokenData.value.exp) return 'Unknown'
  const diff = tokenData.value.exp - now.value
  if (diff <= 0) return 'Expired'
  
  const hours = Math.floor(diff / 3600)
  const minutes = Math.floor((diff % 3600) / 60)
  
  if (hours > 0) return `${hours}h ${minutes}m remaining`
  return `${minutes}m remaining`
})

const permissionsList = computed(() => {
  return tokenData.value.permissions || []
})

const scopesList = computed(() => {
  return tokenData.value.scope ? tokenData.value.scope.split(' ') : []
})

function formatTime(timestamp) {
  if (!timestamp) return '—'
  return new Date(timestamp * 1000).toLocaleString()
}

function formatAudience(aud) {
  if (Array.isArray(aud)) return aud.join(', ')
  return aud
}

function toggleRaw() {
  showRaw.value = !showRaw.value
}

function copyToken() {
  if (!authStore.accessToken) return
  navigator.clipboard.writeText(authStore.accessToken)
  copied.value = true
  setTimeout(() => copied.value = false, 2000)
}

function decodeJwt(token) {
  if (!token) return {}
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
    }).join(''))
    return JSON.parse(jsonPayload)
  } catch (e) {
    console.error('Invalid Token', e)
    return {}
  }
}

watch(() => authStore.accessToken, (newToken) => {
  tokenData.value = decodeJwt(newToken)
}, { immediate: true })
</script>

<style scoped>
/* Modern CSS Reset & Variables */
.dashboard-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  color: #1e293b;
}

/* Header */
.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 3rem;
  border-bottom: 1px solid #e2e8f0;
  padding-bottom: 1.5rem;
}

.title {
  font-size: 2rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  letter-spacing: -0.025em;
}

.subtitle {
  font-size: 1rem;
  color: #64748b;
  margin: 0.25rem 0 0 0;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: #f1f5f9;
  color: #64748b;
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.2s;
}

.status-badge.active {
  background: #dcfce7;
  color: #166534;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: currentColor;
}

/* Metrics Grid */
.metrics-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.metric-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 1rem;
  padding: 1.5rem;
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  transition: transform 0.2s, box-shadow 0.2s;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}

.metric-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}

.card-icon {
  font-size: 1.5rem;
  background: #f8fafc;
  width: 3rem;
  height: 3rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.75rem;
}

.card-content {
  flex: 1;
  min-width: 0; /* Text truncation fix */
}

.metric-card label {
  display: block;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #64748b;
  font-weight: 600;
  margin-bottom: 0.25rem;
}

.metric-card .value {
  font-size: 1.125rem;
  font-weight: 600;
  color: #0f172a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.metric-card .meta {
  display: block;
  font-size: 0.75rem;
  color: #94a3b8;
  margin-top: 0.25rem;
}

/* Details Grid */
.details-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.panel {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 1rem;
  overflow: hidden;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}

.panel-header {
  background: #f8fafc;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.panel-header h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: #334155;
}

.count-badge {
  background: #e2e8f0;
  color: #475569;
  padding: 0.1rem 0.5rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 700;
}

.panel-body {
  padding: 1.5rem;
}

.badges-container {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.badge {
  display: inline-flex;
  padding: 0.35rem 0.75rem;
  border-radius: 0.5rem;
  font-size: 0.8rem;
  font-family: 'Monaco', 'Consolas', monospace;
  font-weight: 500;
}

.permission-badge {
  background: #eff6ff;
  color: #2563eb;
  border: 1px solid #dbeafe;
}

.scope-badge {
  background: #f0fdf4;
  color: #16a34a;
  border: 1px solid #dcfce7;
}

.empty-text {
  color: #94a3b8;
  font-style: italic;
  font-size: 0.875rem;
  margin: 0;
}

/* Technical Details */
.technical-details .metadata-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
  padding: 1.5rem;
  background: #ffffff;
}

.meta-item {
  display: flex;
  flex-direction: column;
}

.meta-label {
  font-size: 0.75rem;
  color: #64748b;
  margin-bottom: 0.25rem;
}

.meta-value {
  font-size: 0.875rem;
  color: #334155;
  font-family: monospace;
  word-break: break-all;
}

.raw-token-box {
  border-top: 1px solid #e2e8f0;
  background: #0f172a;
  padding: 1.5rem;
}

.box-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.box-header span {
  color: #94a3b8;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.copy-btn {
  background: transparent;
  border: 1px solid #334155;
  color: #cbd5e1;
  padding: 0.25rem 0.75rem;
  border-radius: 0.25rem;
  font-size: 0.75rem;
  cursor: pointer;
  transition: all 0.2s;
}

.copy-btn:hover {
  border-color: #64748b;
  color: #ffffff;
}

.raw-token-box pre {
  margin: 0;
  color: #7dd3fc;
  font-family: 'Monaco', 'Consolas', monospace;
  font-size: 0.75rem;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 200px;
  overflow-y: auto;
}

::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}
::-webkit-scrollbar-track {
  background: transparent;
}
::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 4px;
}
::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

@media (max-width: 768px) {
  .dashboard-container {
    padding: 1rem;
  }
  
  .dashboard-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
}
</style>
