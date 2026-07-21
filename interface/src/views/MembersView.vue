<template>
  <div class="p-3 p-md-4 text-start">
   

    <!-- Members Management Card -->
    <div class="card border-0 shadow-sm p-4 rounded-3 bg-body">
      <div class="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-3">
        <div class="text-start">
          <h4 class="fw-bold mb-1 text-body h5">Project Members</h4>
          <p class="text-muted small mb-0">List of team members participating in this workspace.</p>
        </div>
        
        <!-- Add member form (only visible to Owner) -->
        <div v-if="projectStore.userRole === 'Owner'" class="d-flex gap-2 align-items-center flex-wrap">
          <input 
            v-model="memberEmail" 
            type="email" 
            class="form-control form-control-sm" 
            placeholder="Enter member email..."
            style="width: 250px; border-radius: 8px; height: 38px;"
          />
          <select 
            v-model="memberRole" 
            class="form-select form-select-sm" 
            style="width: 120px; border-radius: 8px; height: 38px;"
          >
            <option value="Owner">Owner</option>
            <option value="Manager">Manager</option>
            <option value="Member">Member</option>
          </select>
          <button 
            class="btn btn-sm btn-primary fw-semibold d-flex align-items-center justify-content-center" 
            @click="addProjectMember"
            :disabled="!memberEmail"
            style="border-radius: 8px; height: 38px; padding: 0 16px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
          >
            Add Member
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
              <th scope="col" class="border-0 rounded-start text-start" style="padding: 12px 16px;">Member</th>
              <th scope="col" class="border-0 text-start" style="padding: 12px 16px;">Role</th>
              <th scope="col" class="border-0 text-start" style="padding: 12px 16px;">Joined Date</th>
              <th scope="col" class="border-0 rounded-end text-end" style="padding: 12px 16px; width: 120px;" v-if="projectStore.userRole === 'Owner'">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr 
              v-for="user in members" 
              :key="user.UserId" 
              class="border-bottom"
              :class="{ 'current-user-row': user.Email?.toLowerCase() === projectStore.currentUserEmail?.toLowerCase() }"
            >
              <td style="padding: 16px;" class="text-start">
                <div class="d-flex align-items-center gap-3">
                  <div class="user-avatar text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" :style="{ background: getUserColor(user.Email) }" style="width:38px; height:38px;">
                    {{ userInitial(user.Email) }}
                  </div>
                  <div class="text-start">
                    <div class="fw-semibold text-body">
                      {{ user.Email }}
                      <span v-if="user.Email?.toLowerCase() === projectStore.currentUserEmail?.toLowerCase()" class="badge bg-primary-subtle text-primary border border-primary-subtle rounded-pill px-2 py-0.5 ms-1.5" style="font-size:0.65rem; text-transform: none; font-weight: 600;">You</span>
                    </div>
                    <div class="text-muted font-monospace" style="font-size:10px;">ID: {{ user.UserId }}</div>
                  </div>
                </div>
              </td>
              <td style="padding: 16px;" class="text-start">
                <select 
                  v-if="projectStore.userRole === 'Owner' && projectStore.currentProject?.OwnerId !== user.UserId"
                  :value="user.Role"
                  @change="changeMemberRole(user, $event.target.value)"
                  class="form-select form-select-sm"
                  style="width: 110px; border-radius: 8px;"
                >
                  <option value="Owner">Owner</option>
                  <option value="Manager">Manager</option>
                  <option value="Member">Member</option>
                </select>
                <span v-else class="badge text-uppercase font-monospace" :class="getRoleBadgeClass(user.Role)" style="font-size: 10px; padding: 4px 8px;">
                  {{ user.Role }}
                </span>
              </td>
              <td class="text-muted small text-start" style="padding: 16px;">
                {{ formatDate(user.JoinedAt) }}
              </td>
              <td class="text-end" style="padding: 16px;" v-if="projectStore.userRole === 'Owner'">
                <button 
                  v-if="projectStore.currentProject?.OwnerId !== user.UserId"
                  class="btn btn-sm btn-outline-danger" 
                  @click="removeProjectMember(user)"
                  style="border-radius: 8px; padding: 4px 10px;"
                >
                  Remove
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
import { getMembers, addMember, updateMemberRole, removeMember } from '../services/projectService.js'
import { useProjectStore } from '../stores/projectStore.js'
import { toastSuccess, toastError, confirm, extractMessage } from '../utils/swal.js'

const route = useRoute()
const projectStore = useProjectStore()
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
    toastSuccess('Member added successfully!')
    memberEmail.value = ''
    await loadMembers()
    window.dispatchEvent(new CustomEvent('project-members-changed'))
  } catch (err) {
    toastError(extractMessage(err, 'Failed to add member.'))
  }
}

const changeMemberRole = async (user, newRole) => {
  if (!projectId.value) return
  try {
    await updateMemberRole(projectId.value, user.UserId, newRole)
    toastSuccess('Member role updated successfully!')
    await loadMembers()
  } catch (err) {
    toastError(extractMessage(err, 'Failed to update role.'))
  }
}

const removeProjectMember = async (user) => {
  if (!projectId.value) return
  const ok = await confirm(
    'Remove member?',
    `Are you sure you want to remove <strong>${user.Email}</strong> from the project?`,
    'Remove'
  )
  if (!ok) return
  try {
    await removeMember(projectId.value, user.UserId)
    toastSuccess('Member removed from project!')
    await loadMembers()
    window.dispatchEvent(new CustomEvent('project-members-changed'))
  } catch (err) {
    toastError(extractMessage(err, 'Failed to remove member.'))
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
}
.current-user-row td {
  background-color: rgba(99, 102, 241, 0.08) !important;
}
</style>
