<template>
  <div class="p-3 p-md-4 text-start">
   

    <!-- Members Management Card -->
    <div class="card border-0 shadow-sm p-4 rounded-3 bg-body">
      <div class="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-3">
        <div class="text-start">
          <h4 class="fw-bold mb-1 text-body h5">{{ $t('members.SCR0501') }}</h4>
          <p class="text-muted small mb-0">{{ $t('members.SCR0502') }}</p>
        </div>
        
        <!-- Add member form (only visible to Owner) -->
        <div v-if="projectStore.userRole === 'Owner'" class="d-flex gap-2 align-items-center flex-wrap">
          <input 
            v-model="memberEmail" 
            type="email" 
            class="form-control rounded-2" 
            :placeholder="$t('members.SCR0503')"
            style="width: 250px;"
          />
          <select 
            v-model="memberRole" 
            class="form-select rounded-2" 
            style="width: 120px;"
          >
            <option value="Owner">{{ $t('common.SCR0027') }}</option>
            <option value="Manager">{{ $t('common.SCR0028') }}</option>
            <option value="Member">{{ $t('common.SCR0029') }}</option>
          </select>
          <button 
            class="btn btn-primary fw-semibold rounded-2 d-flex align-items-center justify-content-center px-3" 
            @click="addProjectMember"
            :disabled="!memberEmail"
          >
            {{ $t('members.SCR0504') }}
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loadingMembers" class="text-center py-5">
        <div class="spinner-border text-primary" role="status"></div>
      </div>

      <!-- Members Table -->
      <div v-else class="table-responsive">
        <table class="table table-hover align-middle border-0 mb-0">
          <thead class="table-light">
            <tr>
              <th scope="col" class="border-0 rounded-start text-start px-3 py-3">{{ $t('members.SCR0505') }}</th>
              <th scope="col" class="border-0 text-start px-3 py-3">{{ $t('members.SCR0506') }}</th>
              <th scope="col" class="border-0 text-start px-3 py-3">{{ $t('members.SCR0507') }}</th>
              <th scope="col" class="border-0 rounded-end text-end px-3 py-3" style="width: 120px;" v-if="projectStore.userRole === 'Owner'">{{ $t('members.SCR0508') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr 
              v-for="user in members" 
              :key="user.UserId" 
              class="border-bottom"
              :class="{ 'current-user-row': user.Email?.toLowerCase() === projectStore.currentUserEmail?.toLowerCase() }"
            >
              <td class="text-start p-3">
                <div class="d-flex align-items-center gap-3">
                  <div class="user-avatar text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" :style="{ background: getUserColor(user.Email) }" style="width:38px; height:38px;">
                    {{ userInitial(user.Email) }}
                  </div>
                  <div class="text-start">
                    <div class="fw-semibold text-body">
                      {{ user.Email }}
                      <span v-if="user.Email?.toLowerCase() === projectStore.currentUserEmail?.toLowerCase()" class="badge bg-primary-subtle text-primary border border-primary-subtle rounded-pill px-2 py-0.5 ms-1.5" style="font-size:0.65rem; text-transform: none; font-weight: 600;">{{ $t('members.SCR0510') }}</span>
                    </div>
                    <div class="text-muted font-monospace" style="font-size:10px;">ID: {{ user.UserId }}</div>
                  </div>
                </div>
              </td>
              <td class="text-start p-3">
                <select 
                  v-if="projectStore.userRole === 'Owner' && projectStore.currentProject?.OwnerId !== user.UserId"
                  :value="user.Role"
                  @change="changeMemberRole(user, $event.target.value)"
                  class="form-select form-select-sm rounded-2"
                  style="width: 110px;"
                >
                  <option value="Owner">{{ $t('common.SCR0027') }}</option>
                  <option value="Manager">{{ $t('common.SCR0028') }}</option>
                  <option value="Member">{{ $t('common.SCR0029') }}</option>
                </select>
                <span v-else class="badge text-uppercase font-monospace" :class="getRoleBadgeClass(user.Role)" style="font-size: 10px; padding: 4px 8px;">
                  {{ getRoleLabel(t, user.Role) }}
                </span>
              </td>
              <td class="text-muted small text-start p-3">
                {{ formatDate(user.JoinedAt) }}
              </td>
              <td class="text-end p-3" v-if="projectStore.userRole === 'Owner'">
                <button 
                  v-if="projectStore.currentProject?.OwnerId !== user.UserId"
                  class="btn btn-sm btn-outline-danger rounded-2 px-2 py-1" 
                  @click="removeProjectMember(user)"
                >
                  {{ $t('members.SCR0509') }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { getRoleLabel } from '../utils/i18nLabels.js'
import { getMembers, addMember, updateMemberRole, removeMember } from '../services/projectService.js'
import { useProjectStore } from '../stores/projectStore.js'
import { toastSuccess, toastError, confirm, extractMessage } from '../utils/swal.js'

const route = useRoute()
const projectStore = useProjectStore()
const { t } = useI18n()
const members = ref([])
const loadingMembers = ref(false)
const memberEmail = ref('')
const memberRole = ref('Manager')

const projectId = computed(() => route.params.projectId)

const loadMembers = async () => {
  if (!projectId.value) return
  loadingMembers.value = true
  try {
    const res = await getMembers(projectId.value)
    members.value = res?.data || []
  } catch (err) {
    console.error('Failed to load project members', err)
  } finally {
    loadingMembers.value = false
  }
}

const addProjectMember = async () => {
  if (!memberEmail.value || !memberEmail.value.trim() || !projectId.value) return
  try {
    await addMember(projectId.value, memberEmail.value.trim(), memberRole.value)
    toastSuccess(t('members.SCR0513'))
    memberEmail.value = ''
    await loadMembers()
    window.dispatchEvent(new CustomEvent('project-members-changed'))
  } catch (err) {
    toastError(extractMessage(err, t('common.SCR0015')))
  }
}

const changeMemberRole = async (user, newRole) => {
  if (!projectId.value) return
  try {
    await updateMemberRole(projectId.value, user.UserId, newRole)
    toastSuccess(t('members.SCR0514'))
    await loadMembers()
  } catch (err) {
    toastError(extractMessage(err, t('common.SCR0015')))
  }
}

const removeProjectMember = async (user) => {
  if (!projectId.value) return
  const ok = await confirm(
    t('members.SCR0511'),
    t('members.SCR0512', { email: user.Email }),
    t('members.SCR0509')
  )
  if (!ok) return
  try {
    await removeMember(projectId.value, user.UserId)
    toastSuccess(t('members.SCR0515'))
    await loadMembers()
    window.dispatchEvent(new CustomEvent('project-members-changed'))
  } catch (err) {
    toastError(extractMessage(err, t('common.SCR0015')))
  }
}

const getRoleBadgeClass = (role) => {
  switch (role?.toLowerCase()) {
    case 'owner':
      return 'bg-danger-subtle text-danger border border-danger-subtle'
    case 'manager':
      return 'bg-primary-subtle text-primary border border-primary-subtle'
    case 'member':
    default:
      return 'bg-secondary-subtle text-secondary border border-secondary-subtle'
  }
}

const getUserColor = (email) => {
  if (!email) return '#6366f1'
  const colors = ['#4f46e5', '#10b981', '#f59e0b', '#ef4444', '#ec4899', '#06b6d4', '#8b5cf6']
  let hash = 0
  for (let i = 0; i < email.length; i++) {
    hash = email.charCodeAt(i) + ((hash << 5) - hash)
  }
  const index = Math.abs(hash) % colors.length
  return colors[index]
}

const userInitial = (email) => email ? email[0].toUpperCase() : '?'
const formatDate = (d) => d ? new Date(d).toLocaleDateString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—'

watch(projectId, () => {
  loadMembers()
})

onMounted(() => {
  loadMembers()
})
</script>

<style scoped>
.page-title {
  font-size: 1.68rem;
  letter-spacing: -0.02em;
  font-weight: 700;
  color: var(--bs-heading-color) !important;
}
.current-user-row td {
  background-color: rgba(99, 102, 241, 0.05) !important;
}
</style>
