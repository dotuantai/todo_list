<template>
  <div v-if="showAppShell" class="d-flex vh-100 overflow-hidden bg-body-tertiary">
    <!-- Sidebar -->
    <Sidebar @create-task="openCreateTaskModal" />

    <!-- Main Content Area -->
    <div class="flex-grow-1 d-flex flex-column overflow-hidden">
      <!-- Top Navbar -->
      <header class="top-navbar px-3 px-md-4 d-flex align-items-center justify-content-between border-bottom bg-body flex-shrink-0" style="height: 60px;">
        
        <!-- Left Side: Toggle button and logo on mobile -->
        <div class="d-flex align-items-center flex-grow-1" style="min-width: 0;">
          <button 
            class="btn btn-light border-0 p-2 d-lg-none me-2 rounded-2" 
            type="button" 
            data-bs-toggle="offcanvas" 
            data-bs-target="#sidebarMenu" 
            aria-controls="sidebarMenu"
          >
            <i class="bi bi-list fs-4"></i>
          </button>
          <div class="d-lg-none d-flex align-items-center gap-2">
            <div class="bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-2" :style="{ background: getProjectColor(projectStore.currentProject?.Name) }" style="width: 28px; height: 28px; font-size: 12px;">
              {{ projectStore.currentProject ? projectStore.currentProject.Name[0].toUpperCase() : 'P' }}
            </div>
            <span class="fw-bold text-body mb-0 fs-6 text-truncate">{{ projectStore.currentProject?.Name || $t('common.SCR0035') }}</span>
          </div>
        </div>

        <!-- Right Side: Tools (Language, Theme, Notification) -->
        <div class="d-flex align-items-center gap-2 ms-auto flex-shrink-0">
          <!-- Language Switcher -->
          <div class="dropdown">
            <button 
              class="btn btn-light p-0 border rounded-3 d-flex align-items-center justify-content-center" 
              style="width: 36px; height: 36px; outline: none; box-shadow: none;"
              type="button"
              data-bs-toggle="dropdown"
              aria-expanded="false"
              :title="$t('common.SCR0018')"
            >
              <span class="d-flex align-items-center justify-content-center">
                <svg v-if="locale === 'vi'" viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><circle cx="12" cy="12" r="12" fill="#da251d"/><polygon points="12,6 12.95,9.58 16.71,9.58 13.66,11.8 14.79,15.38 12,13.16 9.21,15.38 10.34,11.8 7.29,9.58 11.05,9.58" fill="#ffff00"/></svg>
                <svg v-else viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-btn-app"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-btn-app)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
              </span>
            </button>
            <ul class="dropdown-menu dropdown-menu-end shadow border-0 mt-2 p-1 rounded-3" style="min-width: 130px; z-index: 1060;">
              <li>
                <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="changeLocale('vi')">
                  <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><circle cx="12" cy="12" r="12" fill="#da251d"/><polygon points="12,6 12.95,9.58 16.71,9.58 13.66,11.8 14.79,15.38 12,13.16 9.21,15.38 10.34,11.8 7.29,9.58 11.05,9.58" fill="#ffff00"/></svg>
                  <span style="font-size: 0.85rem;">{{ $t('common.SCR0019') }}</span>
                </button>
              </li>
              <li>
                <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="changeLocale('en')">
                  <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-item-app"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-item-app)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
                  <span style="font-size: 0.85rem;">{{ $t('common.SCR0020') }}</span>
                </button>
              </li>
            </ul>
          </div>

          <!-- Theme Switcher -->
          <ThemeToggle />

          <!-- Notification Dropdown -->
          <NotificationDropdown />
          
          <div class="vr opacity-25 mx-1" style="height:28px"></div>
        </div>

        <!-- User Profile Dropdown: Occupies 2/10 (20%) of navbar width on desktop -->
        <div class="account-nav-section d-flex align-items-center justify-content-end" style="flex: 0 0 20%; max-width: 20%; min-width: 170px;">
          <div class="dropdown w-100">
            <button 
              class="btn p-1 border-0 bg-transparent d-flex align-items-center gap-2 text-decoration-none w-100 justify-content-end" 
              type="button" 
              data-bs-toggle="dropdown" 
              aria-expanded="false"
              style="outline: none; box-shadow: none;"
            >
              <div class="user-avatar-small bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle flex-shrink-0" style="width: 36px; height: 36px; font-size: 14px; background: linear-gradient(135deg, #059669, #10b981) !important;">
                {{ projectStore.currentInitial }}
              </div>
              
              <div class="d-none d-md-block text-start lh-sm min-w-0 flex-grow-1" style="overflow: hidden;">
                <div class="fw-semibold small text-body text-truncate" :title="projectStore.currentUserEmail">{{ projectStore.currentUserEmail }}</div>
                <div class="text-muted" style="font-size:10px; margin-top:2px" v-if="projectStore.currentProject">
                  <span class="badge text-uppercase font-monospace text-truncate d-inline-block" :class="getRoleBadgeClass(projectStore.userRole)" style="font-size: 8px; padding: 2px 4px; max-width: 100%;">{{ projectStore.userRole }}</span>
                </div>
              </div>
            </button>
            <ul class="dropdown-menu dropdown-menu-end shadow border-0 mt-2 p-2 rounded-3" style="min-width: 200px; font-size: 0.9rem; z-index: 1060;">
              <li class="px-3 py-2 border-bottom mb-1">
                <span class="d-block fw-bold text-truncate text-body" :title="projectStore.currentUserEmail">{{ projectStore.currentUserEmail }}</span>
                <span class="text-muted small">{{ $t('settings.SCR0617') }}</span>
              </li>
              <li>
                <router-link to="/change-password" class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2">
                  <i class="bi bi-shield-lock-fill text-primary"></i>
                  <span>{{ $t('changePassword.SCR0801') }}</span>
                </router-link>
              </li>
              <li>
                <button @click="handleLogout" class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2 text-danger">
                  <i class="bi bi-box-arrow-right"></i>
                  <span>{{ $t('sidebar.SCR0006') }}</span>
                </button>
              </li>
            </ul>
          </div>
        </div>
      </header>

      <!-- Main viewport -->
      <main class="flex-grow-1 overflow-auto">
        <router-view />
      </main>
    </div>
  </div>

  <router-view v-else />

  <TaskModal ref="createTaskModal" />
