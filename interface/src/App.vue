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

      <div class="px-3 py-3" v-if="projectStore.currentProjectId && projectStore.userRole !== 'Member'">
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
          <div class="position-relative notification-wrapper">
            <button 
              @click="toggleNotificationDropdown" 
              class="btn btn-light p-0 border rounded-3 d-flex align-items-center justify-content-center" 
              style="width: 36px; height: 36px; color: #64748b; position: relative;"
              title="Notifications"
            >
              <i class="bi" :class="unreadCount > 0 ? 'bi-bell-fill text-primary' : 'bi-bell'"></i>
              <span v-if="unreadCount > 0" class="position-absolute bg-danger border border-white rounded-circle" style="width: 8px; height: 8px; top: 7px; right: 7px;"></span>
            </button>

            <!-- Dropdown panel -->
            <div 
              v-if="showNotifications" 
              class="card shadow-lg border position-absolute mt-2 p-0 notification-dropdown" 
              style="width: 320px; right: 0; z-index: 1050; border-radius: 12px; overflow: hidden;"
            >
              <div class="card-header bg-white border-bottom py-2.5 px-3 d-flex align-items-center justify-content-between">
                <span class="fw-bold text-body small text-uppercase tracking-wider mb-0" style="font-size: 11px;">Notifications</span>
                <button 
                  v-if="unreadCount > 0" 
                  @click="handleMarkAllAsRead" 
                  class="btn btn-link p-0 text-decoration-none small text-primary fw-semibold" 
                  style="font-size: 11px;"
                >
                  Mark all as read
                </button>
              </div>
              <div class="list-group list-group-flush overflow-auto" style="max-height: 350px;">
                <div v-if="notifications.length === 0" class="text-center py-4 text-muted small fst-italic">
                  <i class="bi bi-bell-slash d-block fs-4 opacity-50 mb-1"></i>
                  No notifications
                </div>
                <button 
                  v-else
                  v-for="n in notifications" 
                  :key="n.Id" 
                  @click="handleNotificationClick(n)"
                  class="list-group-item list-group-item-action text-start p-3 border-bottom d-flex align-items-start gap-2"
                  :style="!n.IsRead ? { backgroundColor: 'rgba(99, 102, 241, 0.03)' } : {}"
                >
                  <div class="flex-grow-1 min-w-0">
                    <div class="d-flex align-items-center justify-content-between gap-2 mb-1">
                      <span class="fw-bold text-body text-truncate" style="font-size: 12.5px;">{{ n.Title }}</span>
                      <span v-if="!n.IsRead" class="badge rounded-circle p-1 bg-primary" style="width: 6px; height: 6px;" title="Unread"></span>
                    </div>
                    <p class="text-secondary mb-1 small lh-sm" style="font-size: 12px;">{{ n.Message }}</p>
                    <span class="text-muted" style="font-size: 9.5px; font-family: monospace;">{{ formatTime(n.CreatedAt) }}</span>
                  </div>
                </button>
              </div>
            </div>
          </div>
          
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
import { HubConnectionBuilder } from '@microsoft/signalr'
import { getNotifications, markAsRead, markAllAsRead } from './services/notificationService.js'

const router = useRouter()
const route = router.currentRoute
const createTaskModal = ref(null)
const projectStore = useProjectStore()
const sidebarMembers = ref([])

// Notification states
const notifications = ref([])
const showNotifications = ref(false)
const unreadCount = computed(() => notifications.value.filter(n => !n.IsRead).length)
let signalRConnection = null

