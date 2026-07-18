<template>
  <div class="p-3 p-md-4 text-start">
    
    <!-- Page Header -->
    <div class="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-3">
      <div>
        <h2 class="fw-bold mb-0 page-title text-body">Your Projects</h2>
        <p class="text-muted small mb-0 mt-1">Detailed list of projects you participate in, manage, and track progress.</p>
      </div>
      <button 
        class="btn btn-primary fw-semibold d-flex align-items-center gap-2 shadow-sm" 
        @click="handleCreateProject"
        style="border-radius: 8px; height: 38px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
      >
        <i class="bi bi-plus-lg"></i> New Project
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="loading && projectsWithProgress.length === 0" class="text-center py-5 my-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">Loading projects...</p>
    </div>

    <!-- Empty State -->
    <div v-else-if="projectsWithProgress.length === 0" class="text-center py-5 bg-body rounded-4 shadow-sm border border-dashed p-4">
      <i class="bi bi-folder2-open text-primary" style="font-size: 4rem;"></i>
      <h3 class="fw-bold text-body mt-3">No projects yet</h3>
      <p class="text-muted mx-auto mb-4" style="max-width: 480px;">Create your first project to set up a Kanban board and manage your tasks.</p>
      <button 
        class="btn btn-primary fw-semibold px-4 py-2" 
        @click="handleCreateProject"
        style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
      >
        New Project
      </button>
    </div>

    <!-- Projects Grid -->
    <div v-else class="row g-3">
      <div 
        v-for="proj in projectsWithProgress" 
        :key="proj.Id" 
        class="col-12 col-md-6 col-xxl-4"
      >
        <div class="card border-0 shadow-sm rounded-3 p-4 h-100 d-flex flex-column justify-content-between position-relative">
          
          <!-- Card Header details -->
          <div>
            <div class="d-flex align-items-start justify-content-between gap-2 mb-2">
              <h3 class="fw-bold text-body h5 mb-0 text-truncate" style="max-width: 220px;" :title="proj.Name">
                {{ proj.Name }}
              </h3>
              <span class="badge text-uppercase font-monospace" :class="getRoleBadgeClass(proj.UserRole)" style="font-size: 9px; padding: 4px 8px;">
                {{ proj.UserRole }}
              </span>
            </div>
            
            <p class="text-muted small text-start mb-3 text-wrap" style="font-size: 0.85rem; min-height: 40px; display: -webkit-box; -webkit-line-clamp: 2; line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;">
              {{ proj.Description || 'No description for this project.' }}
            </p>

            <div class="row g-2 mb-4 border-top border-bottom py-2.5 bg-body-tertiary" style="font-size: 0.8rem;">
              <div class="col-6">
                <span class="text-secondary d-block">Created by:</span>
                <span class="text-body fw-medium text-truncate d-block" :title="proj.OwnerEmail">{{ proj.OwnerEmail }}</span>
              </div>
              <div class="col-6">
                <span class="text-secondary d-block">Created on:</span>
                <span class="text-body fw-medium">{{ formatDateShort(proj.CreatedAt) }}</span>
              </div>
            </div>
          </div>

          <!-- Card Progress & Actions -->
          <div>
            <div class="d-flex align-items-center justify-content-between mb-2">
              <span class="text-secondary small">{{ proj.completedTasks }}/{{ proj.totalTasks }} Tasks</span>
              <span class="fw-bold text-body small">{{ proj.percent }}%</span>
            </div>
            <div class="progress mb-4" style="height: 6px;">
              <div 
                class="progress-bar bg-primary" 
                role="progressbar" 
                :style="{ width: proj.percent + '%' }" 
                :aria-valuenow="proj.percent" 
                aria-valuemin="0" 
                aria-valuemax="100"
              ></div>
            </div>

            <div class="d-flex align-items-center justify-content-between mt-auto gap-2">
              <button 
                class="btn btn-sm btn-primary fw-semibold px-3 py-2 flex-grow-1" 
                @click="goToProjectBoard(proj.Id)"
                style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
              >
                <i class="bi bi-box-arrow-in-right me-1"></i> Go to Board
              </button>
              
              <!-- Owner Actions -->
              <div v-if="proj.UserRole === 'Owner'" class="d-flex gap-1">
                <button 
                  class="btn btn-sm btn-outline-secondary p-2" 
                  @click="handleEditProject(proj)" 
                  title="Edit project" 
                  style="border-radius: 8px; width: 38px; height: 38px;"
                >
                  <i class="bi bi-pencil-square"></i>
                </button>
                <button 
                  class="btn btn-sm btn-outline-danger p-2" 
                  @click="handleDeleteProject(proj)" 
                  title="Delete project" 
                  style="border-radius: 8px; width: 38px; height: 38px;"
                >
                  <i class="bi bi-trash"></i>
                </button>
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
import { getProjects, getProjectTasks, createProject, updateProject, deleteProject } from '../services/projectService.js'
import { projectStore } from '../utils/projectStore.js'
import { toastSuccess, toastError, confirm, extractMessage } from '../utils/swal.js'
import Swal from 'sweetalert2'

