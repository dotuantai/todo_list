<template>
  <div class="p-3 p-md-4">

    <!-- Page header -->
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
     
      <!-- <div class="d-flex align-items-center gap-2">
        <button class="btn btn-outline-secondary d-flex align-items-center justify-content-center" @click="refreshAll" :disabled="loading" title="Refresh" style="width: 38px; height: 38px; border-radius: 8px;">
          <svg v-if="!loading" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          <span v-else class="spinner-border spinner-border-sm text-secondary" role="status"></span>
        </button>
      </div> -->
    </div>

    <!-- Empty Project Selection State -->
    <div v-if="!projectStore.currentProjectId" class="text-center py-5 bg-body rounded-4 shadow-sm border border-dashed p-4">
      <i class="bi bi-folder2-open text-primary" style="font-size: 4rem;"></i>
      <h3 class="fw-bold text-body mt-3">Welcome to TaskFlow Pro</h3>
      <p class="text-muted mx-auto" style="max-width: 480px;">Please select a project from the sidebar or create a new one to start managing your tasks.</p>
    </div>

    <!-- Kanban Board -->
    <div v-else class="row g-3 text-start align-items-start">
      <div v-for="col in columns"
          :key="col.status"
          class="col-12 col-md-6 col-lg-3"
        >
        <div 
          class="card bg-body-tertiary border-0 shadow-sm rounded-3 p-3 kanban-col"
          :class="{ 'kanban-col--dragover': dragOverCol === col.status }"
          @dragover="onDragOver($event, col.status)"
          @dragleave="onDragLeave"
          @drop="onDrop($event, col.status)"
        >
          <!-- Column Header -->
          <div class="d-flex align-items-center gap-2 mb-3">
            <span class="col-dot rounded-circle" :style="{ background: col.color, width: '10px', height: '10px', display: 'inline-block' }"></span>
            <span class="fw-bold text-uppercase text-secondary" style="font-size: 0.8rem; letter-spacing: 0.05em;">{{ col.label }}</span>
            <span class="badge rounded-pill ms-auto" :style="{ background: col.bgMid, color: col.color }">
              {{ getTasksByStatus(col.status).length }}
            </span>
          </div>

          <!-- Column Cards List -->
          <div v-if="loading" class="d-flex flex-column gap-2">
            <div v-for="n in 3" :key="n" class="skeleton-card bg-body rounded-3 shadow-sm w-100" style="height: 100px;"></div>
          </div>

          <div v-else-if="getTasksByStatus(col.status).length === 0" class="text-center py-4 border border-dashed rounded-3 bg-body text-muted">
            <i class="bi bi-inbox d-block mb-1 fs-4 text-secondary opacity-50"></i>
            <span class="small" style="font-size: 0.85rem;">No tasks</span>
          </div>

          <div v-else class="d-flex flex-column gap-2">
            <div
              v-for="task in getTasksByStatus(col.status)"
              :key="task.Id"
              class="card border-0 border-top border-4 shadow-sm task-tag-card p-3"
              :class="{ 'task-tag-card--dragging': draggingTask?.Id === task.Id }"
              :style="{ borderTopColor: col.color, cursor: projectStore.userRole !== 'Viewer' ? 'grab' : 'default' }"
              :draggable="projectStore.userRole !== 'Viewer'"
              @dragstart="onDragStart(task)"
              @dragend="onDragEnd"
              @click="openModal(task)"
            >
              <span class="fw-bold text-body mb-2 text-start d-block" style="font-size: 0.95rem; line-height: 1.4;">{{ task.Title }}</span>
              <div class="d-flex flex-column gap-1 align-items-start">
                <span class="text-muted small d-flex align-items-center gap-1.5" style="font-size: 0.75rem;">
                  <i class="bi bi-calendar3"></i>
                  {{ formatDateShort(task.CreatedAt) }}
                </span>
                <span v-if="task.Deadline" class="small d-flex align-items-center gap-1.5" :class="isOverdue(task) ? 'text-danger fw-bold' : 'text-muted'" style="font-size: 0.75rem;">
                  <i :class="isOverdue(task) ? 'bi bi-exclamation-circle-fill' : 'bi bi-clock'"></i>
                  {{ formatDateShort(task.Deadline) }}
                  <span v-if="isOverdue(task)" class="badge bg-danger bg-opacity-10 text-danger rounded-pill px-2 py-0.5 ms-1" style="font-size: 0.65rem;">Overdue</span>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>



    <!-- ── Task Detail Modal ── -->
    <Teleport to="body">
      <div v-if="modal.open" class="modal-backdrop show" style="background: rgba(0,0,0,0.5);"></div>
      <div v-if="modal.open" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true" style="overflow-y: auto;">
        <div class="modal-dialog modal-dialog-centered modal-lg">
          <div class="modal-content border-0 shadow-lg rounded-4">
            
            <!-- Modal Header -->
            <div class="modal-header border-bottom p-4" :style="{ background: getColByStatus(modal.task?.Status)?.bgLight }">
              <div class="text-start flex-grow-1">
                <div class="d-flex gap-2 flex-wrap mb-2">
                  <span class="badge text-uppercase font-monospace" :style="{ background: getColByStatus(modal.task?.Status)?.bgMid, color: getColByStatus(modal.task?.Status)?.color }">
                    {{ getColByStatus(modal.task?.Status)?.label }}
                  </span>
                  <span v-if="modal.task && isOverdue(modal.task)" class="badge bg-danger bg-opacity-10 text-danger rounded-pill">
                    Overdue
                  </span>
                </div>
                <h5 class="modal-title fw-bold text-body h5 mb-0 text-start">{{ modal.task?.Title }}</h5>
              </div>
              <div class="d-flex gap-1 align-items-center">
                <button v-if="projectStore.userRole !== 'Viewer'" class="btn btn-sm btn-light border p-2" :class="{ 'btn-primary text-white': editMode }" @click="toggleEdit" title="Edit task" style="border-radius: 8px; width: 34px; height: 34px; display: flex; align-items: center; justify-content: center;">
                  <i class="bi bi-pencil-fill"></i>
                </button>
                <button class="btn-close ms-2" @click="closeModal" aria-label="Close"></button>
              </div>
            </div>

            <!-- ── VIEW MODE ── -->
            <div v-if="!editMode" class="modal-body p-4 text-start">
              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Description</label>
                <div class="text-body bg-body-secondary p-3 rounded-3" style="white-space: pre-wrap; font-size: 0.95rem; line-height: 1.6;">
                  {{ modal.task?.Description || 'No description provided.' }}
                </div>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-6 col-md-3">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Created At</label>
                  <div class="text-body fw-medium">{{ formatDate(modal.task?.CreatedAt) }}</div>
                </div>
                <div class="col-6 col-md-3">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Deadline</label>
                  <div class="text-body fw-medium" :class="modal.task && isOverdue(modal.task) ? 'text-danger fw-bold' : ''">
                    {{ modal.task?.Deadline ? formatDate(modal.task.Deadline) : '—' }}
                  </div>
                </div>
                <div class="col-6 col-md-3">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Task ID</label>
                  <div class="text-muted font-monospace">#{{ modal.task?.Id }}</div>
                </div>
                <div class="col-6 col-md-3">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Status</label>
                  <div>
                    <span class="badge" :style="{ background: getColByStatus(modal.task?.Status)?.bgMid, color: getColByStatus(modal.task?.Status)?.color }">
                      {{ getColByStatus(modal.task?.Status)?.label }}
                    </span>
                  </div>
                </div>
              </div>

              <!-- Assigned Users list -->
              <div v-if="modal.task?.AssignedUsers" class="mb-2">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider d-flex align-items-center gap-2">
                  Assigned Users
                  <span class="badge bg-body-secondary text-secondary border rounded-pill">{{ modal.task.AssignedUsers.length }}</span>
                </label>
                <div v-if="modal.task.AssignedUsers.length === 0" class="text-muted small fst-italic py-2">No users assigned yet.</div>
                <div v-else class="row g-2 mt-1">
                  <div
                    v-for="user in modal.task.AssignedUsers"
                    :key="user.UserId"
                    class="col-12 col-md-6"
                  >
                    <div class="card bg-body-secondary border p-2.5 rounded-3 h-100">
                      <div class="d-flex align-items-center gap-2">
                        <div class="user-avatar bg-secondary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width:30px; height:30px; font-size:11px;">
                          {{ userInitial(user.Email) }}
                        </div>
                        <div class="flex-grow-1 min-w-0 text-start">
                          <div class="small fw-semibold text-body text-truncate" :title="user.Email">{{ user.Email }}</div>
                        </div>

                        <!-- Edit permission inline (Only for Owner/Editor) -->
                        <template v-if="editingPermUserId !== user.UserId && projectStore.userRole !== 'Viewer'">
                          <span v-if="user.CanView" class="badge bg-info bg-opacity-10 text-info" style="font-size: 0.7rem;"><i class="bi bi-eye-fill"></i> View</span>
                          <span v-if="user.CanEdit" class="badge bg-success bg-opacity-10 text-success ms-1" style="font-size: 0.7rem;"><i class="bi bi-pencil-fill"></i> Edit</span>
                          <button class="btn btn-sm btn-light border p-1 ms-1" @click="startEditPerm(user)" title="Edit permissions" style="width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center;">
                            <i class="bi bi-gear" style="font-size:11px"></i>
                          </button>
                          <button class="btn btn-sm btn-outline-danger p-1 ms-1" @click="removeUser(user)" title="Remove assignment" style="width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center;">
                            <i class="bi bi-trash3-fill" style="font-size:11px"></i>
                          </button>
                        </template>
                        <template v-else-if="editingPermUserId === user.UserId">
                          <button class="btn btn-sm btn-light border p-1 ms-auto" @click="cancelEditPerm" title="Cancel" style="width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center;">
                            <i class="bi bi-x-lg" style="font-size:10px"></i>
                          </button>
                        </template>
                        <template v-else>
                          <span v-if="user.CanView" class="badge bg-info bg-opacity-10 text-info" style="font-size: 0.7rem;"><i class="bi bi-eye-fill"></i> View</span>
                          <span v-if="user.CanEdit" class="badge bg-success bg-opacity-10 text-success ms-1" style="font-size: 0.7rem;"><i class="bi bi-pencil-fill"></i> Edit</span>
                        </template>
                      </div>

                      <div v-if="editingPermUserId === user.UserId" class="d-flex align-items-center gap-3 pt-2 mt-2 border-top">
                        <div class="form-check form-check-inline mb-0">
                          <input type="checkbox" class="form-check-input" :id="'view-chk-' + user.UserId" v-model="editingPerms.canView" />
                          <label class="form-check-label small" :for="'view-chk-' + user.UserId">View</label>
                        </div>
                        <div class="form-check form-check-inline mb-0">
                          <input type="checkbox" class="form-check-input" :id="'edit-chk-' + user.UserId" v-model="editingPerms.canEdit" />
                          <label class="form-check-label small" :for="'edit-chk-' + user.UserId">Edit</label>
                        </div>
                        <button
                          class="btn btn-sm btn-primary ms-auto px-3 py-1"
                          style="font-size: 0.75rem;"
                          @click="savePermission(user)"
                          :disabled="!editingPerms.canView && !editingPerms.canEdit"
                        >
                          Save
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- ── EDIT MODE ── -->
            <div v-else class="modal-body p-4 text-start">
              <div class="alert alert-primary bg-primary bg-opacity-10 border-0 text-primary d-flex align-items-center gap-2 rounded-3 mb-3">
                <i class="bi bi-info-circle-fill"></i>
                <span>Editing task <strong>#{{ modal.task?.Id }}</strong></span>
              </div>

              <div class="mb-3">
                <label class="form-label fw-semibold text-secondary small text-uppercase">Title</label>
                <input id="edit-title" v-model="editForm.title" type="text" class="form-control" placeholder="Task title" />
              </div>

              <div class="mb-3">
                <label class="form-label fw-semibold text-secondary small text-uppercase">Description</label>
                <textarea id="edit-desc" v-model="editForm.description" class="form-control" rows="4" placeholder="Task description"></textarea>
              </div>

              <div class="row g-3">
                <div class="col-12 col-md-6">
                  <label class="form-label fw-semibold text-secondary small text-uppercase">Deadline</label>
                  <input id="edit-deadline" v-model="editForm.deadline" type="datetime-local" class="form-control" />
                </div>
                <div class="col-12 col-md-6">
                  <label class="form-label fw-semibold text-secondary small text-uppercase">Status</label>
                  <select id="edit-status" v-model="editForm.status" class="form-select">
                    <option v-for="col in columns" :key="col.status" :value="col.status">{{ col.label }}</option>
                  </select>
                </div>
              </div>

              <div class="mt-4 pt-3 border-top text-end">
                <button class="btn btn-sm btn-outline-danger px-3 py-2 fw-semibold" @click="handleDeleteTask">
                  <i class="bi bi-trash3 me-1"></i> Delete Task
                </button>
              </div>
            </div>

            <!-- ── FOOTER — View mode, Assign Users (Only for Owner/Editor) ── -->
            <div v-if="!editMode && projectStore.userRole !== 'Viewer'" class="modal-footer p-4 border-top bg-body-secondary d-flex align-items-center gap-3 flex-wrap">
              <div class="flex-grow-1 text-start" style="min-width: 250px;">
                <select v-model="selectedAssigneeId" class="form-select form-select-sm" style="border-radius: 8px; height: 38px;">
                  <option :value="null">-- Select member to assign --</option>
                  <option v-for="m in projectMembersNotAssigned" :key="m.UserId" :value="m.UserId">
                    {{ m.Email }} ({{ m.Role }})
                  </option>
                </select>
              </div>

              <div class="d-flex align-items-center gap-3 bg-body border rounded-3 px-3 py-1.5" style="height: 38px;">
                <div class="form-check form-check-inline mb-0">
                  <input type="checkbox" class="form-check-input" id="assign-view-chk" v-model="assignPerms.canView" />
                  <label class="form-check-label small" for="assign-view-chk">View</label>
                </div>
                <div class="form-check form-check-inline mb-0">
                  <input type="checkbox" class="form-check-input" id="assign-edit-chk" v-model="assignPerms.canEdit" />
                  <label class="form-check-label small" for="assign-edit-chk">Edit</label>
                </div>
              </div>

              <div class="ms-auto d-flex gap-2">
                <button class="btn btn-sm btn-primary fw-semibold" @click="assignUser" :disabled="!selectedAssigneeId || (!assignPerms.canView && !assignPerms.canEdit)" style="height: 38px; border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none; padding: 0 16px;">
                  <i class="bi bi-person-plus-fill me-1"></i> Assign
                </button>
                <button class="btn btn-sm btn-outline-secondary" @click="closeModal" style="height: 38px; border-radius: 8px; padding: 0 16px;">Close</button>
              </div>
            </div>

            <!-- ── FOOTER — View mode, Viewer ── -->
            <div v-else-if="!editMode" class="modal-footer p-4 border-top bg-body-secondary text-end">
              <button class="btn btn-sm btn-outline-secondary px-4 py-2" @click="closeModal" style="border-radius: 8px;">Close</button>
            </div>

            <!-- ── FOOTER — Edit mode ── -->
            <div v-else class="modal-footer p-4 border-top bg-body-secondary d-flex justify-content-end gap-2">
              <button class="btn btn-sm btn-outline-secondary px-3 py-2 fw-semibold" @click="cancelEdit" style="border-radius: 8px;">
                <i class="bi bi-x me-1"></i> Cancel
              </button>
              <button class="btn btn-sm btn-primary px-3 py-2 fw-semibold" @click="saveEdit" :disabled="saving" style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;">
                <span v-if="saving" class="spinner-border spinner-border-sm me-2" role="status"></span>
                <i v-else class="bi bi-check2 me-1"></i>
                {{ saving ? 'Saving…' : 'Save changes' }}
              </button>
            </div>

          </div>
        </div>
      </div>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted, watch } from 'vue'
