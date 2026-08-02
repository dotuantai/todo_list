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
              <h4 class="fw-bold text-body h5 mb-0">{{ $t('settings.title') }}</h4>
              <p class="text-muted small mb-0">{{ $t('settings.desc_subtitle') }}</p>
            </div>
          </div>
          
          <hr />

          <form @submit.prevent="handleUpdateProject">
            <div class="mb-3">
              <label class="form-label small fw-semibold text-muted">{{ $t('settings.proj_name') }}</label>
              <input v-model="editForm.name" type="text" class="form-control" required style="border-radius: 8px; height: 40px;" />
            </div>
            <div class="mb-3">
              <label class="form-label small fw-semibold text-muted">{{ $t('settings.proj_desc') }}</label>
              <textarea v-model="editForm.description" class="form-control" rows="3" style="border-radius: 8px;"></textarea>
            </div>
            <div class="d-flex align-items-center justify-content-between pt-2">
              <button type="submit" class="btn btn-primary fw-semibold" style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;">
                {{ $t('settings.save_changes') }}
              </button>
              
              <button type="button" class="btn btn-outline-danger fw-semibold" @click="handleDeleteProject" style="border-radius: 8px;">
                <i class="bi bi-trash me-1"></i> {{ $t('settings.delete_proj') }}
              </button>
            </div>
          </form>
        </div>

        <!-- Column Management Card (Owner/Manager Only) -->
        <div v-if="(isOwner || projectStore.userRole === 'Manager') && projectStore.currentProject" class="card border-0 shadow-sm rounded-3 p-4 mb-4 bg-body">
          <div class="d-flex align-items-center justify-content-between mb-3">
            <div class="d-flex align-items-center gap-3">
              <div class="bg-info bg-opacity-10 text-info rounded-3 p-2 d-flex align-items-center justify-content-center" style="width: 42px; height: 42px;">
                <i class="bi bi-layout-three-columns fs-5"></i>
              </div>
              <div>
                <h4 class="fw-bold text-body h5 mb-0">{{ $t('settings.columns_title') }}</h4>
                <p class="text-muted small mb-0">{{ $t('settings.columns_desc') }}</p>
              </div>
            </div>
            <button type="button" class="btn btn-sm btn-primary fw-semibold" style="border-radius: 8px;" @click="openColModal()">
              <i class="bi bi-plus-lg me-1"></i> {{ $t('settings.add_column') }}
            </button>
          </div>
          
          <hr />

          <div v-if="loadingColumns" class="text-center py-4">
            <span class="spinner-border spinner-border-sm text-primary" role="status"></span>
          </div>
          <div v-else class="list-group list-group-flush">
            <div v-for="col in columns" :key="col.Id" class="list-group-item d-flex justify-content-between align-items-center border-0 border-bottom px-0 py-3">
              <div>
                <h6 class="mb-1 fw-bold text-body">{{ col.Name }}</h6>
                <small class="text-muted">{{ $t('settings.order') }}: {{ col.Order }} <span v-if="col.IsCompletedStage" class="badge bg-success-subtle text-success ms-2">{{ $t('settings.completed_stage') }}</span></small>
              </div>
              <div class="btn-group gap-2">
                <button class="btn btn-sm btn-light border" style="border-radius: 6px;" @click="openColModal(col)">
                  <i class="bi bi-pencil"></i>
                 </button>
                <button class="btn btn-sm btn-light border text-danger" style="border-radius: 6px;" @click="handleDeleteColumn(col.Id)" :disabled="columns.length <= 1">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Theme Switcher Card -->
        <div class="card border-0 shadow-sm rounded-3 p-4 mb-4 bg-body">
          <div class="d-flex align-items-center gap-3 mb-3">
            <div class="bg-primary bg-opacity-10 text-primary rounded-3 p-2 d-flex align-items-center justify-content-center" style="width: 42px; height: 42px;">
              <i class="bi bi-palette-fill fs-5"></i>
            </div>
            <div>
              <h4 class="fw-bold text-body h5 mb-0">{{ $t('settings.app_theme') }}</h4>
              <p class="text-muted small mb-0">{{ $t('settings.theme_subtitle') }}</p>
            </div>
          </div>
          
          <hr />

          <div class="d-flex align-items-center justify-content-between py-2">
            <div>
              <div class="fw-semibold text-body">{{ $t('settings.dark_mode') }}</div>
              <p class="text-muted small mb-0">{{ $t('settings.dark_mode_desc') }}</p>
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
      </div>

      <!-- Right Column: Account Quick Card -->
      <div class="col-12 col-lg-4">
        <div class="card border-0 shadow-sm rounded-3 p-4 text-center bg-body">
          <div class="user-avatar-large mx-auto mb-3 bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width: 72px; height: 72px; font-size: 28px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
            {{ userInitial }}
          </div>
          <h4 class="fw-bold text-body mb-1 text-truncate" :title="userEmail">{{ userEmail }}</h4>
          <span class="badge text-uppercase font-monospace bg-light text-secondary border rounded-pill px-3 py-1.5" style="font-size: 10px;">
            {{ $t('dashboard.member_role', { role: projectStore.userRole }) }}
          </span>

          <hr class="my-4" />

          <div class="text-start">
            <div class="mb-3">
              <span class="text-secondary small d-block">{{ $t('settings.account_name') }}</span>
              <span class="text-body fw-medium">{{ userEmail.split('@')[0] }}</span>
            </div>
            <div class="mb-3">
              <span class="text-secondary small d-block">{{ $t('settings.active_workspace') }}</span>
              <span class="text-body fw-medium">{{ projectStore.currentProject?.Name || 'None' }}</span>
            </div>
            <div>
              <span class="text-secondary small d-block">{{ $t('settings.connection_status') }}</span>
              <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill">{{ $t('common.active') }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Column Modal -->
    <Teleport to="body">
      <div v-if="colModal.open" class="modal-backdrop show" style="background: rgba(0,0,0,0.5);"></div>
      <div v-if="colModal.open" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content border-0 shadow-lg rounded-4">
            <div class="modal-header border-bottom p-4">
              <h5 class="modal-title fw-bold text-body">{{ colModal.isEdit ? $t('settings.edit_column') : $t('settings.add_column') }}</h5>
              <button type="button" class="btn-close" @click="closeColModal" aria-label="Close"></button>
            </div>
            <div class="modal-body p-4">
              <form @submit.prevent="saveColumn">
                <div class="mb-3">
                  <label class="form-label small fw-semibold text-muted">{{ $t('settings.col_name') }}</label>
                  <input v-model="colForm.Name" type="text" class="form-control" required style="border-radius: 8px;" />
                </div>
                <div class="mb-3">
                  <label class="form-label small fw-semibold text-muted">{{ $t('settings.col_order') }}</label>
                  <input v-model.number="colForm.Order" type="number" min="0" class="form-control" required style="border-radius: 8px;" />
                </div>
                <div class="mb-3 form-check">
                  <input v-model="colForm.IsCompletedStage" type="checkbox" class="form-check-input" id="isCompletedStage" />
                  <label class="form-check-label small fw-semibold text-muted" for="isCompletedStage">{{ $t('settings.is_completed_stage') }}</label>
                  <div class="form-text small">{{ $t('settings.completed_stage_desc') }}</div>
                </div>
                <div class="d-flex justify-content-end gap-2 mt-4 pt-2">
                  <button type="button" class="btn btn-outline-secondary" @click="closeColModal" style="border-radius: 8px;">{{ $t('settings.cancel') }}</button>
                  <button type="submit" class="btn btn-primary" :disabled="colSaving" style="border-radius: 8px;">
                    <span v-if="colSaving" class="spinner-border spinner-border-sm me-1" role="status"></span>
                    {{ $t('settings.save') }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useProjectStore } from '../stores/projectStore.js'
