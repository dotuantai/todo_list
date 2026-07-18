<template>
  <div v-if="showAppShell" class="d-flex vh-100 overflow-hidden bg-body-tertiary">
    <!-- Sidebar (Responsive Offcanvas) -->
    <nav 
      class="offcanvas-lg offcanvas-start bg-body border-end d-flex flex-column flex-shrink-0" 
      tabindex="-1" 
      id="sidebarMenu" 
      aria-labelledby="sidebarMenuLabel"
      style="width: 260px; height: 100vh; z-index: 1045;"
    >
      <!-- Sidebar Header -->
      <div class="px-4 py-3 border-bottom d-flex align-items-center justify-content-between">
        <router-link to="/projects" class="d-flex align-items-center gap-3 text-decoration-none" style="cursor: pointer;">
          <div class="logo-box text-white d-flex align-items-center justify-content-center fw-bold fs-5 rounded-3" :style="{ background: getProjectColor(projectStore.currentProject?.Name) }" style="width: 38px; height: 38px; min-width: 38px;">
            {{ projectStore.currentProject ? projectStore.currentProject.Name[0].toUpperCase() : 'P' }}
          </div>
          <div class="text-start min-w-0">
            <h1 class="mb-0 fs-6 fw-bold text-body lh-1 text-truncate" style="max-width: 140px;" id="sidebarMenuLabel">
              {{ projectStore.currentProject?.Name || 'Project' }}
            </h1>
            <p class="small text-muted mb-0 mt-1" style="font-size: 10px;">Switch Workspace</p>
          </div>
        </router-link>
        <!-- Close button visible only on mobile/tablet -->
        <button type="button" class="btn-close d-lg-none" data-bs-dismiss="offcanvas" data-bs-target="#sidebarMenu" aria-label="Close"></button>
      </div>

      <!-- Action Button: Create Task -->
      <div class="px-3 py-3" v-if="projectStore.currentProjectId && projectStore.userRole !== 'Viewer'">
        <button class="btn btn-primary w-100 py-2 fw-semibold d-flex align-items-center justify-content-center gap-2 shadow-sm" style="border-radius: 10px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;" @click="openCreateTaskModal">
          <i class="bi bi-plus-lg"></i> Create Task
        </button>
      </div>

      <!-- Sidebar Navigation Menu -->
      <div class="flex-grow-1 px-2 overflow-auto py-2">
        <ul class="nav flex-column gap-1 mb-4">
          <!-- Dashboard link -->
          <li class="nav-item">
            <router-link :to="`/projects/${projectStore.currentProjectId}/dashboard`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm">
              <i class="bi bi-grid-1x2-fill fs-6"></i>
              <span class="small fw-medium">Dashboard</span>
            </router-link>
          </li>
          <!-- Task Board link -->
          <li class="nav-item">
            <router-link :to="`/projects/${projectStore.currentProjectId}/tasks`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm">
              <i class="bi bi-kanban fs-6"></i>
              <span class="small fw-medium">Task Board</span>
            </router-link>
          </li>
          <!-- Members link -->
          <li class="nav-item">
            <router-link :to="`/projects/${projectStore.currentProjectId}/members`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm">
              <i class="bi bi-people-fill fs-6"></i>
              <span class="small fw-medium">Members</span>
            </router-link>
          </li>
          <!-- Settings link -->
          <li class="nav-item">
            <router-link :to="`/projects/${projectStore.currentProjectId}/settings`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm">
              <i class="bi bi-gear-fill fs-6"></i>
              <span class="small fw-medium">Settings</span>
            </router-link>
          </li>
        </ul>
      </div>

      <!-- Sidebar Footer -->
      <div class="px-2 py-3 border-top">
        <ul class="nav flex-column gap-1">
          <li class="nav-item">
            <button
              @click="handleLogout"
              class="btn btn-link nav-link sidebar-link-danger d-flex align-items-center gap-2 px-3 py-2 rounded-3 w-100 text-start text-decoration-none border-0 bg-transparent">
              <i class="bi bi-box-arrow-right fs-6"></i>
              <span class="small fw-medium">Sign out</span>
            </button>
          </li>
        </ul>
      </div>
    </nav>

    <!-- Main Content Area -->
    <div class="flex-grow-1 d-flex flex-column overflow-hidden">
      <!-- Top Navbar -->
      <header class="top-navbar px-3 px-md-4 d-flex align-items-center justify-content-between border-bottom bg-body flex-shrink-0" style="height: 60px;">
        
        <!-- Toggle button and logo on mobile -->
        <div class="d-flex align-items-center">
          <button 
            class="btn btn-light border-0 p-2 d-lg-none me-2" 
            type="button" 
            data-bs-toggle="offcanvas" 
            data-bs-target="#sidebarMenu" 
            aria-controls="sidebarMenu"
            style="border-radius: 8px;"
          >
            <i class="bi bi-list fs-4"></i>
          </button>
          <div class="d-lg-none d-flex align-items-center gap-2">
            <div class="bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-2" :style="{ background: getProjectColor(projectStore.currentProject?.Name) }" style="width: 28px; height: 28px; font-size: 12px;">
              {{ projectStore.currentProject ? projectStore.currentProject.Name[0].toUpperCase() : 'P' }}
            </div>
            <span class="fw-bold text-body mb-0 fs-6">{{ projectStore.currentProject?.Name || 'Project' }}</span>
          </div>
        </div>

        <!-- Right Side: Profile & notification -->
        <div class="d-flex align-items-center gap-2 ms-auto">
          <!-- Notification system -->
          <button class="btn btn-light p-0 border rounded-3 d-flex align-items-center justify-content-center" style="width: 36px; height: 36px; color: #64748b; position: relative;">
            <i class="bi bi-bell"></i>
            <span class="position-absolute bg-danger border border-white rounded-circle" style="width: 7px; height: 7px; top: 8px; right: 8px;"></span>
          </button>
          
          <div class="vr opacity-25 mx-1" style="height:28px"></div>
          
          <!-- User info details -->
          <div class="d-flex align-items-center gap-2">
            <div class="user-avatar-small bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width: 36px; height: 36px; font-size: 14px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
              {{ projectStore.currentInitial }}
            </div>
            
            <div class="d-none d-md-block text-start" style="line-height: 1.2;">
              <div class="fw-semibold small text-body text-truncate" style="max-width: 150px;" :title="projectStore.currentUserEmail">{{ projectStore.currentUserEmail }}</div>
              <div class="text-muted" style="font-size:10px; margin-top:2px" v-if="projectStore.currentProject">
                <span class="badge text-uppercase font-monospace" :class="getRoleBadgeClass(projectStore.userRole)" style="font-size: 8px; padding: 2px 4px;">{{ projectStore.userRole }}</span>
              </div>
            </div>
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
import TaskModal from './components/TaskModal.vue'
import { logout } from './services/authService.js'
import { useProjectStore } from './stores/projectStore.js'
import { getMembers } from './services/projectService.js'

