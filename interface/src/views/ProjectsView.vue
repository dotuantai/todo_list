<template>
  <div class="min-vh-100 bg-body-tertiary">
    <!-- Top Landing Header -->
    <header class="bg-body border-bottom px-4 py-3 d-flex align-items-center justify-content-between">
      <div class="d-flex align-items-center gap-3">
        <div class="logo-box text-white d-flex align-items-center justify-content-center fw-bold fs-5 rounded-3" style="width: 38px; height: 38px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
          TF
        </div>
        <div class="text-start">
          <h1 class="mb-0 fs-5 fw-bold text-body lh-1">TaskFlow Pro</h1>
          <p class="small text-muted mb-0 mt-1" style="font-size: 11px;">Workspaces</p>
        </div>
      </div>
      <div class="d-flex align-items-center gap-3">
        <!-- Theme Toggle -->
        <button class="btn btn-light border-0 p-2 d-flex align-items-center justify-content-center" style="border-radius: 8px; width: 36px; height: 36px;" @click="toggleTheme" title="Toggle Theme">
          <i class="bi" :class="isDarkMode ? 'bi-sun-fill' : 'bi-moon-fill'"></i>
        </button>
        <!-- User Profile -->
        <div class="d-flex align-items-center gap-2">
          <div class="user-avatar-small bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width: 36px; height: 36px; font-size: 14px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
            {{ projectStore.currentInitial }}
          </div>
          <div class="d-none d-md-block text-start" style="line-height: 1.2;">
            <div class="fw-semibold small text-body text-truncate" style="max-width: 150px;">{{ projectStore.currentUserEmail }}</div>
            <div class="text-muted" style="font-size: 10px; margin-top: 1px;">SaaS Workspace</div>
          </div>
        </div>
        <div class="vr opacity-25" style="height: 24px;"></div>
        <!-- Sign Out -->
        <button class="btn btn-outline-danger btn-sm px-3 fw-semibold" style="border-radius: 8px;" @click="handleLogout">
          <i class="bi bi-box-arrow-right me-1"></i> Sign out
        </button>
      </div>
    </header>

    <!-- Main Container -->
    <div class="container py-5">
      <!-- Section Header -->
      <div class="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-3">
        <div class="text-start">
          <h2 class="fw-bold mb-1 text-body">Select a Project</h2>
          <p class="text-muted small mb-0">Choose a workspace to view your dashboard, tasks board, and settings.</p>
        </div>
        <button 
          class="btn btn-primary fw-semibold d-flex align-items-center gap-2 shadow-sm" 
          @click="handleCreateProject"
          style="border-radius: 8px; height: 40px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
        >
          <i class="bi bi-plus-lg"></i> Create Project
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading && projectsWithProgress.length === 0" class="text-center py-5 my-5">
        <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
        <p class="text-muted mt-3">Loading your workspaces...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="projectsWithProgress.length === 0" class="text-center py-5 bg-body rounded-4 shadow-sm border border-dashed p-5">
        <i class="bi bi-folder2-open text-primary" style="font-size: 4.5rem;"></i>
        <h3 class="fw-bold text-body mt-3">No workspaces found</h3>
        <p class="text-muted mx-auto mb-4" style="max-width: 480px;">Get started by creating your first collaborative project board to manage tasks with your team.</p>
        <button 
          class="btn btn-primary fw-semibold px-4 py-2.5" 
          @click="handleCreateProject"
          style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
        >
          Create Project
        </button>
      </div>

      <!-- Project Cards Grid -->
      <div v-else class="row g-4">
        <div 
          v-for="proj in projectsWithProgress" 
          :key="proj.Id" 
          class="col-12 col-md-6 col-lg-4"
        >
          <div 
            class="card project-card border-0 shadow-sm rounded-4 p-4 h-100 d-flex flex-column justify-content-between position-relative bg-body"
            @click="goToProject(proj.Id)"
            style="cursor: pointer;"
          >
            <!-- Card Content -->
            <div>
              <!-- Header: Logo, Name, Role -->
              <div class="d-flex align-items-center gap-3 mb-3">
                <div class="project-logo text-white d-flex align-items-center justify-content-center fw-bold fs-4 rounded-3 shadow-sm" :style="{ background: getProjectColor(proj.Name) }" style="width: 48px; height: 48px; min-width: 48px;">
                  {{ proj.Name[0].toUpperCase() }}
                </div>
                <div class="text-start min-w-0 flex-grow-1">
                  <h3 class="fw-bold text-body h5 mb-1 text-truncate" :title="proj.Name">
                    {{ proj.Name }}
                  </h3>
                  <span class="badge text-uppercase font-monospace" :class="getRoleBadgeClass(proj.UserRole)" style="font-size: 8px; padding: 2px 6px;">
                    {{ proj.UserRole }}
                  </span>
                </div>
              </div>

              <!-- Description -->
              <p class="text-muted small text-start mb-4 text-wrap description-text">
                {{ proj.Description || 'No description provided for this project.' }}
              </p>

              <!-- Meta Row: Members, Updated time -->
              <div class="d-flex align-items-center justify-content-between border-top pt-3 pb-3 mb-3" style="font-size: 0.8rem;">
                <div class="d-flex align-items-center gap-1.5 text-secondary">
                  <i class="bi bi-people-fill"></i>
                  <span>{{ proj.memberCount || 1 }} members</span>
                </div>
                <div class="text-secondary small">
                  Updated: {{ formatDateShort(proj.UpdatedAt || proj.CreatedAt) }}
                </div>
              </div>
            </div>

            <!-- Footer: Progress & Click indicator -->
            <div>
              <div class="d-flex align-items-center justify-content-between mb-2">
                <span class="text-secondary small">{{ proj.completedTasks }}/{{ proj.totalTasks }} tasks completed</span>
                <span class="fw-bold text-body small">{{ proj.percent }}%</span>
              </div>
              <div class="progress mb-3" style="height: 6px; border-radius: 3px;">
                <div 
                  class="progress-bar bg-primary" 
                  role="progressbar" 
                  :style="{ width: proj.percent + '%' }" 
                  :aria-valuenow="proj.percent" 
                  aria-valuemin="0" 
                  aria-valuemax="100"
                ></div>
              </div>

              <div class="d-flex align-items-center justify-content-end gap-2 mt-2 pt-2">
                <span class="text-primary small fw-semibold enter-workspace-text">
                  Enter workspace <i class="bi bi-arrow-right ms-1"></i>
                </span>
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getProjects, getProjectTasks, getMembers, createProject } from '../services/projectService.js'
import { logout } from '../services/authService.js'
import { useProjectStore } from '../stores/projectStore.js'
import { toastSuccess, toastError, extractMessage } from '../utils/swal.js'
import Swal from 'sweetalert2'

