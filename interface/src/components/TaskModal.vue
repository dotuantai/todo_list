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
              <h1 class="modal-title h4 fw-bold mb-1 text-body">{{ $t('taskModal.create_title') }}</h1>
              <p class="text-muted small mb-0">{{ $t('taskModal.btn_create') }}</p>
            </div>
            <button type="button" class="btn-close" @click="closeModal" aria-label="Close"></button>
          </div>

          <div class="modal-body p-4 text-start">
            <form @submit.prevent="handleSubmit">

              <!-- Task Title -->
              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.task_name') }} <span class="text-danger">*</span></label>
                <input 
                  v-model="form.title"
                  type="text" 
                  class="form-control" 
                  placeholder="e.g. Design System Implementation"
                  required
                />
              </div>

              <!-- Deadline + Status -->
              <div class="row g-3 mb-4">
                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.deadline') }}</label>
                  <div class="input-group">
                    <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-color: var(--bs-border-color);"><i class="bi bi-calendar3"></i></span>
                    <input v-model="form.deadline" type="date" class="form-control text-body" style="border-color: var(--bs-border-color);" />
                  </div>
                </div>
                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.status') }}</label>
                  <select v-model="form.status" class="form-select text-body">
                    <option value="ToDo">{{ $t('dashboard.todo') }}</option>
                    <option value="InProgress">{{ $t('dashboard.in_progress') }}</option>
                    <option value="Done">{{ $t('dashboard.done') }}</option>
                    <option value="Closed">{{ $t('dashboard.closed') }}</option>
                  </select>
                </div>
              </div>

              <!-- Description -->
              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('taskModal.description') }}</label>
                <div class="border rounded-3 overflow-hidden bg-body" style="border-color: var(--bs-border-color) !important;">
                  <div class="bg-body-secondary border-bottom p-2 d-flex gap-1" style="border-color: var(--bs-border-color) !important;">
                    <button type="button" class="btn btn-sm btn-light border-0"><i class="bi bi-type-bold"></i></button>
                    <button type="button" class="btn btn-sm btn-light border-0"><i class="bi bi-type-italic"></i></button>
                    <button type="button" class="btn btn-sm btn-light border-0"><i class="bi bi-list-ul"></i></button>
                    <button type="button" class="btn btn-sm btn-light border-0"><i class="bi bi-link"></i></button>
                  </div>
                  <textarea 
                    v-model="form.description"
                    class="form-control border-0 shadow-none rounded-0 bg-transparent" 
                    rows="5"
                    placeholder="Describe the task details here..."
                  ></textarea>
                </div>
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
                  {{ $t('taskModal.btn_cancel') }}
                </button>
                <button 
                  type="submit" 
                  class="btn btn-primary px-4 py-2 fw-semibold"
                  :disabled="loading"
                  style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
                >
                  <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                  {{ loading ? $t('common.loading') : $t('taskModal.btn_create') }}
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
import { createProjectTask } from '../services/projectService.js'
import { useProjectStore } from '../stores/projectStore.js'
const projectStore = useProjectStore()
import { toastSuccess, toastError, toastWarning, extractMessage } from '../utils/swal.js'

const { t } = useI18n()

const show = ref(false)
const loading = ref(false)

const form = ref({
  title: '',
  description: '',
  deadline: '',
  status: 'ToDo'
})

const openModal = () => {
  form.value = {
    title: '',
    description: '',
    deadline: '',
    status: 'ToDo'
  }
  show.value = true
  document.body.style.overflow = 'hidden'
}

const closeModal = () => {
  show.value = false
  document.body.style.overflow = ''
  loading.value = false
}

const handleSubmit = async () => {
  if (!form.value.title.trim()) {
    toastWarning(t('errors.Please complete all fields'))
    return
  }

  if (!projectStore.currentProjectId) {
    toastWarning(t('tasks.welcome_desc'))
    return
  }

  loading.value = true

  try {
    const payload = {
      title: form.value.title,
      description: form.value.description,
      deadline: form.value.deadline || null,
      Status: form.value.status
    }

    await createProjectTask(projectStore.currentProjectId, payload)

    toastSuccess(t('common.success'))
    closeModal()

    // Notify TaskView to reload the list
    window.dispatchEvent(new CustomEvent('task-created'))

  } catch (error) {
    console.error('Create task failed:', error)
    toastError(extractMessage(error, t('errors.default')))
  } finally {
    loading.value = false
  }
}

defineExpose({ openModal })
</script>