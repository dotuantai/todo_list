<template>
  <div class="min-vh-100 bg-body-tertiary">
    <!-- Top Landing Header -->
    <header class="bg-body border-bottom px-4 py-3 d-flex align-items-center justify-content-between">
      <div class="d-flex align-items-center gap-3">
        <div class="logo-box text-white d-flex align-items-center justify-content-center fw-bold fs-5 rounded-3" style="width: 38px; height: 38px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
          TT
        </div>
        <div class="text-start">
          <h1 class="mb-0 fs-5 fw-bold text-body lh-1">TutaFlow</h1>
          <p class="small text-muted mb-0 mt-1" style="font-size: 11px;">{{ $t('sidebar.SCR0008') }}</p>
        </div>
      </div>
      <div class="d-flex align-items-center gap-3">
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
              <svg v-else viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-btn-proj"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-btn-proj)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
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
                <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-item-proj"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-item-proj)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
                <span style="font-size: 0.85rem;">{{ $t('common.SCR0020') }}</span>
              </button>
            </li>
          </ul>
        </div>

        <!-- Theme Toggle -->
        <button class="btn btn-light border-0 p-2 d-flex align-items-center justify-content-center" style="border-radius: 8px; width: 36px; height: 36px;" @click="toggleTheme" :title="$t('common.SCR0021')">
          <i class="bi" :class="isDarkMode ? 'bi-sun-fill' : 'bi-moon-fill'"></i>
        </button>
        <!-- User Profile Dropdown -->
        <div class="dropdown">
          <button 
            class="btn p-0 border-0 bg-transparent d-flex align-items-center gap-2 text-decoration-none" 
            type="button" 
            data-bs-toggle="dropdown" 
            aria-expanded="false"
            style="outline: none; box-shadow: none;"
          >
            <div class="user-avatar-small bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width: 36px; height: 36px; font-size: 14px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
              {{ projectStore.currentInitial }}
            </div>
            <div class="d-none d-md-block text-start" style="line-height: 1.2;">
              <div class="fw-semibold small text-body text-truncate" style="max-width: 150px;">{{ projectStore.currentUserEmail }}</div>
              <div class="text-muted" style="font-size: 10px; margin-top: 1px;">{{ $t('common.SCR0030') }}</div>
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

    <!-- Main Container -->
    <div class="container py-5">
      <!-- Section Header -->
      <div class="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-3">
        <div class="text-start">
          <h2 class="fw-bold mb-1 text-body">{{ $t('projects.SCR0301') }}</h2>
          <p class="text-muted small mb-0">{{ $t('projects.SCR0302') }}</p>
        </div>
        <button 
          v-if="projectStore.appRole !== 'Member'"
          class="btn btn-primary fw-semibold d-flex align-items-center gap-2 shadow-sm" 
          @click="handleCreateProject"
          style="border-radius: 8px; height: 40px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
        >
          <i class="bi bi-plus-lg"></i> {{ $t('projects.SCR0303') }}
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading && projectsWithProgress.length === 0" class="text-center py-5 my-5">
        <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
        <p class="text-muted mt-3">{{ $t('common.SCR0009') }}</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="projectsWithProgress.length === 0" class="text-center py-5 bg-body rounded-4 shadow-sm border border-dashed p-5">
        <i class="bi bi-folder2-open text-primary" style="font-size: 4.5rem;"></i>
        <h3 class="fw-bold text-body mt-3">{{ $t('projects.SCR0305') }}</h3>
        <p class="text-muted mx-auto mb-4" style="max-width: 480px;">{{ $t('projects.SCR0306') }}</p>
        <button 
          v-if="projectStore.appRole !== 'Member'"
          class="btn btn-primary fw-semibold px-4 py-2.5" 
          @click="handleCreateProject"
          style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
        >
          {{ $t('projects.SCR0303') }}
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
                  {{ (proj.Name && proj.Name[0]) ? proj.Name[0].toUpperCase() : 'P' }}
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
                {{ proj.Description || $t('projects.SCR0318') }}
              </p>

              <!-- Meta Row: Members, Updated time -->
              <div class="d-flex align-items-center justify-content-between border-top pt-3 pb-3 mb-3" style="font-size: 0.8rem;">
                <div class="d-flex align-items-center gap-1.5 text-secondary">
                  <i class="bi bi-people-fill"></i>
                  <span>{{ $t('projects.SCR0307', { count: proj.memberCount || 1 }) }}</span>
                </div>
                <div class="text-secondary small">
                  {{ $t('projects.SCR0310', { date: formatDateShort(proj.UpdatedAt || proj.CreatedAt) }) }}
                </div>
              </div>
            </div>

            <!-- Footer: Progress & Click indicator -->
            <div>
              <div class="d-flex align-items-center justify-content-between mb-2">
                <span class="text-secondary small">{{ $t('projects.SCR0308', { completed: proj.completedTasks, total: proj.totalTasks }) }}</span>
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
                  {{ $t('projects.SCR0309') }} <i class="bi bi-arrow-right ms-1"></i>
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
import { useI18n } from 'vue-i18n'
import { getProjects, createProject } from '../services/projectService.js'
import { logout } from '../services/authService.js'
import { useProjectStore } from '../stores/projectStore.js'
import { toastSuccess, toastError, extractMessage } from '../utils/swal.js'
import Swal from 'sweetalert2'

