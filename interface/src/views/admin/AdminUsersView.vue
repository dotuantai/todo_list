<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3 class="fw-bold mb-0 text-body">Danh sách Nhân sự</h3>
      <div class="text-muted small">
        Tổng số: <span class="fw-bold text-primary">{{ users.length }}</span> thành viên
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">Đang tải dữ liệu nhân sự...</p>
    </div>

    <!-- Data Table for Users -->
    <div v-else class="bg-white rounded-4 shadow-sm border overflow-hidden">
      <div class="table-responsive">
        <table class="table table-hover mb-0 align-middle">
          <thead class="bg-light">
            <tr>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">Tài khoản</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">Phân quyền</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">Trạng thái</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">Ngày tham gia</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary text-end" style="font-size: 0.75rem; letter-spacing: 0.5px;">Hành động</th>
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
                    {{ user.IsActive ? 'Hoạt động' : 'Đã khóa' }}
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
                        {{ user.Role === 'Member' ? 'Nâng cấp lên Manager' : 'Hạ cấp xuống Member' }}
                      </button>
                    </li>
                    <li>
                      <button class="dropdown-item py-2" @click="handleToggleStatus(user)" :class="user.IsActive ? 'text-danger' : 'text-success'">
                        <i class="bi bi-lock-fill me-2" v-if="user.IsActive"></i>
                        <i class="bi bi-unlock-fill me-2" v-else></i>
                        {{ user.IsActive ? 'Khóa tài khoản' : 'Mở khóa tài khoản' }}
                      </button>
                    </li>
                  </ul>
                </div>
              </td>
            </tr>
            <tr v-if="users.length === 0">
              <td colspan="5" class="text-center py-5 text-muted">
                Không tìm thấy dữ liệu thành viên
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getAllUsers, updateUserRole, updateUserStatus } from '../../services/authService.js'
import { toastSuccess, toastError, extractMessage } from '../../utils/swal.js'

const users = ref([])
const loading = ref(false)

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
  try {
    await updateUserRole(user.UserId, newRole)
    user.Role = newRole
    toastSuccess(`Đã thay đổi quyền thành ${newRole}`)
  } catch (error) {
    toastError(extractMessage(error, 'Thay đổi quyền thất bại'))
  }
}

const handleToggleStatus = async (user) => {
  const newStatus = !user.IsActive
  try {
    await updateUserStatus(user.UserId, newStatus)
    user.IsActive = newStatus
    toastSuccess(newStatus ? 'Đã kích hoạt tài khoản' : 'Đã khóa tài khoản')
  } catch (error) {
    toastError(extractMessage(error, 'Thay đổi trạng thái thất bại'))
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
