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
              <h1 class="modal-title h4 fw-bold mb-1 text-dark">Create New Task</h1>
              <p class="text-muted small mb-0">Draft a new objective and assign it to your team.</p>
            </div>
            <button type="button" class="btn-close" @click="closeModal" aria-label="Close"></button>
          </div>

          <div class="modal-body p-4 text-start">
            <form @submit.prevent="handleSubmit">

              <!-- Task Title -->
              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Task Title <span class="text-danger">*</span></label>
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
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Deadline</label>
                  <div class="input-group">
                    <span class="input-group-text bg-light text-muted"><i class="bi bi-calendar3"></i></span>
                    <input v-model="form.deadline" type="date" class="form-control text-dark" />
                  </div>
                </div>
                <div class="col-md-6 text-start">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Status</label>
                  <select v-model="form.status" class="form-select text-dark">
                    <option value="ToDo">To Do</option>
                    <option value="InProgress">In Progress</option>
                    <option value="Done">Done</option>
                    <option value="Closed">Closed</option>
                  </select>
                </div>
              </div>

              <!-- Description -->
              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Description</label>
                <div class="border rounded-3 overflow-hidden bg-white">
                  <div class="bg-light border-bottom p-2 d-flex gap-1">
                    <button type="button" class="btn btn-sm btn-light border-0"><i class="bi bi-type-bold"></i></button>
                    <button type="button" class="btn btn-sm btn-light border-0"><i class="bi bi-type-italic"></i></button>
                    <button type="button" class="btn btn-sm btn-light border-0"><i class="bi bi-list-ul"></i></button>
                    <button type="button" class="btn btn-sm btn-light border-0"><i class="bi bi-link"></i></button>
                  </div>
                  <textarea 
                    v-model="form.description"
                    class="form-control border-0 shadow-none rounded-0" 
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
                  Cancel
                </button>
                <button 
                  type="submit" 
                  class="btn btn-primary px-4 py-2 fw-semibold"
                  :disabled="loading"
                  style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;"
                >
                  <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                  {{ loading ? 'Creating...' : 'Create Task' }}
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
import { createProjectTask } from '../services/projectService.js'
import { projectStore } from '../utils/projectStore.js'
import { toastSuccess, toastError, toastWarning, extractMessage } from '../utils/swal.js'

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
    toastWarning('Vui lòng nhập tiêu đề task!')
    return
  }

  if (!projectStore.currentProjectId) {
    toastWarning('Vui lòng chọn một dự án trước!')
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

    toastSuccess('Tạo task thành công!')
    closeModal()

    // Thông báo cho TaskView reload danh sách
    window.dispatchEvent(new CustomEvent('task-created'))

  } catch (error) {
    console.error('Create task failed:', error)
    toastError(extractMessage(error, 'Tạo task thất bại.'))
  } finally {
    loading.value = false
  }
}

defineExpose({ openModal })
</script>