import { assignTask, updateTask, updatePermission, removeAssignment, updateStatusTask, deleteTask } from '../services/taskService.js'
import { getMembers, addMember, updateMemberRole, removeMember, getProjectTasks, updateProject, deleteProject } from '../services/projectService.js'
import { useProjectStore } from '../stores/projectStore.js'
const projectStore = useProjectStore()
import { toastSuccess, toastError, confirm, extractMessage } from '../utils/swal.js'
import Swal from 'sweetalert2'

const tasks         = ref([])
const loading       = ref(false)
const saving        = ref(false)
const modal         = reactive({ open: false, task: null })
const editMode      = ref(false)
const editForm      = reactive({ title: '', description: '', deadline: '', status: '' })

// Project members states
const members        = ref([])
const loadingMembers = ref(false)

// Assignee selection states
const selectedAssigneeId = ref(null)
const assignPerms        = reactive({ canView: true, canEdit: false })
const editingPermUserId  = ref(null)
const editingPerms       = reactive({ canView: false, canEdit: false })

// Drag states
const draggingTask       = ref(null)
const draggingFromStatus = ref(null)
const dragOverCol        = ref(null)

const columns = [
  { status: 'ToDo',       label: 'To Do',       color: 'var(--status-todo-color)', bgLight: 'var(--status-todo-bg-light)', bgMid: 'var(--status-todo-bg-mid)' },
  { status: 'InProgress', label: 'In Progress',  color: 'var(--status-inprogress-color)', bgLight: 'var(--status-inprogress-bg-light)', bgMid: 'var(--status-inprogress-bg-mid)' },
  { status: 'Done',       label: 'Done',         color: 'var(--status-done-color)', bgLight: 'var(--status-done-bg-light)', bgMid: 'var(--status-done-bg-mid)' },
  { status: 'Closed',     label: 'Closed',       color: 'var(--status-closed-color)', bgLight: 'var(--status-closed-bg-light)', bgMid: 'var(--status-closed-bg-mid)' },
]

