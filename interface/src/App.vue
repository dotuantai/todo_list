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
        <div class="d-flex align-items-center gap-3">
          <div class="logo-box bg-primary text-white d-flex align-items-center justify-content-center fw-bold fs-5 rounded-3" style="width: 38px; height: 38px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
            TF
          </div>
          <div class="text-start">
            <h1 class="mb-0 fs-6 fw-bold text-body lh-1" id="sidebarMenuLabel">TaskFlow Pro</h1>
            <p class="small text-muted mb-0 mt-1" style="font-size: 10px;">Enterprise Plan</p>
          </div>
        </div>
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
        <ul class="nav flex-column gap-1">
          <!-- Dashboard link -->
          <li class="nav-item">
            <router-link to="/" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm">
              <i class="bi bi-grid-1x2-fill fs-6"></i>
              <span class="small fw-medium">Dashboard</span>
            </router-link>
          </li>
          <!-- Task Board link -->
          <li class="nav-item">
            <router-link to="/tasks" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm">
              <i class="bi bi-kanban fs-6"></i>
              <span class="small fw-medium">Task Board</span>
            </router-link>
          </li>
          <!-- Projects List link -->
          <li class="nav-item">
            <router-link to="/projects" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm">
              <i class="bi bi-folder2-open fs-6"></i>
              <span class="small fw-medium">Danh sách dự án</span>
            </router-link>
          </li>
          <!-- Settings link -->
          <li class="nav-item">
            <router-link to="/settings" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3" active-class="active-project bg-primary text-white shadow-sm">
              <i class="bi bi-gear-fill fs-6"></i>
              <span class="small fw-medium">Cài đặt</span>
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
              <span class="small fw-medium">Đăng xuất</span>
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
            <div class="bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-2" style="width: 28px; height: 28px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important; font-size: 12px;">
              TF
            </div>
            <span class="fw-bold text-body mb-0 fs-6">TaskFlow</span>
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
              {{ currentInitial }}
            </div>
            
            <div class="d-none d-md-block text-start" style="line-height: 1.2;">
              <div class="fw-semibold small text-body text-truncate" style="max-width: 150px;" :title="currentUserName">{{ currentUserName }}</div>
              <div class="text-muted" style="font-size:10px; margin-top:2px" v-if="projectStore.currentProject">
                <span class="badge text-uppercase font-monospace" :class="getRoleBadgeClass(projectStore.userRole)" style="font-size: 8px; padding: 2px 4px;">{{ projectStore.userRole }}</span>
              </div>
              <span class="badge bg-secondary-subtle text-secondary" style="font-size:8px; padding: 2px 4px;" v-else>
                Hệ thống
              </span>
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
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import TaskModal from './components/TaskModal.vue'
import { logout } from './services/authService.js'
import { projectStore } from './utils/projectStore.js'
import { getProjects, createProject } from './services/projectService.js'
import { toastSuccess, toastError, extractMessage } from './utils/swal.js'
import Swal from 'sweetalert2'

const router = useRouter()
const route = router.currentRoute
const createTaskModal = ref(null)

const openCreateTaskModal = () => {
  createTaskModal.value?.openModal()
}

const authRoutes = ['/login', '/register']
const showAppShell = computed(() => !authRoutes.includes(route.value.path))

const currentUserName = computed(() => {
  const token = localStorage.getItem('token')
  if (!token) return 'Guest'
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''))
    const payload = JSON.parse(jsonPayload)
    // Save to localStorage for quick settings use
    if (payload.email) localStorage.setItem('userEmail', payload.email)
    return payload.email || payload.unique_name || payload.sub || 'User'
  } catch (e) {
    console.error('Error decoding token', e)
    return 'User'
  }
})

const currentInitial = computed(() => {
  return currentUserName.value ? currentUserName.value[0].toUpperCase() : '?'
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

const loadProjects = async () => {
  if (!showAppShell.value) return
  const token = localStorage.getItem('token')
  if (!token || token === 'null' || token === 'undefined' || token.split('.').length !== 3) {
    localStorage.removeItem('token')
    if (route.value.path !== '/login' && route.value.path !== '/register') {
      router.push('/login')
    }
    return
  }
  projectStore.loading = true
  try {
    const res = await getProjects()
    projectStore.setProjects(res?.data || [])
  } catch (err) {
    console.error(err)
    if (err.response?.status === 401 || err.response?.status === 400) {
      localStorage.removeItem('token')
      projectStore.setCurrentProjectId(null)
      projectStore.setProjects([])
      if (route.value.path !== '/login' && route.value.path !== '/register') {
        router.push('/login')
      }
    } else {
      toastError(extractMessage(err, 'Không thể tải danh sách dự án.'))
    }
  } finally {
    projectStore.loading = false
  }
}

const handleCreateProject = async () => {
  const { value: formValues } = await Swal.fire({
    title: 'Create New Project',
    html:
      '<div class="text-start mb-2"><label class="small fw-semibold text-muted">Project Name</label></div>' +
      '<input id="swal-proj-name" class="form-control mb-3" placeholder="Enter project name" style="border-radius:10px; height:42px;">' +
      '<div class="text-start mb-2"><label class="small fw-semibold text-muted">Description (optional)</label></div>' +
      '<textarea id="swal-proj-desc" class="form-control" placeholder="Enter description" rows="3" style="border-radius:10px;"></textarea>',
    focusConfirm: false,
    showCancelButton: true,
    confirmButtonText: 'Create Project',
    cancelButtonText: 'Cancel',
    customClass: {
      popup: 'swal-popup',
      confirmButton: 'swal-btn swal-btn--confirm',
      cancelButton: 'swal-btn swal-btn--cancel'
    },
    buttonsStyling: false,
    preConfirm: () => {
      const name = document.getElementById('swal-proj-name').value
      const description = document.getElementById('swal-proj-desc').value
      if (!name || !name.trim()) {
        Swal.showValidationMessage('Project name is required')
      }
      return { name, description }
    }
  })

  if (formValues) {
    try {
      const res = await createProject(formValues)
      toastSuccess('Tạo dự án thành công!')
      await loadProjects()
      if (res?.data?.Id) {
        projectStore.setCurrentProjectId(res.data.Id)
      }
    } catch (err) {
      toastError(extractMessage(err, 'Không thể tạo dự án.'))
    }
  }
}

const onProjectsChanged = () => {
  loadProjects()
}

onMounted(() => {
  // Sync Dark/Light theme configuration
  const currentTheme = localStorage.getItem('theme') || 'light'
  document.documentElement.setAttribute('data-bs-theme', currentTheme)

  loadProjects()
  window.addEventListener('projects-changed', onProjectsChanged)
})

onUnmounted(() => {
  window.removeEventListener('projects-changed', onProjectsChanged)
})

watch(showAppShell, (newVal) => {
  if (newVal) {
    loadProjects()
  }
})

const handleLogout = async () => {
  try {
    await logout()
  } catch (error) {
    console.error(error)
  } finally {
    localStorage.removeItem('token')
    localStorage.removeItem('userEmail')
    projectStore.setCurrentProjectId(null)
    projectStore.setProjects([])
    router.push('/login')      
  }
}
</script>

<style scoped>
.sidebar-link {
  transition: background 0.15s, color 0.15s;
  color: #64748b !important;
}
.sidebar-link:hover {
  background: #f1f5f9 !important;
  color: #0f172a !important;
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
  color: #ef4444 !important;
}
.sidebar-link-danger:hover {
  background: #fff1f2 !important;
  color: #dc2626 !important;
}
</style>