</template>

<script setup>
import { ref, computed, onMounted, watch, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import TaskModal from './components/TaskModal.vue'
import Sidebar from './components/Sidebar.vue'
import NotificationDropdown from './components/NotificationDropdown.vue'
import ThemeToggle from './components/ThemeToggle.vue'
import { logout } from './services/authService.js'
import { useProjectStore } from './stores/projectStore.js'
import { useSignalR } from './composables/useSignalR.js'
import { useTheme } from './composables/useTheme.js'

const router = useRouter()
const { locale } = useI18n()
const { initSignalR, stopSignalR } = useSignalR()
const { initTheme } = useTheme()

const changeLocale = (lang) => {
  locale.value = lang
  localStorage.setItem('locale', lang)
}
const route = router.currentRoute
const createTaskModal = ref(null)
const projectStore = useProjectStore()

watch(() => projectStore.isAuthenticated, (newVal) => {
  if (newVal) {
    initSignalR()
  } else {
    stopSignalR()
  }
})

const openCreateTaskModal = () => {
  createTaskModal.value?.openModal()
}

const showAppShell = computed(() => {
  return projectStore.isAuthenticated && !!route.value.params.projectId
})

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

const getProjectColor = (name) => {
  if (!name) return 'linear-gradient(135deg, #059669, #10b981)'
  const colors = [
    'linear-gradient(135deg, #059669, #10b981)',
    'linear-gradient(135deg, #10b981, #059669)',
    'linear-gradient(135deg, #f59e0b, #d97706)',
    'linear-gradient(135deg, #ef4444, #dc2626)',
    'linear-gradient(135deg, #ec4899, #db2777)',
    'linear-gradient(135deg, #06b6d4, #0891b2)',
    'linear-gradient(135deg, #8b5cf6, #7c3aed)'
  ]
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  const index = Math.abs(hash) % colors.length
  return colors[index]
}

onMounted(() => {
  initTheme()
  projectStore.decodeToken()
  if (projectStore.isAuthenticated) {
    initSignalR()
  }
})

onUnmounted(() => {
  stopSignalR()
})

const handleLogout = async () => {
  try {
    await logout()
  } catch (error) {
    console.error(error)
  } finally {
    stopSignalR()
    projectStore.clearStore()
    router.push('/login')      
  }
}
</script>

<style scoped>
.user-avatar-small {
  box-shadow: var(--shadow-sm);
  transition: opacity 0.2s ease;
}
.user-avatar-small:hover {
  opacity: 0.9;
}
</style>