const getTasksByStatus   = (status) => tasks.value.filter(t => t.Status === status)
const getColByStatus     = (status) => columns.find(c => c.status === status)

const isOverdue = (task) => {
  if (!task.Deadline || task.Status === 'Done' || task.Status === 'Closed') return false
  return new Date(task.Deadline) < new Date()
}

const openModal = (task) => {
  modal.task = task
  modal.open = true
  editMode.value = false
  document.body.style.overflow = 'hidden'
}

const closeModal = () => {
  modal.open = false
  editMode.value = false
  document.body.style.overflow = ''
  selectedAssigneeId.value = null
  assignPerms.canView = true
  assignPerms.canEdit = false
  editingPermUserId.value = null
}

const toDatetimeLocal = (iso) => {
  if (!iso) return ''
  return iso.slice(0, 16)
}

const toggleEdit = () => {
  if (!editMode.value) {
    editForm.title       = modal.task?.Title || ''
    editForm.description = modal.task?.Description || ''
    editForm.deadline    = toDatetimeLocal(modal.task?.Deadline)
    editForm.status      = modal.task?.Status || 'ToDo'
  }
  editMode.value = !editMode.value
}

const cancelEdit = () => { editMode.value = false }

const saveEdit = async () => {
  saving.value = true
  try {
    const payload = {
      taskId:      modal.task.Id,
      title:       editForm.title,
      description: editForm.description,
      Deadline:    editForm.deadline ? new Date(editForm.deadline).toISOString() : null,
      Status:      editForm.status,
    }
    await updateTask(payload)
    await loadData()
    const updated = tasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
    editMode.value = false
    toastSuccess('Task updated successfully!')
  } catch (err) {
    console.error(err)
    toastError(extractMessage(err, 'Failed to update task.'))
  } finally {
    saving.value = false
  }
}

