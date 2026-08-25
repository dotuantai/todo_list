<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3 class="fw-bold mb-0 text-body">{{ $t('admin.users_title') }}</h3>
      <div class="d-flex align-items-center gap-3">
        <div class="text-muted small">
          {{ $t('admin.users_total') }} <span class="fw-bold text-primary">{{ users.length }}</span> {{ $t('admin.users_members') }}
        </div>
        <button class="btn btn-primary btn-sm d-flex align-items-center gap-2 shadow-sm" @click="showCreateModal = true">
          <i class="bi bi-person-plus-fill"></i>
          {{ $t('admin.users_add_btn') }}
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">{{ $t('admin.users_loading') }}</p>
    </div>

    <!-- Data Table for Users -->
    <div v-else class="bg-white rounded-4 shadow-sm border overflow-hidden">
      <div class="table-responsive">
        <table class="table table-hover mb-0 align-middle">
          <thead class="bg-light">
            <tr>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.users_col_account') }}</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.users_col_role') }}</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.users_col_status') }}</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.users_col_joined') }}</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary text-end" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.users_col_actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in users" :key="user.UserId" class="user-row">
              <td class="px-4 py-3">
                <div class="d-flex align-items-center gap-3">
                  <div class="user-avatar text-white d-flex align-items-center justify-content-center fw-bold rounded-circle shadow-sm" :style="{ background: getAvatarColor(user.Email) }" style="width: 36px; height: 36px;">
                    {{ user.Email ? user.Email[0].toUpperCase() : 'U' }}
                  </div>
                  <div>
                    <h6 class="mb-0 fw-bold">{{ user.Email }}</h6>
                    <small class="text-muted text-truncate d-inline-block" style="font-size: 11px;">ID: {{ user.UserId }}</small>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3">
                <span class="badge" :class="getRoleBadge(user.Role)">
                  {{ user.Role }}
                </span>
              </td>
              <td class="px-4 py-3">
                <div class="d-flex align-items-center gap-2">
                  <span class="status-indicator" :class="user.IsActive ? 'bg-success' : 'bg-secondary'"></span>
                  <span :class="user.IsActive ? 'text-success' : 'text-secondary'" style="font-size: 0.85rem; font-weight: 500;">
                    {{ user.IsActive ? $t('admin.users_status_active') : $t('admin.users_status_locked') }}
                  </span>
                </div>
              </td>
              <td class="px-4 py-3 text-muted small">
                {{ formatDate(user.CreatedAt) }}
              </td>
              <td class="px-4 py-3 text-end">
                <div class="dropdown" v-if="user.Role !== 'Admin'">
                  <button class="btn btn-sm btn-light border-0" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="bi bi-three-dots-vertical"></i>
                  </button>
                  <ul class="dropdown-menu dropdown-menu-end shadow-sm border-0" style="font-size: 0.85rem;">
                    <li>
                      <button class="dropdown-item py-2" @click="handleToggleRole(user)">
                        <i class="bi bi-arrow-up-circle me-2 text-primary" v-if="user.Role === 'Member'"></i>
                        <i class="bi bi-arrow-down-circle me-2 text-warning" v-else></i>
                        {{ user.Role === 'Member' ? $t('admin.users_action_upgrade') : $t('admin.users_action_downgrade') }}
                      </button>
                    </li>
                    <li>
                      <button class="dropdown-item py-2" @click="handleResetPassword(user)">
                        <i class="bi bi-key-fill me-2 text-info"></i>
                        {{ $t('admin.users_action_reset_pwd') }}
                      </button>
                    </li>
                    <li>
                      <button class="dropdown-item py-2" @click="handleToggleStatus(user)" :class="user.IsActive ? 'text-danger' : 'text-success'">
                        <i class="bi bi-lock-fill me-2" v-if="user.IsActive"></i>
                        <i class="bi bi-unlock-fill me-2" v-else></i>
                        {{ user.IsActive ? $t('admin.users_action_lock') : $t('admin.users_action_unlock') }}
                      </button>
                    </li>
                  </ul>
                </div>
              </td>
            </tr>
            <tr v-if="users.length === 0">
              <td colspan="5" class="text-center py-5 text-muted">
                {{ $t('admin.users_empty') }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

    <!-- Create User Modal -->
    <div class="modal fade" id="createUserModal" tabindex="-1" aria-hidden="true" :class="{ 'show d-block': showCreateModal }" :style="{ background: showCreateModal ? 'rgba(0,0,0,0.5)' : 'none' }">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-0 shadow">
          <div class="modal-header border-bottom-0 pb-0">
            <h5 class="modal-title fw-bold">{{ $t('admin.users_modal_title') }}</h5>
            <button type="button" class="btn-close" @click="showCreateModal = false"></button>
          </div>
          <div class="modal-body py-4">
            <form @submit.prevent="handleCreateUser">
              <div class="mb-3">
                <label class="form-label fw-semibold small">{{ $t('admin.users_modal_name') }} <span class="text-danger">*</span></label>
                <input type="text" class="form-control" v-model="createForm.FullName" required placeholder="Nguyễn Văn A">
              </div>
              <div class="mb-3">
                <label class="form-label fw-semibold small">{{ $t('admin.users_modal_email') }} <span class="text-danger">*</span></label>
                <input type="email" class="form-control" v-model="createForm.Email" required placeholder="email@company.com">
              </div>
              <div class="mb-4">
                <label class="form-label fw-semibold small">{{ $t('admin.users_modal_role') }} <span class="text-danger">*</span></label>
                <select class="form-select" v-model="createForm.Role" required>
                  <option value="Member">Member</option>
                  <option value="Manager">Manager</option>
                </select>
              </div>
              <div class="d-flex justify-content-end gap-2">
                <button type="button" class="btn btn-light" @click="showCreateModal = false" :disabled="creatingUser">{{ $t('admin.users_modal_cancel') }}</button>
                <button type="submit" class="btn btn-primary d-flex align-items-center gap-2" :disabled="creatingUser">
                  <span v-if="creatingUser" class="spinner-border spinner-border-sm" role="status"></span>
                  {{ creatingUser ? $t('admin.users_modal_creating') : $t('admin.users_modal_create_btn') }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue'
import { getAllUsers, updateUserRole, updateUserStatus, createUser, resetTemporaryPassword } from '../../services/authService.js'
import { toastSuccess, toastError, extractMessage, confirm as swalConfirm } from '../../utils/swal.js'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const users = ref([])
const loading = ref(false)

const showCreateModal = ref(false)
const creatingUser = ref(false)
const createForm = reactive({
  FullName: '',
  Email: '',
  Role: 'Member'
})

onMounted(async () => {
  await fetchUsers()
})

const fetchUsers = async () => {
  loading.value = true
  try {
    const res = await getAllUsers()
    if (res?.data) {
      users.value = Array.isArray(res.data) ? res.data : (res.data.Data || [])
    }
  } catch (error) {
    console.error('Failed to fetch users:', error)
  } finally {
    loading.value = false
  }
}

const handleToggleRole = async (user) => {
  const newRole = user.Role === 'Member' ? 'Manager' : 'Member'
  const title = t('admin.users_swal_role_title')
  const text = t('admin.users_swal_role_text', { email: user.Email, role: newRole })
  const confirmBtn = t('admin.users_swal_confirm_btn')
  
  const isConfirmed = await swalConfirm(title, text, confirmBtn)
  if (!isConfirmed) return
  
  try {
    await updateUserRole(user.UserId, newRole)
    user.Role = newRole
    toastSuccess(t('admin.users_toast_role_success', { role: newRole }))
  } catch (error) {
    toastError(extractMessage(error, t('admin.users_toast_role_fail')))
  }
}

const handleToggleStatus = async (user) => {
  const newStatus = !user.IsActive
  const actionText = newStatus ? t('admin.users_swal_status_action_unlock') : t('admin.users_swal_status_action_lock')
  
  const title = t('admin.users_swal_status_title', { action: actionText })
  const text = t('admin.users_swal_status_text', { action: actionText, email: user.Email })
  const confirmBtn = t('admin.users_swal_confirm_btn')
  
  const isConfirmed = await swalConfirm(title, text, confirmBtn)
  if (!isConfirmed) return
  
  try {
    await updateUserStatus(user.UserId, newStatus)
    user.IsActive = newStatus
    toastSuccess(newStatus ? t('admin.users_toast_active') : t('admin.users_toast_locked'))
  } catch (error) {
    toastError(extractMessage(error, t('admin.users_toast_status_fail')))
  }
}

const handleCreateUser = async () => {
  if (!createForm.Email || !createForm.FullName) return
  creatingUser.value = true
  try {
    await createUser(createForm)
    toastSuccess(t('admin.users_toast_create_success'))
    showCreateModal.value = false
    createForm.FullName = ''
    createForm.Email = ''
    createForm.Role = 'Member'
    await fetchUsers()
  } catch (error) {
    toastError(extractMessage(error, t('admin.users_toast_create_fail')))
  } finally {
    creatingUser.value = false
  }
}

const handleResetPassword = async (user) => {
  const title = t('admin.users_swal_reset_title')
  const text = t('admin.users_swal_reset_text', { email: user.Email })
  const confirmBtn = t('admin.users_swal_confirm_btn')
  
  const isConfirmed = await swalConfirm(title, text, confirmBtn)
  if (!isConfirmed) return
  
  try {
    await resetTemporaryPassword(user.UserId)
    toastSuccess(t('admin.users_toast_reset_success'))
  } catch (error) {
    toastError(extractMessage(error, t('admin.users_toast_reset_fail')))
  }
}

const getRoleBadge = (role) => {
  if (role === 'Admin') return 'bg-danger-subtle text-danger border border-danger-subtle'
  if (role === 'Manager') return 'bg-primary-subtle text-primary border border-primary-subtle'
  return 'bg-secondary-subtle text-secondary border border-secondary-subtle'
}

const getAvatarColor = (email) => {
  if (!email) return '#6366F1'
  const colors = ['#6366F1', '#10B981', '#F59E0B', '#EF4444', '#EC4899', '#06B6D4', '#8B5CF6']
  let hash = 0
  for (let i = 0; i < email.length; i++) {
    hash = email.charCodeAt(i) + ((hash << 5) - hash)
  }
  return colors[Math.abs(hash) % colors.length]
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}
</script>

<style scoped>
.user-row {
  transition: background-color 0.15s ease;
}
.user-row:hover {
  background-color: rgba(99, 102, 241, 0.03);
}
.status-indicator {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
}
</style>
