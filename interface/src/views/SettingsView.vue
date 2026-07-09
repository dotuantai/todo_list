<template>
  <div class="p-3 p-md-4 text-start">
    
    <!-- Page Header -->
    <div class="mb-4">
      <h2 class="fw-bold mb-0 page-title text-body">Cài đặt</h2>
      <p class="text-muted small mb-0 mt-1">Quản lý giao diện, tài khoản cá nhân và thông báo hệ thống.</p>
    </div>

    <div class="row g-4">
      <!-- Left Column: Settings Cards -->
      <div class="col-12 col-lg-8">
        
        <!-- Theme Switcher Card -->
        <div class="card border-0 shadow-sm rounded-3 p-4 mb-4">
          <div class="d-flex align-items-center gap-3 mb-3">
            <div class="bg-primary bg-opacity-10 text-primary rounded-3 p-2 d-flex align-items-center justify-content-center" style="width: 42px; height: 42px;">
              <i class="bi bi-palette-fill fs-5"></i>
            </div>
            <div>
              <h4 class="fw-bold text-body h5 mb-0">Giao diện ứng dụng</h4>
              <p class="text-muted small mb-0">Tùy chỉnh chủ đề sáng tối phù hợp với môi trường làm việc.</p>
            </div>
          </div>
          
          <hr />

          <div class="d-flex align-items-center justify-content-between py-2">
            <div>
              <div class="fw-semibold text-body">Chế độ tối (Dark Mode)</div>
              <p class="text-muted small mb-0">Chuyển đổi toàn bộ hệ thống sang giao diện tối để bảo vệ mắt.</p>
            </div>
            <div class="form-check form-switch fs-4">
              <input 
                v-model="isDarkMode" 
                class="form-check-input" 
                type="checkbox" 
                role="switch" 
                id="themeSwitch"
                @change="handleThemeToggle"
                style="cursor: pointer;"
              />
            </div>
          </div>
        </div>

        <!-- Notifications Card -->
        <div class="card border-0 shadow-sm rounded-3 p-4 mb-4">
          <div class="d-flex align-items-center gap-3 mb-3">
            <div class="bg-warning bg-opacity-10 text-warning rounded-3 p-2 d-flex align-items-center justify-content-center" style="width: 42px; height: 42px;">
              <i class="bi bi-bell-fill fs-5"></i>
            </div>
            <div>
              <h4 class="fw-bold text-body h5 mb-0">Thông báo cấu hình</h4>
              <p class="text-muted small mb-0">Cài đặt cách bạn nhận thông báo từ hệ thống.</p>
            </div>
          </div>
          
          <hr />

          <div class="d-flex align-items-center justify-content-between py-2 border-bottom">
            <div>
              <div class="fw-semibold text-body">Thông báo qua Email</div>
              <p class="text-muted small mb-0">Gửi mail tóm tắt khi có công việc mới được giao.</p>
            </div>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" role="switch" checked disabled />
            </div>
          </div>

          <div class="d-flex align-items-center justify-content-between py-2">
            <div>
              <div class="fw-semibold text-body">Thông báo đẩy trên trình duyệt</div>
              <p class="text-muted small mb-0">Hiện thông báo nhỏ ở góc màn hình khi có cập nhật tức thời.</p>
            </div>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" role="switch" />
            </div>
          </div>
        </div>

      </div>

      <!-- Right Column: Account Quick Card -->
      <div class="col-12 col-lg-4">
        <div class="card border-0 shadow-sm rounded-3 p-4 text-center">
          <div class="user-avatar-large mx-auto mb-3 bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width: 72px; height: 72px; font-size: 28px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
            {{ userInitial }}
          </div>
          <h4 class="fw-bold text-body mb-1">{{ userEmail }}</h4>
          <span class="badge text-uppercase font-monospace bg-light text-secondary border rounded-pill px-3 py-1.5" style="font-size: 10px;">
            Hệ thống quản lý
          </span>

          <hr class="my-4" />

          <div class="text-start">
            <div class="mb-3">
              <span class="text-secondary small d-block">Tên tài khoản:</span>
              <span class="text-body fw-medium">{{ userEmail.split('@')[0] }}</span>
            </div>
            <div class="mb-3">
              <span class="text-secondary small d-block">Môi trường:</span>
              <span class="text-body fw-medium">SaaS Cloud Production</span>
            </div>
            <div>
              <span class="text-secondary small d-block">Trạng thái kết nối:</span>
              <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill">Đang hoạt động</span>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { toastSuccess } from '../utils/swal.js'

const isDarkMode = ref(false)
const userEmail = ref(localStorage.getItem('userEmail') || 'User@example.com')

const userInitial = computed(() => {
  return userEmail.value ? userEmail.value[0].toUpperCase() : '?'
})

const handleThemeToggle = () => {
  const theme = isDarkMode.value ? 'dark' : 'light'
  document.documentElement.setAttribute('data-bs-theme', theme)
  localStorage.setItem('theme', theme)
  window.dispatchEvent(new CustomEvent('theme-changed', { detail: theme }))
  toastSuccess(`Đã chuyển sang chế độ ${isDarkMode.value ? 'Tối' : 'Sáng'}!`)
}

onMounted(() => {
  const currentTheme = document.documentElement.getAttribute('data-bs-theme') || localStorage.getItem('theme') || 'light'
  isDarkMode.value = (currentTheme === 'dark')
})
</script>

<style scoped>
.page-title {
  font-size: 1.68rem;
  letter-spacing: -0.02em;
}
</style>