const handleDeleteTask = async () => {
  const ok = await confirm(
    'Delete task?',
    `Are you sure you want to delete this task? This action cannot be undone.`,
    'Delete Task'
  )
  if (!ok) return
  
  saving.value = true
  try {
    await deleteTask(modal.task.Id)
    toastSuccess('Task deleted successfully!')
    closeModal()
    await loadData()
  } catch (err) {
    console.error(err)
    toastError(extractMessage(err, 'Failed to delete task.'))
  } finally {
    saving.value = false
  }
}

// Members computed list (members not assigned to the active task)
const projectMembersNotAssigned = computed(() => {
  if (!members.value || !modal.task) return []
  const assignedIds = modal.task.AssignedUsers?.map(au => au.UserId) || []
  return members.value.filter(m => !assignedIds.includes(m.UserId))
})

const loadData = async () => {
  if (!projectStore.currentProjectId) {
    tasks.value = []
    return
  }
  loading.value = true
  try {
    const res = await getProjectTasks(projectStore.currentProjectId)
    tasks.value = res?.data || []
  } catch (e) {
    console.error(e)
    toastError(extractMessage(e, 'Failed to load task list.'))
  } finally {
    loading.value = false
  }
}

const loadMembers = async () => {
  if (!projectStore.currentProjectId) {
    members.value = []
    return
  }
  loadingMembers.value = true
  try {
    const res = await getMembers(projectStore.currentProjectId)
    members.value = res?.data || []
  } catch (err) {
    console.error(err)
  } finally {
    loadingMembers.value = false
  }
}

