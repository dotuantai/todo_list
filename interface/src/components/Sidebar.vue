<template>
  <nav 
    class="offcanvas-lg offcanvas-start bg-body border-end d-flex flex-column flex-shrink-0" 
    tabindex="-1" 
    id="sidebarMenu" 
    aria-labelledby="sidebarMenuLabel"
    style="width: 260px; height: 100vh; z-index: 1045;"
  >
    <!-- Sidebar Header -->
    <div class="px-4 py-3 border-bottom d-flex align-items-center justify-content-between">
      <router-link to="/projects" class="d-flex align-items-center gap-3 text-decoration-none" style="cursor: pointer;" @click="closeMobileMenu">
        <div class="logo-box text-white d-flex align-items-center justify-content-center fw-bold fs-5 rounded-3" :style="{ background: getProjectColor(projectStore.currentProject?.Name) }" style="width: 38px; height: 38px; min-width: 38px;">
          {{ projectStore.currentProject ? projectStore.currentProject.Name[0].toUpperCase() : 'P' }}
        </div>
        <div class="text-start min-w-0">
          <h1 class="mb-0 fs-6 fw-bold text-body lh-1 text-truncate" style="max-width: 140px;" id="sidebarMenuLabel">
            {{ projectStore.currentProject?.Name || $t('common.SCR0035') }}
          </h1>
          <p class="small text-muted mb-0 mt-1" style="font-size: 10px;">{{ $t('sidebar.SCR0008') }}</p>
        </div>
      </router-link>
      <!-- Close button visible only on mobile/tablet -->
      <button type="button" class="btn-close d-lg-none" data-bs-dismiss="offcanvas" data-bs-target="#sidebarMenu" :aria-label="$t('sidebar.SCR0012')"></button>
    </div>

    <div class="px-3 py-3" v-if="projectStore.currentProjectId && projectStore.userRole !== 'Member'">
      <button class="btn btn-primary w-100 py-2 fw-semibold d-flex align-items-center justify-content-center gap-2 shadow-sm" style="border-radius: 10px; background: linear-gradient(135deg, #059669, #10b981); border: none;" @click="$emit('create-task'); closeMobileMenu()">
        <i class="bi bi-plus-lg"></i> {{ $t('sidebar.SCR0007') }}
      </button>
    </div>

    <!-- Sidebar Navigation Menu -->
    <div class="flex-grow-1 px-2 overflow-auto py-2">
      <ul class="nav flex-column gap-1 mb-4">
        <!-- Dashboard link -->
        <li class="nav-item">
          <router-link :to="`/projects/${projectStore.currentProjectId}/dashboard`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm" @click="closeMobileMenu">
            <i class="bi bi-grid-1x2-fill fs-6"></i>
            <span class="small fw-medium">{{ $t('sidebar.SCR0001') }}</span>
          </router-link>
        </li>
        <!-- Task Board link -->
        <li class="nav-item">
          <router-link :to="`/projects/${projectStore.currentProjectId}/tasks`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm" @click="closeMobileMenu">
            <i class="bi bi-kanban fs-6"></i>
            <span class="small fw-medium">{{ $t('sidebar.SCR0002') }}</span>
          </router-link>
        </li>
        <!-- Calendar link -->
        <li class="nav-item">
          <router-link :to="`/projects/${projectStore.currentProjectId}/calendar`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm" @click="closeMobileMenu">
            <i class="bi bi-calendar3 fs-6"></i>
            <span class="small fw-medium">{{ $t('sidebar.SCR0009') }}</span>
          </router-link>
        </li>
        <!-- Gantt link -->
        <li class="nav-item">
          <router-link :to="`/projects/${projectStore.currentProjectId}/gantt`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm" @click="closeMobileMenu">
            <i class="bi bi-bar-chart-steps fs-6"></i>
            <span class="small fw-medium">{{ $t('sidebar.SCR0010') }}</span>
          </router-link>
        </li>
        <!-- Files / Documents link -->
        <li class="nav-item">
          <router-link :to="`/projects/${projectStore.currentProjectId}/files`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm" @click="closeMobileMenu">
            <i class="bi bi-folder2-open fs-6"></i>
            <span class="small fw-medium">{{ $t('sidebar.SCR0003') }}</span>
          </router-link>
        </li>
        <!-- Members link -->
        <li class="nav-item">
          <router-link :to="`/projects/${projectStore.currentProjectId}/members`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm" @click="closeMobileMenu">
            <i class="bi bi-people-fill fs-6"></i>
            <span class="small fw-medium">{{ $t('sidebar.SCR0004') }}</span>
          </router-link>
        </li>
        <!-- Settings link -->
        <li class="nav-item">
          <router-link :to="`/projects/${projectStore.currentProjectId}/settings`" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm" @click="closeMobileMenu">
            <i class="bi bi-gear-fill fs-6"></i>
            <span class="small fw-medium">{{ $t('sidebar.SCR0005') }}</span>
          </router-link>
        </li>
      </ul>
    </div>

    <!-- Sidebar Footer -->
    <div class="px-2 py-3 border-top">
      <ul class="nav flex-column gap-1">
        <li class="nav-item" v-if="projectStore.appRole === 'Admin'">
          <router-link
            to="/admin/dashboard"
            @click="closeMobileMenu"
            class="btn btn-link nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3 w-100 text-start text-decoration-none border-0 bg-transparent text-primary">
            <i class="bi bi-shield-lock-fill fs-6"></i>
            <span class="small fw-bold">{{ $t('sidebar.SCR0011') }}</span>
          </router-link>
        </li>
        <li class="nav-item">
          <button
            @click="handleLogout(); closeMobileMenu()"
            class="btn btn-link nav-link sidebar-link-danger d-flex align-items-center gap-2 px-3 py-2 rounded-3 w-100 text-start text-decoration-none border-0 bg-transparent">
            <i class="bi bi-box-arrow-right fs-6"></i>
            <span class="small fw-medium">{{ $t('sidebar.SCR0006') }}</span>
          </button>
        </li>
      </ul>
    </div>
  </nav>