const router = useRouter()
const loading = ref(false)
const projectsWithProgress = ref([])

const loadProjectsWithProgress = async () => {
  loading.value = true
  try {
    const res = await getProjects()
    const list = res?.data || []
    projectStore.setProjects(list)

    const promises = list.map(async (proj) => {
      try {
        const tasksRes = await getProjectTasks(proj.Id)
        const tasks = tasksRes?.data || []
        const completed = tasks.filter(t => t.Status === 'Done' || t.Status === 'Closed').length
        const total = tasks.length
        const percent = total > 0 ? Math.round((completed / total) * 100) : 0
        return {
          ...proj,
          percent,
          totalTasks: total,
          completedTasks: completed
        }
      } catch (e) {
        console.error('Error loading tasks for project progress ' + proj.Id, e)
        return {
          ...proj,
          percent: 0,
          totalTasks: 0,
          completedTasks: 0
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

const goToProjectBoard = (projectId) => {
  projectStore.setCurrentProjectId(projectId)
  router.push('/tasks')
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
      window.dispatchEvent(new Event('projects-changed'))
      if (res?.data?.Id) {
        projectStore.setCurrentProjectId(res.data.Id)
      }
    } catch (err) {
      toastError(extractMessage(err, 'Failed to create project.'))
    }
  }
}

const handleEditProject = async (proj) => {
  const { value: formValues } = await Swal.fire({
    title: 'Edit Project',
    html:
      '<div class="text-start mb-2"><label class="small fw-semibold text-muted">Project Name</label></div>' +
      `<input id="swal-proj-name" class="form-control mb-3" placeholder="Enter project name" value="${proj.Name || ''}" style="border-radius:10px; height:42px;">` +
      '<div class="text-start mb-2"><label class="small fw-semibold text-muted">Description (optional)</label></div>' +
      `<textarea id="swal-proj-desc" class="form-control" placeholder="Enter description" rows="3" style="border-radius:10px;">${proj.Description || ''}</textarea>`,
    focusConfirm: false,
    showCancelButton: true,
    confirmButtonText: 'Save Changes',
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
      await updateProject(proj.Id, formValues)
      toastSuccess('Project updated successfully!')
      await loadProjectsWithProgress()
      window.dispatchEvent(new Event('projects-changed'))
    } catch (err) {
      toastError(extractMessage(err, 'Failed to update project.'))
    }
  }
}

const handleDeleteProject = async (proj) => {
  const ok = await confirm(
    'Delete project?',
    `Are you sure you want to delete project <strong>${proj.Name}</strong>? This will remove all tasks and members from this project and cannot be undone.`,
    'Delete Project'
  )
  if (!ok) return

  try {
    await deleteProject(proj.Id)
    toastSuccess('Project deleted successfully!')
    if (projectStore.currentProjectId === proj.Id) {
      projectStore.setCurrentProjectId(null)
    }
    await loadProjectsWithProgress()
    window.dispatchEvent(new Event('projects-changed'))
  } catch (err) {
    console.error(err)
    toastError(extractMessage(err, 'Failed to delete project.'))
  }
}

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

const formatDateShort = (d) => d ? new Date(d).toLocaleDateString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'

onMounted(() => {
  loadProjectsWithProgress()
})
</script>

<style scoped>
.page-title {
  font-size: 1.68rem;
  letter-spacing: -0.02em;
}
.border-dashed {
  border-style: dashed !important;
}
</style>