const refreshAll = async () => {
  await Promise.all([loadData(), loadMembers()])
}

// Note: Project actions & member management are handled in SettingsView and MembersView respectively.

// Task assignment Actions
const assignUser = async () => {
  if (!selectedAssigneeId.value) return
  try {
    await assignTask({
      taskId:  modal.task.Id,
      userId:  selectedAssigneeId.value,
      canView: assignPerms.canView,
      canEdit: assignPerms.canEdit,
    })
    selectedAssigneeId.value = null
    assignPerms.canView = true
    assignPerms.canEdit = false
    await loadData()
    const updated = tasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
    toastSuccess('Task assigned successfully!')
  } catch (err) {
    console.error(err)
    toastError(extractMessage(err, 'Failed to assign task.'))
  }
}

const startEditPerm = (user) => {
  editingPermUserId.value = user.UserId
  editingPerms.canView = user.CanView
  editingPerms.canEdit = user.CanEdit
}

const cancelEditPerm = () => {
  editingPermUserId.value = null
}

const savePermission = async (user) => {
  try {
    await updatePermission({
      taskId: modal.task.Id,
      userId: user.UserId,
      canView: editingPerms.canView,
      canEdit: editingPerms.canEdit,
    })
    editingPermUserId.value = null
    await loadData()
    const updated = tasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
    toastSuccess('Permissions updated successfully!')
  } catch (err) {
    console.error(err)
    toastError(extractMessage(err, 'Failed to update permissions.'))
  }
}