</template>

<script setup>
import { useProjectStore } from '../stores/projectStore.js'
import { logout } from '../services/authService.js'
import { useRouter } from 'vue-router'
import { useSignalR } from '../composables/useSignalR.js'

const projectStore = useProjectStore()
const router = useRouter()
const { stopSignalR } = useSignalR()

defineEmits(['create-task'])

const handleLogout = async () => {
  try {
    await logout()
  } catch (error) {
    console.error(error)
  } finally {
    localStorage.removeItem('token')
    stopSignalR()
    projectStore.clearStore()
    router.push('/login')
  }
}

const closeMobileMenu = () => {
  if (window.innerWidth < 992) {
    const closeBtn = document.querySelector('#sidebarMenu .btn-close')
    if (closeBtn) closeBtn.click()
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
</script>

<style scoped>
.sidebar-link {
  transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
  color: var(--bs-secondary-color) !important;
  border-left: 3px solid transparent;
  border-radius: 0 8px 8px 0 !important;
}
.sidebar-link:hover {
  background: var(--bs-secondary-bg) !important;
  color: var(--bs-primary) !important;
  border-left-color: var(--bs-border-color);
  padding-left: 18px !important;
}
.active-project {
  background: rgba(13, 148, 136, 0.08) !important;
  color: var(--bs-primary) !important;
  border-left-color: var(--bs-primary) !important;
  font-weight: 600 !important;
}
.active-project i,
.active-project span {
  color: var(--bs-primary) !important;
}
.sidebar-link-danger {
  color: var(--bs-danger) !important;
  border-left: 3px solid transparent;
  border-radius: 0 8px 8px 0 !important;
  transition: all 0.2s ease;
}
.sidebar-link-danger:hover {
  background: rgba(239, 68, 68, 0.08) !important;
  color: var(--bs-danger) !important;
  border-left-color: var(--bs-danger) !important;
  padding-left: 18px !important;
}
.logo-box {
  box-shadow: var(--shadow-sm);
  transition: transform 0.2s ease;
}
.logo-box:hover {
  transform: scale(1.05);
}
</style>
