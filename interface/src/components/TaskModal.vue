<template>
  <Teleport to="body">
    <div 
      v-if="show" 
      class="modal fade show" 
      style="display: block; background: rgba(0,0,0,0.65);"
    >
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content shadow-xl">

          <!-- Modal Header -->
          <div class="modal-header border-0 pb-2">
            <div>
              <h1 class="modal-title h4 fw-bold mb-1">Create New Task</h1>
              <p class="text-muted small mb-0">Draft a new objective and assign it to your team.</p>
            </div>
            <button type="button" class="btn-close" @click="closeModal"></button>
          </div>

          <div class="modal-body pt-0">
            <form @submit.prevent="handleSubmit">

              <!-- Task Title -->
              <div class="mb-4">
                <label class="form-label fw-semibold">Task Title <span class="text-danger">*</span></label>
                <input 
                  v-model="form.title"
                  type="text" 
                  class="form-control form-control-lg" 
                  placeholder="e.g. Design System Implementation"
                  required
                />
              </div>

              <!-- Deadline + Status -->
              <div class="row g-4 mb-4">
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Deadline</label>
                  <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-calendar3"></i></span>
                    <input v-model="form.deadline" type="date" class="form-control" />
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Status</label>
                  <select v-model="form.status" class="form-select">
                    <option value="ToDo">To Do</option>
                    <option value="InProgress">In Progress</option>
                    <option value="Done">Done</option>
                    <option value="Closed">Closed</option>
                  </select>
                </div>
              </div>

              <!-- Description -->
              <div class="mb-5">
                <label class="form-label fw-semibold">Description</label>
                <div class="border rounded-3 overflow-hidden">
                  <div class="bg-light border-bottom p-2 d-flex gap-1">
                    <button type="button" class="btn btn-sm btn-light"><i class="bi bi-type-bold"></i></button>
                    <button type="button" class="btn btn-sm btn-light"><i class="bi bi-type-italic"></i></button>
                    <button type="button" class="btn btn-sm btn-light"><i class="bi bi-list-ul"></i></button>
                    <button type="button" class="btn btn-sm btn-light"><i class="bi bi-link"></i></button>
                  </div>
                  <textarea 
                    v-model="form.description"
                    class="form-control border-0 shadow-none" 
                    rows="5"
                    placeholder="Describe the task details here..."
                  ></textarea>
                </div>
              </div>

              

              <!-- Footer Actions -->
              <div class="d-flex justify-content-end gap-3 pt-4 border-top">
                <button 
                  type="button" 
                  class="btn btn-light px-5" 
                  @click="closeModal"
                  :disabled="loading"
                >
                  Cancel
                </button>
                <button 
                  type="submit" 
                  class="btn btn-primary px-5 fw-semibold"
                  :disabled="loading"
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
import { createTask } from '../Services/taskService.js'  

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
  if (!form.value.title) {
    alert("Please enter task title")
    return
  }

  loading.value = true

  try {
    const payload = {
      title: form.value.title,
      description: form.value.description,
      deadline: form.value.deadline || null,
      Status: form.value.status   // ← Quan trọng: API dùng "Status" (chữ S hoa)
    }

    const response = await createTask(payload)
    
    console.log('Task created successfully:', response.data)
    alert('✅ Task created successfully!')
    
    closeModal()
    
    // Tùy chọn: emit event để reload danh sách task ở trang khác
    // emit('task-created')

  } catch (error) {
    console.error('Create task failed:', error)
    alert('❌ Failed to create task. Please try again.')
  } finally {
    loading.value = false
  }
}

defineExpose({ openModal })
</script>

<style scoped>
.modal-content {
  border-radius: 16px;
  border: none;
}
.avatar {
  width: 42px;
  height: 42px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
}
</style>