const initSignalR = async () => {
  if (signalRConnection) return
  const token = localStorage.getItem('token')
  if (!token) return

  const baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7087/api'
  const hubUrl = baseUrl.replace('/api', '/hubs/notifications')

  signalRConnection = new HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => token
    })
    .withAutomaticReconnect()
    .build()

  signalRConnection.on('ReceiveNotification', (n) => {
    notifications.value.unshift(n)
  })

  signalRConnection.on('TaskCreated', (task) => {
    window.dispatchEvent(new CustomEvent('task-created', { detail: task }))
  })

  signalRConnection.on('TaskUpdated', (task) => {
    window.dispatchEvent(new CustomEvent('task-updated', { detail: task }))
  })

  signalRConnection.on('TaskDeleted', (taskId) => {
    window.dispatchEvent(new CustomEvent('task-deleted', { detail: taskId }))
  })

  signalRConnection.onreconnected(async (connectionId) => {
    console.log(`SignalR reconnected: ${connectionId}`)
    if (projectStore.currentUserId) {
      await signalRConnection.invoke('RegisterUser', projectStore.currentUserId)
    }
    if (projectStore.currentProjectId) {
      await signalRConnection.invoke('JoinProject', projectStore.currentProjectId)
    }
  })

  try {
    await signalRConnection.start()
    console.log('SignalR connected.')
    if (projectStore.currentUserId) {
      await signalRConnection.invoke('RegisterUser', projectStore.currentUserId)
    }
    if (projectStore.currentProjectId) {
      await signalRConnection.invoke('JoinProject', projectStore.currentProjectId)
    }
  } catch (err) {
    console.error('SignalR connection failed', err)
  }
}

const stopSignalR = async () => {
  if (signalRConnection) {
    try {
      await signalRConnection.stop()
    } catch (e) {}
    signalRConnection = null
  }
}

const loadNotifications = async () => {
  if (!projectStore.isAuthenticated) return
  try {
    const res = await getNotifications()
    notifications.value = res?.data || []
  } catch (e) {
    console.error('Failed to load notifications:', e)
  }
}

const toggleNotificationDropdown = async () => {
  showNotifications.value = !showNotifications.value
  if (showNotifications.value) {
    await loadNotifications()
  }
}

const handleNotificationClick = async (n) => {
  if (n.IsRead) return
  try {
    await markAsRead(n.Id)
    n.IsRead = true
  } catch (e) {
    console.error('Failed to mark notification as read:', e)
  }
}

const handleMarkAllAsRead = async () => {
  try {
    await markAllAsRead()
    notifications.value.forEach(n => n.IsRead = true)
  } catch (e) {
    console.error('Failed to mark all as read:', e)
  }
}

const closeNotificationDropdownOnOutside = (e) => {
  if (!e.target.closest('.notification-wrapper')) {
    showNotifications.value = false
  }
}

const formatTime = (iso) => {
  if (!iso) return ''
  const dateObj = new Date(iso)
  return dateObj.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) + ' ' + dateObj.toLocaleDateString('en-US', { day: '2-digit', month: '2-digit' })
}

watch(() => projectStore.isAuthenticated, (newVal) => {
  if (newVal) {
    initSignalR()
    loadNotifications()
  } else {
    stopSignalR()
    notifications.value = []
  }
})

watch(() => projectStore.currentProjectId, async (newVal, oldVal) => {
  if (signalRConnection && signalRConnection.state === 'Connected') {
    if (oldVal) {
      try {
        await signalRConnection.invoke('LeaveProject', oldVal)
        console.log(`Left SignalR project group: ${oldVal}`)
      } catch (err) {
        console.error('Failed to leave SignalR project group', err)
      }
    }
    if (newVal) {
      try {
        await signalRConnection.invoke('JoinProject', newVal)
        console.log(`Joined SignalR project group: ${newVal}`)
      } catch (err) {
        console.error('Failed to join SignalR project group', err)
      }
    }
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
  window.addEventListener('click', closeNotificationDropdownOnOutside)
  if (route.value.params.projectId) {
    loadSidebarMembers()
  }
  if (projectStore.isAuthenticated) {
    initSignalR()
    loadNotifications()
  }
})

onUnmounted(() => {
  window.removeEventListener('project-members-changed', loadSidebarMembers)
  window.removeEventListener('click', closeNotificationDropdownOnOutside)
  stopSignalR()
})

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
.notification-dropdown {
  background: var(--bs-card-bg);
  border-color: var(--bs-border-color) !important;
}
.notification-dropdown .list-group-item:hover {
  background-color: var(--bs-secondary-bg) !important;
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