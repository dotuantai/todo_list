<template>
  <Teleport to="body">
    <!-- Modal Backdrop -->
    <div v-if="show" class="modal-backdrop show" style="background: rgba(0,0,0,0.5); z-index: 1040;"></div>
    
    <!-- Modal Wrapper -->
    <div 
      v-if="show" 
      class="modal fade show d-block" 
      tabindex="-1" 
      role="dialog" 
      aria-modal="true" 
      style="overflow-y: auto; z-index: 1050;"
    >
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content border-0 shadow-lg rounded-3">

          <!-- Modal Header -->
          <div class="modal-header border-bottom p-4">
            <div class="text-start">
              <h1 class="modal-title h4 fw-bold mb-1 text-body">{{ $t('taskModal.SCR0233') }}</h1>
              <p class="text-muted small mb-0">{{ $t('taskModal.SCR0242') }}</p>
            </div>
            <button type="button" class="btn-close" @click="closeModal" :aria-label="$t('taskModal.SCR0255')"></button>
          </div>

          <div class="modal-body p-4 text-start">
            <form @submit.prevent="handleSubmit">

              <!-- Task Title -->
              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.SCR0235') }} <span class="text-danger">*</span></label>
                <input 
                  v-model="form.title"
                  type="text" 
                  class="form-control" 
                  :placeholder="$t('taskModal.SCR0248')"
                  required
                />
              </div>

              <div class="row g-3 mb-4">
                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.SCR0245') }} <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-color: var(--bs-border-color);"><i class="bi bi-calendar-event"></i></span>
                    <input v-model="form.startDate" type="date" class="form-control text-body border-start-0" style="border-color: var(--bs-border-color); padding-left: 0;" required />
                  </div>
                </div>
                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.SCR0237') }} <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-color: var(--bs-border-color);"><i class="bi bi-calendar3"></i></span>
                    <input v-model="form.deadline" type="date" class="form-control text-body border-start-0" style="border-color: var(--bs-border-color); padding-left: 0;" required />
                  </div>
                </div>

                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.SCR0246') }}</label>
                  <div class="input-group">
                    <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-color: var(--bs-border-color);"><i class="bi bi-stopwatch"></i></span>
                    <input v-model="form.estimatedHours" type="number" step="0.5" min="0" :placeholder="$t('taskModal.SCR0249')" class="form-control text-body border-start-0" style="border-color: var(--bs-border-color); padding-left: 0;" />
                  </div>
                </div>
                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.SCR0247') }}</label>
                  <div class="input-group">
                    <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-color: var(--bs-border-color);"><i class="bi bi-clock-history"></i></span>
                    <input v-model="form.actualHours" type="number" step="0.5" min="0" :placeholder="$t('taskModal.SCR0250')" class="form-control text-body border-start-0" style="border-color: var(--bs-border-color); padding-left: 0;" />
                  </div>
                </div>

                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.SCR0238') }}</label>
                  <div v-if="loadingColumns" class="d-flex align-items-center gap-2 py-2">
                    <span class="spinner-border spinner-border-sm text-primary" role="status"></span>
                    <span class="text-muted small">{{ $t('common.SCR0009') }}</span>
                  </div>
                  <div v-else class="input-group">
                    <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-color: var(--bs-border-color);"><i class="bi bi-kanban"></i></span>
                    <select v-model="form.columnId" class="form-select text-body border-start-0" style="border-color: var(--bs-border-color); padding-left: 0;">
                      <option v-for="col in columns" :key="col.Id" :value="col.Id">{{ col.Name }}</option>
                    </select>
                  </div>
                </div>
                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('tasks.SCR0246') }}</label>
                  <div class="input-group">
                    <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-color: var(--bs-border-color);"><i class="bi bi-flag"></i></span>
                    <select v-model="form.priority" class="form-select text-body border-start-0" style="border-color: var(--bs-border-color); padding-left: 0;">
                      <option value="Low">{{ $t('tasks.SCR0224') }}</option>
                      <option value="Medium">{{ $t('tasks.SCR0225') }}</option>
                      <option value="High">{{ $t('tasks.SCR0226') }}</option>
                    </select>
                  </div>
                </div>

                <div class="col-12 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.SCR0252') }} <span class="text-muted text-lowercase" style="font-size: 0.75rem;">({{ $t('taskModal.SCR0253') }})</span></label>
                  <div class="input-group">
                    <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-color: var(--bs-border-color);"><i class="bi bi-person"></i></span>
                    <select v-model="form.assigneeId" class="form-select text-body border-start-0" style="border-color: var(--bs-border-color); padding-left: 0;">
                    <option :value="null">{{ $t('taskModal.SCR0254') }}</option>
                      <option v-for="m in members" :key="m.UserId" :value="m.UserId">
                        {{ m.Email }} ({{ getRoleLabel(t, m.Role) }})
                      </option>
                    </select>
                  </div>
                </div>
              </div>

              <!-- Description -->
              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.SCR0236') }}</label>
                <textarea 
                  v-model="form.description"
                  class="form-control text-body" 
                  rows="5"
                  :placeholder="$t('taskModal.SCR0251')"
                  style="border-color: var(--bs-border-color);"
                ></textarea>
              </div>

              <!-- Footer Actions -->
              <div class="d-flex justify-content-end gap-2 pt-4 border-top">
                <button 
                  type="button" 
                  class="btn btn-outline-secondary px-4 py-2 fw-semibold" 
                  @click="closeModal"
                  :disabled="loading"
                  style="border-radius: 8px;"
                >
                  {{ $t('taskModal.SCR0244') }}
                </button>
                <button 
                  type="submit" 
                  class="btn btn-primary px-4 py-2 fw-semibold"
                  :disabled="loading"
                  style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
                >
                  <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                  {{ loading ? $t('common.SCR0009') : $t('taskModal.SCR0242') }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { ref, defineExpose } from 'vue'
import { useI18n } from 'vue-i18n'
import { getRoleLabel } from '../utils/i18nLabels.js'
import { createProjectTask, getProjectColumns, getMembers } from '../services/projectService.js'
import { useProjectStore } from '../stores/projectStore.js'
const projectStore = useProjectStore()
import { toastSuccess, toastError, toastWarning, extractMessage } from '../utils/swal.js'

const { t } = useI18n()

const show = ref(false)
const loading = ref(false)
const loadingColumns = ref(false)
const columns = ref([])
const members = ref([])

const form = ref({
  title: '',
  description: '',
  startDate: '',
  deadline: '',
  estimatedHours: null,
  actualHours: null,
  columnId: null,
  priority: 'Medium',
  assigneeId: null
})

const loadColumns = async () => {
  if (!projectStore.currentProjectId) return
  loadingColumns.value = true
  try {
    const res = await getProjectColumns(projectStore.currentProjectId)
    columns.value = res?.data || []
    // Default to first column (lowest order)
    if (columns.value.length > 0 && !form.value.columnId) {
      form.value.columnId = columns.value[0].Id
    }
  } catch (e) {
    console.error('Failed to load columns', e)
  } finally {
    loadingColumns.value = false
  }
}

const loadMembers = async () => {
  if (!projectStore.currentProjectId) return
  try {
    const res = await getMembers(projectStore.currentProjectId)
    members.value = res?.data?.Data || res?.data || []
  } catch (e) {
    console.error('Failed to load members', e)
  }
}

const openModal = () => {
  form.value = {
    title: '',
    description: '',
    startDate: '',
    deadline: '',
    estimatedHours: null,
    actualHours: null,
    columnId: null,
    priority: 'Medium',
    assigneeId: null
  }
  show.value = true
  document.body.style.overflow = 'hidden'
  loadColumns()
  loadMembers()
}

const closeModal = () => {
  show.value = false
  document.body.style.overflow = ''
  loading.value = false
}

const handleSubmit = async () => {
  if (!form.value.title.trim()) {
    toastWarning(t('taskModal.SCR0256'))
    return
  }

  if (!projectStore.currentProjectId) {
    toastWarning(t('tasks.SCR0203'))
    return
  }

  if (!form.value.columnId) {
    toastWarning(t('taskModal.SCR0256'))
    return
  }

  loading.value = true

  try {
    const payload = {
      title: form.value.title,
      description: form.value.description,
      startDate: form.value.startDate || null,
      deadline: form.value.deadline || null,
      estimatedHours: form.value.estimatedHours,
      actualHours: form.value.actualHours,
      columnId: form.value.columnId,
      priority: form.value.priority,
      assigneeId: form.value.assigneeId
    }

    await createProjectTask(projectStore.currentProjectId, payload)

    toastSuccess(t('common.SCR0011'))
    closeModal()

    // Notify TaskView to reload the list
    window.dispatchEvent(new CustomEvent('task-created'))

  } catch (error) {
    console.error('Create task failed:', error)
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    loading.value = false
  }
}

defineExpose({ openModal })
</script>