import { updateProject, deleteProject, getProjectColumns, createProjectColumn, updateProjectColumn, deleteProjectColumn } from '../services/projectService.js'
import { toastSuccess, toastError, confirm, extractMessage } from '../utils/swal.js'

const router = useRouter()
const projectStore = useProjectStore()
const { t } = useI18n()
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
    loadColumns()
  }
}

// Columns state
const columns = ref([])
const loadingColumns = ref(false)
const colModal = ref({ open: false, isEdit: false, colId: null })
const colForm = ref({ Name: '', Order: 0, IsCompletedStage: false })
const colSaving = ref(false)

const loadColumns = async () => {
  if (!projectStore.currentProjectId) return
  loadingColumns.value = true
  try {
    const res = await getProjectColumns(projectStore.currentProjectId)
    columns.value = (res?.data || []).sort((a, b) => a.Order - b.Order)
  } catch(e) {
    console.error(e)
  } finally {
    loadingColumns.value = false
  }
}

const openColModal = (col = null) => {
  if (col) {
    colModal.value = { open: true, isEdit: true, colId: col.Id }
    colForm.value = { Name: col.Name, Order: col.Order, IsCompletedStage: col.IsCompletedStage }
  } else {
    colModal.value = { open: true, isEdit: false, colId: null }
    colForm.value = { Name: '', Order: columns.value.length, IsCompletedStage: false }
  }
}

const closeColModal = () => {
  colModal.value.open = false
}

const saveColumn = async () => {
  colSaving.value = true
  try {
    if (colModal.value.isEdit) {
      await updateProjectColumn(projectStore.currentProjectId, colModal.value.colId, colForm.value)
      toastSuccess(t('settings.col_updated'))
    } else {
      await createProjectColumn(projectStore.currentProjectId, colForm.value)
      toastSuccess(t('settings.col_added'))
    }
    closeColModal()
    await loadColumns()
  } catch(e) {
    toastError(extractMessage(e, t('errors.default')))
  } finally {
    colSaving.value = false
  }
}

const handleDeleteColumn = async (colId) => {
  const ok = await confirm(t('settings.del_col_title'), t('settings.del_col_desc'), t('settings.del_col_btn'))
  if (!ok) return
  try {
    await deleteProjectColumn(projectStore.currentProjectId, colId)
    toastSuccess(t('settings.col_deleted'))
    await loadColumns()
  } catch(e) {
    toastError(extractMessage(e, t('errors.default')))
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
    toastError(extractMessage(err, t('errors.default')))
  }
}

const handleDeleteProject = async () => {
  if (!projectStore.currentProject) return
  const currentProj = projectStore.currentProject

  const ok = await confirm(
    t('settings.delete_confirm_title'),
    t('settings.delete_confirm_desc', { name: currentProj.Name }),
    t('settings.delete_confirm_btn')
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
    toastError(extractMessage(err, t('errors.default')))
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
  font-weight: 700;
  color: var(--bs-heading-color) !important;
}
</style>
