<template>
  <div class="p-3 p-md-4 text-start">
    
    <!-- Page Header -->
    <div class="mb-4">
      <h2 class="fw-bold mb-0 page-title text-body">Settings</h2>
      <p class="text-muted small mb-0 mt-1">Manage appearance, personal account, and system notifications.</p>
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
              <h4 class="fw-bold text-body h5 mb-0">Application Theme</h4>
              <p class="text-muted small mb-0">Customize light/dark theme to suit your working environment.</p>
            </div>
          </div>
          
          <hr />

          <div class="d-flex align-items-center justify-content-between py-2">
            <div>
              <div class="fw-semibold text-body">Dark Mode</div>
              <p class="text-muted small mb-0">Switch the entire system to dark theme to protect your eyes.</p>
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
              <h4 class="fw-bold text-body h5 mb-0">Notification Settings</h4>
              <p class="text-muted small mb-0">Configure how you receive notifications from the system.</p>
            </div>
          </div>
          
          <hr />

          <div class="d-flex align-items-center justify-content-between py-2 border-bottom">
            <div>
              <div class="fw-semibold text-body">Email Notifications</div>
              <p class="text-muted small mb-0">Send summary emails when new tasks are assigned.</p>
            </div>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" role="switch" checked disabled />
            </div>
          </div>

          <div class="d-flex align-items-center justify-content-between py-2">
            <div>
              <div class="fw-semibold text-body">Browser Push Notifications</div>
              <p class="text-muted small mb-0">Show small notifications on the screen corner for real-time updates.</p>
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
            Management System
          </span>

          <hr class="my-4" />

          <div class="text-start">
            <div class="mb-3">
              <span class="text-secondary small d-block">Account name:</span>
              <span class="text-body fw-medium">{{ userEmail.split('@')[0] }}</span>
            </div>
            <div class="mb-3">
              <span class="text-secondary small d-block">Environment:</span>
              <span class="text-body fw-medium">SaaS Cloud Production</span>
            </div>
            <div>
              <span class="text-secondary small d-block">Connection status:</span>
              <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill">Active</span>
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
  toastSuccess(`Switched to ${isDarkMode.value ? 'Dark' : 'Light'} mode!`)
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
