<template>
  <div class="page-container">
    <header class="page-header">
      <div class="header-content">
        <h1 class="title">User Management</h1>
        <p class="subtitle">Manage members of <span class="highlight">{{ authStore.orgDisplayName }}</span></p>
      </div>
      <button class="btn primary-btn" @click="showInviteModal = true">
        <span class="icon">+</span> Invite User
      </button>
    </header>

    <!-- Notifications -->
    <div v-if="actionSuccess" class="notification success">
      <span class="icon">✅</span> {{ actionSuccess }}
    </div>
    <div v-if="actionError" class="notification error">
      <span class="icon">⚠️</span> {{ actionError }}
    </div>

    <!-- Members List -->
    <div class="content-card">
      <div v-if="loading" class="state-empty">
        <div class="spinner"></div>
        <p>Loading members...</p>
      </div>

      <div v-else-if="members.length === 0" class="state-empty">
        <div class="empty-icon">👥</div>
        <h3>No members found</h3>
        <p>Get started by inviting your team members to this organization.</p>
        <button class="btn secondary-btn" @click="showInviteModal = true">Invite First Member</button>
      </div>

      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>User ID</th>
              <th class="text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="member in members" :key="member.userId">
              <td class="font-medium">{{ member.name || '—' }}</td>
              <td class="text-muted">{{ member.email }}</td>
              <td><code class="code-badge">{{ member.userId }}</code></td>
              <td class="text-right">
                <button
                  class="btn danger-btn-sm"
                  @click="confirmRemove(member)"
                  :disabled="removingId === member.userId"
                >
                  {{ removingId === member.userId ? 'Removing...' : 'Remove' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Invite Modal -->
    <Teleport to="body">
      <div v-if="showInviteModal" class="modal-backdrop" @click.self="showInviteModal = false">
        <div class="modal-dialog">
          <div class="modal-header">
            <h2>Invite New User</h2>
            <button class="close-btn" @click="showInviteModal = false">×</button>
          </div>

          <div v-if="inviteSuccess" class="modal-body success-state">
            <div class="success-icon">✉️</div>
            <h3>Invitation Sent!</h3>
            <p>{{ inviteSuccess }}</p>
            <button class="btn primary-btn full-width" @click="closeInvite">Done</button>
          </div>

          <form v-else @submit.prevent="handleInvite" class="modal-body">
            <div v-if="inviteError" class="notification error compact">
              {{ inviteError }}
            </div>

            <div class="form-group">
              <label>Email Address</label>
              <input 
                v-model="inviteForm.email" 
                type="email" 
                placeholder="colleague@company.com" 
                class="input-field"
                required 
                autofocus
              />
            </div>

            <div class="form-group">
              <label>Role</label>
              <div class="select-wrapper">
                <select v-model="inviteForm.role" class="input-field">
                  <option value="0">Member (Read Only)</option>
                  <option value="1">Admin (Full Access)</option>
                </select>
              </div>
            </div>

            <div class="modal-footer">
              <button type="button" class="btn secondary-btn" @click="showInviteModal = false">Cancel</button>
              <button type="submit" class="btn primary-btn" :disabled="inviteLoading">
                {{ inviteLoading ? 'Sending...' : 'Send Invitation' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { authApi } from '@/services/apiService'

const authStore = useAuthStore()

const members = ref([])
const loading = ref(true)
const actionSuccess = ref(null)
const actionError = ref(null)
const removingId = ref(null)

const showInviteModal = ref(false)
const inviteLoading = ref(false)
const inviteError = ref(null)
const inviteSuccess = ref(null)
const inviteForm = reactive({ email: '', role: '0' })

async function loadMembers() {
  if (!authStore.orgId) return
  loading.value = true
  try {
    const { data } = await authApi.getMembers(authStore.orgId)
    members.value = Array.isArray(data) ? data : []
  } catch (e) {
    actionError.value = e.message
  } finally {
    loading.value = false
  }
}

async function confirmRemove(member) {
  if (!confirm(`Remove ${member.email || member.userId} from the organization?`)) return
  removingId.value = member.userId
  actionSuccess.value = null
  actionError.value = null
  try {
    await authApi.removeMember(authStore.orgId, member.userId)
    members.value = members.value.filter(m => m.userId !== member.userId)
    actionSuccess.value = `${member.email} removed successfully.`
  } catch (e) {
    actionError.value = e.message
  } finally {
    removingId.value = null
  }
}

async function handleInvite() {
  inviteError.value = null
  inviteLoading.value = true
  try {
    await authApi.inviteUser({
      organizationId: authStore.orgId,
      email: inviteForm.email,
      role: parseInt(inviteForm.role)
    })
    inviteSuccess.value = `Invitation sent to ${inviteForm.email}.`
    await loadMembers()
  } catch (e) {
    inviteError.value = e.message
  } finally {
    inviteLoading.value = false
  }
}

function closeInvite() {
  showInviteModal.value = false
  inviteSuccess.value = null
  inviteError.value = null
  inviteForm.email = ''
  inviteForm.role = '0'
}

onMounted(loadMembers)
</script>

<style scoped>
.page-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
  font-family: 'Inter', sans-serif;
  color: #1e293b;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid #e2e8f0;
}
.title {
  font-size: 2rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  letter-spacing: -0.025em;
}
.subtitle {
  color: #64748b;
  margin-top: 0.5rem;
}
.highlight {
  color: #2563eb;
  font-weight: 600;
}

/* Cards & Content */
.content-card {
  background: white;
  border-radius: 1rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

/* Notifications */
.notification {
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1.5rem;
  display: flex;
  gap: 0.75rem;
  align-items: center;
  font-size: 0.95rem;
}
.notification.success { background: #f0fdf4; color: #166534; border: 1px solid #dcfce7; }
.notification.error { background: #fef2f2; color: #991b1b; border: 1px solid #fee2e2; }

/* Empty States */
.state-empty {
  padding: 4rem 2rem;
  text-align: center;
  color: #64748b;
}
.empty-icon { font-size: 3rem; margin-bottom: 1rem; opacity: 0.5; }

/* Table */
.data-table {
  width: 100%;
  border-collapse: collapse;
}
.data-table th, .data-table td {
  padding: 1rem 1.5rem;
  text-align: left;
  border-bottom: 1px solid #f1f5f9;
}
.data-table th {
  background: #f8fafc;
  color: #64748b;
  font-weight: 600;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.data-table tr:last-child td { border-bottom: none; }
.font-medium { font-weight: 500; color: #334155; }
.text-muted { color: #64748b; }
.code-badge {
  background: #f1f5f9;
  padding: 0.25rem 0.5rem;
  border-radius: 0.375rem;
  font-family: monospace;
  font-size: 0.85rem;
  color: #475569;
}
.text-right { text-align: right; }

/* Buttons */
.btn {
  border: none;
  cursor: pointer;
  border-radius: 0.5rem;
  font-weight: 500;
  font-family: inherit;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}
.primary-btn {
  background: #2563eb;
  color: white;
  padding: 0.625rem 1.25rem;
  box-shadow: 0 1px 2px rgba(0,0,0,0.1);
}
.primary-btn:hover:not(:disabled) { background: #1d4ed8; }
.secondary-btn {
  background: white;
  border: 1px solid #cbd5e1;
  color: #334155;
  padding: 0.625rem 1.25rem;
}
.secondary-btn:hover { background: #f8fafc; border-color: #94a3b8; }
.danger-btn-sm {
  background: #fee2e2;
  color: #991b1b;
  padding: 0.375rem 0.75rem;
  font-size: 0.85rem;
}
.danger-btn-sm:hover:not(:disabled) { background: #fecaca; }

/* Modal */
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}
.modal-dialog {
  background: white;
  width: 100%;
  max-width: 480px;
  border-radius: 1rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  animation: modal-pop 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes modal-pop {
  from { transform: scale(0.95); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}
.modal-header {
  padding: 1.5rem;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.modal-header h2 { margin: 0; font-size: 1.25rem; color: #0f172a; }
.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  color: #94a3b8;
  cursor: pointer;
}
.modal-body { padding: 1.5rem; }
.modal-footer {
  padding: 1.25rem 1.5rem;
  background: #f8fafc;
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  border-top: 1px solid #e2e8f0;
}

/* Forms */
.form-group { margin-bottom: 1.25rem; }
.form-group label {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: #334155;
  margin-bottom: 0.5rem;
}
.input-field {
  width: 100%;
  padding: 0.625rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.5rem;
  font-size: 0.95rem;
  color: #1e293b;
}
.input-field:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.success-state {
  text-align: center;
  padding: 2rem;
}
.success-icon { font-size: 3rem; margin-bottom: 1rem; }
.full-width { width: 100%; }

/* Spinner */
.spinner {
  border: 3px solid #f3f3f3;
  border-top: 3px solid #3b82f6;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>