const router = useRouter()
const projectStore = useProjectStore()
const { t, locale } = useI18n()

const changeLocale = (lang) => {
  locale.value = lang
  localStorage.setItem('locale', lang)
}

const isDarkMode = ref(document.documentElement.getAttribute('data-bs-theme') === 'dark')

const toggleTheme = () => {
  isDarkMode.value = !isDarkMode.value
  const theme = isDarkMode.value ? 'dark' : 'light'
  document.documentElement.setAttribute('data-bs-theme', theme)
  localStorage.setItem('theme', theme)
}

const projectsWithProgress = ref([])
const loading = ref(false)

const loadProjectsWithProgress = async () => {
  loading.value = true
  try {
    const res = await getProjects(1, 100)
    const rawList = res.data?.Items || []

    projectsWithProgress.value = rawList.map(proj => {
      const completed = proj.CompletedTasks ?? proj.completedTasks ?? proj.CompletedTasksCount ?? proj.completedTasksCount ?? 0
      const total = proj.TotalTasks ?? proj.totalTasks ?? proj.TotalTasksCount ?? proj.totalTasksCount ?? 0
      const percent = total > 0 ? Math.round((completed / total) * 100) : 0
      
      return {
        ...proj,
        percent,
        totalTasks: total,
        completedTasks: completed,
        memberCount: proj.MemberCount ?? proj.memberCount ?? 0
      }
    })
  } catch (err) {
    console.error('Error loading projects list', err)
    toastError(t('common.SCR0015'))
  } finally {
    loading.value = false
  }
}

const goToProject = (projectId) => {
  router.push(`/projects/${projectId}/dashboard`)
}

const handleCreateProject = async () => {
  const { value: formValues } = await Swal.fire({
    title: t('projects.SCR0303'),
    html:
      `<div class="text-start mb-2"><label class="small fw-semibold text-muted">${t('projects.SCR0312')}</label></div>` +
      `<input id="swal-proj-name" class="form-control mb-3" placeholder="${t('projects.SCR0313')}" style="border-radius:10px; height:42px;">` +
      `<div class="text-start mb-2"><label class="small fw-semibold text-muted">${t('projects.SCR0314')}</label></div>` +
      `<textarea id="swal-proj-desc" class="form-control" placeholder="${t('projects.SCR0315')}" rows="3" style="border-radius:10px;"></textarea>`,
    focusConfirm: false,
    showCancelButton: true,
    confirmButtonText: t('projects.SCR0316'),
    cancelButtonText: t('projects.SCR0317'),
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
      toastSuccess(t('projects.SCR0319'))
      await loadProjectsWithProgress()
      if (res?.data?.Id) {
        goToProject(res.data.Id)
      }
    } catch (err) {
      toastError(extractMessage(err, t('common.SCR0015')))
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
    'linear-gradient(135deg, #8b5cf6, #7c3aed)',
    'linear-gradient(135deg, #06b6d4, #0891b2)'
  ]
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  return colors[Math.abs(hash) % colors.length]
}

const formatDateShort = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
}

const handleLogout = () => {
  logout()
  projectStore.clearStore()
  router.push('/login')
}

onMounted(async () => {
  projectStore.decodeToken()
  await loadProjectsWithProgress()
})
</script>

<style scoped>
.project-card {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  border: 1px solid var(--bs-border-color-translucent) !important;
}

.project-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 24px -10px rgba(0, 0, 0, 0.15) !important;
  border-color: var(--bs-primary-border-subtle) !important;
}

.description-text {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  height: 38px;
}

.enter-workspace-text {
  opacity: 0;
  transform: translateX(-5px);
  transition: all 0.2s ease;
}

.project-card:hover .enter-workspace-text {
  opacity: 1;
  transform: translateX(0);
}
</style>