const router = useRouter()
const projectStore = useProjectStore()
const loading = ref(false)
const projectsWithProgress = ref([])
const isDarkMode = ref(false)

const loadProjectsWithProgress = async () => {
  loading.value = true
  try {
    const res = await getProjects()
    const list = res?.data || []
    projectStore.setProjects(list)

    const promises = list.map(async (proj) => {
      try {
        const [tasksRes, membersRes] = await Promise.all([
          getProjectTasks(proj.Id),
          getMembers(proj.Id)
        ])
        
        const tasks = tasksRes?.data || []
        const members = membersRes?.data || []
        const completed = tasks.filter(t => t.Status === 'Done' || t.Status === 'Closed').length
        const total = tasks.length
        const percent = total > 0 ? Math.round((completed / total) * 100) : 0
        
        return {
          ...proj,
          percent,
          totalTasks: total,
          completedTasks: completed,
          memberCount: members.length
        }
      } catch (e) {
        console.error('Error loading meta for project ' + proj.Id, e)
        return {
          ...proj,
          percent: 0,
          totalTasks: 0,
          completedTasks: 0,
          memberCount: 1
        }
      }
    })
    projectsWithProgress.value = await Promise.all(promises)
  } catch (err) {
    console.error('Error loading projects list', err)
    toastError('Failed to load project list.')
  } finally {
    loading.value = false
  }
}

const goToProject = (projectId) => {
  router.push(`/projects/${projectId}/dashboard`)
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
      toastSuccess('Project created successfully!')
      await loadProjectsWithProgress()
      if (res?.data?.Id) {
        goToProject(res.data.Id)
      }
    } catch (err) {
      toastError(extractMessage(err, 'Failed to create project.'))
    }
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

const toggleTheme = () => {
  isDarkMode.value = !isDarkMode.value
  const theme = isDarkMode.value ? 'dark' : 'light'
  document.documentElement.setAttribute('data-bs-theme', theme)
  localStorage.setItem('theme', theme)
  toastSuccess(`Switched to ${isDarkMode.value ? 'Dark' : 'Light'} mode!`)
}

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

const formatDateShort = (d) => d ? new Date(d).toLocaleDateString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'

onMounted(() => {
  const currentTheme = document.documentElement.getAttribute('data-bs-theme') || localStorage.getItem('theme') || 'light'
  isDarkMode.value = (currentTheme === 'dark')
  loadProjectsWithProgress()
})
</script>

<style scoped>
.project-card {
  transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1), box-shadow 0.25s ease, border-color 0.25s ease;
  border: 1px solid var(--bs-border-color) !important;
}

.project-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 28px rgba(0, 0, 0, 0.08) !important;
  border-color: #6366f1 !important;
}

.description-text {
  font-size: 0.875rem;
  line-height: 1.5;
  min-height: 48px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  color: var(--bs-secondary-color);
}

.enter-workspace-text {
  opacity: 0;
  transform: translateX(-4px);
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.project-card:hover .enter-workspace-text {
  opacity: 1;
  transform: translateX(0);
}

.border-dashed {
  border-style: dashed !important;
}
</style>