const router = useRouter()
const route = router.currentRoute
const createTaskModal = ref(null)
const projectStore = useProjectStore()
const sidebarMembers = ref([])

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
    case 'editor':
      return 'bg-primary-subtle text-primary border border-primary-subtle'
    case 'viewer':
    default:
      return 'bg-secondary-subtle text-secondary border border-secondary-subtle'
  }
}

const getProjectColor = (name) => {
  if (!name) return 'linear-gradient(135deg, #4f46e5, #6366f1)'
  const colors = [
    'linear-gradient(135deg, #4f46e5, #6366f1)',
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

const getUserColor = (email) => {
  if (!email) return '#4f46e5'
  const colors = ['#4f46e5', '#10b981', '#f59e0b', '#ef4444', '#ec4899', '#06b6d4', '#8b5cf6']
  let hash = 0
  for (let i = 0; i < email.length; i++) {
    hash = email.charCodeAt(i) + ((hash << 5) - hash)
  }
  const index = Math.abs(hash) % colors.length
  return colors[index]
}

const loadSidebarMembers = async () => {
  if (!route.value.params.projectId) {
    sidebarMembers.value = []
    return
  }
  try {
    const res = await getMembers(route.value.params.projectId)
    sidebarMembers.value = res?.data || []
  } catch (e) {
    console.error('Failed to load sidebar members:', e)
  }
}

const getPreferredTheme = () => {
  const saved = localStorage.getItem('theme')
  if (saved) return saved
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
  return prefersDark ? 'dark' : 'light'
}

watch(() => route.value.params.projectId, (newId) => {
  if (newId) {
    loadSidebarMembers()
  } else {
    sidebarMembers.value = []
  }
})

onMounted(() => {
  const currentTheme = getPreferredTheme()
  document.documentElement.setAttribute('data-bs-theme', currentTheme)
  projectStore.decodeToken()
  window.addEventListener('project-members-changed', loadSidebarMembers)
  if (route.value.params.projectId) {
    loadSidebarMembers()
  }
})

onUnmounted(() => {
  window.removeEventListener('project-members-changed', loadSidebarMembers)
})

const handleLogout = async () => {
  try {
    await logout()
  } catch (error) {
    console.error(error)
  } finally {
    localStorage.removeItem('token')
    projectStore.clearStore()
    router.push('/login')      
  }
}
</script>

<style scoped>
.sidebar-link {
  transition: background 0.15s, color 0.15s;
  color: var(--bs-secondary-color) !important;
}
.sidebar-link:hover {
  background: var(--bs-secondary-bg) !important;
  color: var(--bs-heading-color) !important;
}
.active-project {
  background: linear-gradient(135deg, #4f46e5, #6366f1) !important;
  color: #fff !important;
}
.active-project i,
.active-project span {
  color: #fff !important;
}
.sidebar-link-danger {
  color: var(--bs-danger) !important;
}
.sidebar-link-danger:hover {
  background: rgba(var(--bs-danger-rgb), 0.12) !important;
  color: var(--bs-danger) !important;
}
</style>