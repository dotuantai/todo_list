<template>
  <div class="p-3 p-md-4 text-start">
    <div class="row g-4">
      <!-- Left Column: Settings Cards -->
      <div class="col-12 col-lg-8">
        
        <!-- Project Management Card (Owner Only) -->
        <div v-if="isOwner && projectStore.currentProject" class="card border-0 shadow-sm rounded-3 p-4 mb-4 bg-body">
          <div class="d-flex align-items-center gap-3 mb-3">
            <div class="bg-primary bg-opacity-10 text-primary rounded-3 p-2 d-flex align-items-center justify-content-center" style="width: 42px; height: 42px;">
              <i class="bi bi-sliders fs-5"></i>
            </div>
            <div>
              <h4 class="fw-bold text-body h5 mb-0">Project Details</h4>
              <p class="text-muted small mb-0">Modify workspace names, description details, or delete this project.</p>
            </div>
          </div>
          
          <hr />

          <form @submit.prevent="handleUpdateProject">
            <div class="mb-3">
              <label class="form-label small fw-semibold text-muted">Project Name</label>
              <input v-model="editForm.name" type="text" class="form-control" required style="border-radius: 8px; height: 40px;" />
            </div>
            <div class="mb-3">
              <label class="form-label small fw-semibold text-muted">Description (optional)</label>
              <textarea v-model="editForm.description" class="form-control" rows="3" style="border-radius: 8px;"></textarea>
            </div>
            <div class="d-flex align-items-center justify-content-between pt-2">
              <button type="submit" class="btn btn-primary fw-semibold" style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;">
                Save Changes
              </button>
              
              <button type="button" class="btn btn-outline-danger fw-semibold" @click="handleDeleteProject" style="border-radius: 8px;">
                <i class="bi bi-trash me-1"></i> Delete Project
              </button>
            </div>
          </form>
        </div>

        <!-- Theme Switcher Card -->
        <div class="card border-0 shadow-sm rounded-3 p-4 mb-4 bg-body">
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
        <div class="card border-0 shadow-sm rounded-3 p-4 mb-4 bg-body">
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
        <div class="card border-0 shadow-sm rounded-3 p-4 text-center bg-body">
          <div class="user-avatar-large mx-auto mb-3 bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width: 72px; height: 72px; font-size: 28px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
            {{ userInitial }}
          </div>
          <h4 class="fw-bold text-body mb-1 text-truncate" :title="userEmail">{{ userEmail }}</h4>
          <span class="badge text-uppercase font-monospace bg-light text-secondary border rounded-pill px-3 py-1.5" style="font-size: 10px;">
            {{ projectStore.userRole }} Role
          </span>

          <hr class="my-4" />

          <div class="text-start">
            <div class="mb-3">
              <span class="text-secondary small d-block">Account Name:</span>
              <span class="text-body fw-medium">{{ userEmail.split('@')[0] }}</span>
            </div>
            <div class="mb-3">
              <span class="text-secondary small d-block">Active Workspace:</span>
              <span class="text-body fw-medium">{{ projectStore.currentProject?.Name || 'None' }}</span>
            </div>
            <div>
              <span class="text-secondary small d-block">Connection Status:</span>
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
import { useRouter } from 'vue-router'
import { useProjectStore } from '../stores/projectStore.js'
import { updateProject, deleteProject } from '../services/projectService.js'
import { toastSuccess, toastError, confirm, extractMessage } from '../utils/swal.js'

const router = useRouter()
const projectStore = useProjectStore()
const isDarkMode = ref(false)

const userEmail = computed(() => projectStore.currentUserEmail || 'User@example.com')
const userInitial = computed(() => projectStore.currentInitial)

// Form for editing project details
const editForm = ref({
  name: '',
  description: ''
})

const isOwner = computed(() => projectStore.userRole === 'Owner')

const initProjectForm = () => {
  if (projectStore.currentProject) {
    editForm.value.name = projectStore.currentProject.Name || ''
    editForm.value.description = projectStore.currentProject.Description || ''
  }
}

const handleThemeToggle = () => {
  const theme = isDarkMode.value ? 'dark' : 'light'
  document.documentElement.setAttribute('data-bs-theme', theme)
  localStorage.setItem('theme', theme)
  window.dispatchEvent(new CustomEvent('theme-changed', { detail: theme }))
  toastSuccess(`Switched to ${isDarkMode.value ? 'Dark' : 'Light'} mode!`)
}

const handleUpdateProject = async () => {
  if (!projectStore.currentProjectId) return
  if (!editForm.value.name.trim()) return
  try {
    const payload = {
      name: editForm.value.name.trim(),
      description: editForm.value.description.trim()
    }
    await updateProject(projectStore.currentProjectId, payload)
    toastSuccess('Project updated successfully!')
    await projectStore.fetchProjects()
  } catch (err) {
    toastError(extractMessage(err, 'Failed to update project.'))
  }
}

const handleDeleteProject = async () => {
  if (!projectStore.currentProject) return
  const currentProj = projectStore.currentProject

  const ok = await confirm(
    'Delete project?',
    `Are you sure you want to delete project <strong>${currentProj.Name}</strong>? This will remove all tasks and members from this project and cannot be undone.`,
    'Delete Project'
  )
  if (!ok) return

  try {
    await deleteProject(currentProj.Id)
    toastSuccess('Project deleted successfully!')
    projectStore.setCurrentProjectId(null)
    await projectStore.fetchProjects()
    router.push('/projects')
  } catch (err) {
    console.error(err)
    toastError(extractMessage(err, 'Failed to delete project.'))
  }
}

onMounted(() => {
  const currentTheme = document.documentElement.getAttribute('data-bs-theme') || localStorage.getItem('theme') || 'light'
  isDarkMode.value = (currentTheme === 'dark')
  initProjectForm()
})
</script>

<style scoped>
.page-title {
  font-size: 1.68rem;
  letter-spacing: -0.02em;
}
</style>