const removeUser = async (user) => {
  const ok = await confirm(
    'Revoke assignment?',
    `Are you sure you want to revoke the assignment for <strong>${user.Email}</strong>?`,
    'Revoke'
  )
  if (!ok) return
  try {
    await removeAssignment({ taskId: modal.task.Id, userId: user.UserId })
    await loadData()
    const updated = tasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
    toastSuccess('Assignment revoked successfully!')
  } catch (err) {
    console.error(err)
    toastError(extractMessage(err, 'Failed to revoke assignment.'))
  }
}

// Drag & drop (disabled for Viewers)
const onDragStart = (task) => {
  if (projectStore.userRole === 'Viewer') return
  draggingTask.value = task
  draggingFromStatus.value = task.Status
}

const onDragEnd = () => {
  draggingTask.value = null
  draggingFromStatus.value = null
  dragOverCol.value = null
}

const onDragOver = (e, status) => {
  if (projectStore.userRole === 'Viewer') return
  e.preventDefault()
  dragOverCol.value = status
}

const onDragLeave = () => {
  dragOverCol.value = null
}

const onDrop = async (e, targetStatus) => {
  if (projectStore.userRole === 'Viewer') return
  e.preventDefault()
  dragOverCol.value = null
  const task = draggingTask.value
  if (!task || task.Status === targetStatus) return

  // Optimistic update
  const oldStatus = task.Status
  task.Status = targetStatus

  try {
    await updateStatusTask({ taskId: task.Id, status: targetStatus })
    toastSuccess('Status updated!')
  } catch (err) {
    task.Status = oldStatus
    console.error('Failed to update status, rolled back:', err)
    toastError(extractMessage(err, 'Failed to change status.'))
  }
}

// Watch active project changes
watch(() => projectStore.currentProjectId, () => {
  refreshAll()
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

const onKeydown = (e) => { if (e.key === 'Escape') closeModal() }
const onTaskCreated = () => loadData()

onMounted(() => {
  window.addEventListener('keydown', onKeydown)
  window.addEventListener('task-created', onTaskCreated)
  refreshAll()
})

onUnmounted(() => {
  window.removeEventListener('keydown', onKeydown)
  window.removeEventListener('task-created', onTaskCreated)
})

const formatDate      = (d) => d ? new Date(d).toLocaleString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—'
const formatDateShort = (d) => d ? new Date(d).toLocaleDateString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'
const userInitial     = (email) => email ? email[0].toUpperCase() : '?'
</script>

<style scoped>
.page-title {
  font-size: 1.68rem;
  letter-spacing: -0.02em;
  color: #0f172a;
}
.task-tag-card {
  transition: transform 0.15s, box-shadow 0.15s;
  cursor: pointer;
  border-top-width: 4px !important;
}
.task-tag-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(15,23,42,0.08) !important;
}
.task-tag-card--dragging {
  opacity: 0.45;
  transform: scale(0.97);
}
.kanban-col {
  min-height: 550px;
  transition: background-color 0.2s;
}
.kanban-col--dragover { 
  outline: 2px dashed #6366f1; 
  outline-offset: -4px;
  background: #eef0fe !important;
}
.skeleton-card {
  background: linear-gradient(90deg, var(--bs-border-color) 25%, var(--bs-secondary-bg) 50%, var(--bs-border-color) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}
@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
.modal-backdrop {
  z-index: 1040;
}
.modal {
  z-index: 1050;
}
</style>