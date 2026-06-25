<template>
  <div class="p-3 p-md-4">

    <!-- Page header -->
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
      <div>
        <h2 class="fw-bold mb-0 page-title">Task Board</h2>
        <p class="text-muted small mb-0 mt-1">Click any task to view details</p>
      </div>
      <div class="d-flex align-items-center gap-2">
        <div class="tab-toggle d-flex">
          <button :class="['tab-btn', activeTab === 'created' && 'tab-btn--active']" @click="activeTab = 'created'">
            My Tasks <span class="tab-count">{{ createdTasks.length }}</span>
          </button>
          <button :class="['tab-btn', activeTab === 'assigned' && 'tab-btn--active']" @click="activeTab = 'assigned'">
            Assigned <span class="tab-count">{{ assignedTasks.length }}</span>
          </button>
        </div>
        <button class="btn btn-sm btn-outline-secondary ref-btn" @click="loadData" :disabled="loading" title="Refresh">
          <svg v-if="!loading" xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          <span v-else class="tm-spinner"></span>
        </button>
      </div>
    </div>

    <!-- Kanban board -->
    <div class="kanban-board">
      <div v-for="col in columns"
          :key="col.status"
          class="kanban-col"
          :class="{ 'kanban-col--dragover': dragOverCol === col.status }"
          @dragover="onDragOver($event, col.status)"
          @dragleave="onDragLeave"
          @drop="onDrop($event, col.status)"
        >

        <div class="kanban-col-header d-flex align-items-center gap-2 mb-2">
          <span class="col-dot" :style="{ background: col.color }"></span>
          <span class="col-label">{{ col.label }}</span>
          <span class="col-badge ms-auto" :style="{ background: col.bgLight, color: col.color }">
            {{ getTasksByStatus(col.status).length }}
          </span>
        </div>

        <div v-if="loading" class="d-flex flex-column gap-2">
          <div v-for="n in 3" :key="n" class="skeleton-card"></div>
        </div>

        <div v-else-if="getTasksByStatus(col.status).length === 0" class="col-empty">
          <i class="bi bi-inbox text-muted" style="font-size:20px"></i>
          <div class="text-muted small mt-1">No tasks</div>
        </div>

        <div v-else class="d-flex flex-column gap-2">
          <button
            v-for="task in getTasksByStatus(col.status)"
            :key="task.Id"
            class="task-tag text-start"
            :class="{ 'task-tag--dragging': draggingTask?.Id === task.Id }"
            :style="{ '--col-color': col.color }"
            :draggable="true"
            @dragstart="onDragStart(task)"
            @dragend="onDragEnd"
            @click="openModal(task)"
          >
            <span class="task-title d-block mb-2">{{ task.Title }}</span>
            <div class="d-flex flex-column gap-1">
              <span class="task-meta">
                <i class="bi bi-calendar3"></i>
                {{ formatDateShort(task.CreatedAt) }}
              </span>
              <span v-if="task.Deadline" class="task-meta" :class="isOverdue(task) && 'task-meta--overdue'">
                <i :class="isOverdue(task) ? 'bi bi-exclamation-circle-fill' : 'bi bi-clock'"></i>
                {{ formatDateShort(task.Deadline) }}
                <span v-if="isOverdue(task)" class="overdue-chip">Overdue</span>
              </span>
            </div>
          </button>
        </div>

      </div>
    </div>

    <!-- ── Modal ── -->
    <Teleport to="body">
      <Transition name="tm-fade">
        <div v-if="modal.open" class="modal-overlay d-flex align-items-center justify-content-center p-3" >
          <Transition name="tm-slide">
            <div v-if="modal.open" class="task-modal" role="dialog" aria-modal="true">

              <!-- Modal header -->
              <div class="task-modal-header" :style="{ background: getColByStatus(modal.task?.Status)?.bgLight, borderBottomColor: getColByStatus(modal.task?.Status)?.borderColor }">
                <div class="flex-grow-1">
                  <div class="d-flex gap-2 flex-wrap mb-2">
                    <span class="modal-badge" :style="{ background: getColByStatus(modal.task?.Status)?.bgMid, color: getColByStatus(modal.task?.Status)?.color }">
                      {{ getColByStatus(modal.task?.Status)?.label }}
                    </span>
                    <span class="modal-badge" :style="{ background: activeTab === 'created' ? '#EEF2FF' : '#ECFDF5', color: activeTab === 'created' ? '#4F46E5' : '#059669' }">
                      {{ activeTab === 'created' ? 'Creator' : 'Assigned' }}
                    </span>
                    <span v-if="modal.task && isOverdue(modal.task)" class="modal-badge" style="background:#FEE2E2;color:#DC2626">
                      Overdue
                    </span>
                  </div>
                  <h5 class="fw-bold mb-0 text-dark">{{ modal.task?.Title }}</h5>
                </div>
                <div class="d-flex gap-1">
                  <!-- Edit toggle — only for created tasks -->
                  <button v-if="activeTab === 'created'" class="btn-icon-action" :class="editMode && 'btn-icon-action--active'" @click="toggleEdit" title="Edit task">
                    <i class="bi bi-pencil-fill"></i>
                  </button>
                  <button class="btn-icon-close" @click="closeModal">
                    <i class="bi bi-x-lg"></i>
                  </button>
                </div>
              </div>

              <!-- ── VIEW MODE ── -->
              <div v-if="!editMode" class="task-modal-body">

                <div class="modal-field">
                  <div class="modal-label">Description</div>
                  <div class="modal-value">{{ modal.task?.Description || 'No description.' }}</div>
                </div>

                <div class="row g-3">
                  <div class="col-6">
                    <div class="modal-field">
                      <div class="modal-label">Created at</div>
                      <div class="modal-value">{{ formatDate(modal.task?.CreatedAt) }}</div>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="modal-field">
                      <div class="modal-label">Deadline</div>
                      <div class="modal-value d-flex align-items-center gap-2 flex-wrap" :class="modal.task && isOverdue(modal.task) ? 'text-danger' : ''">
                        {{ modal.task?.Deadline ? formatDate(modal.task.Deadline) : '—' }}
                        <span v-if="modal.task && isOverdue(modal.task)" class="overdue-chip">Overdue</span>
                      </div>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="modal-field">
                      <div class="modal-label">Task ID</div>
                      <div class="modal-value">#{{ modal.task?.Id }}</div>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="modal-field">
                      <div class="modal-label">Status</div>
                      <div class="modal-value">{{ getColByStatus(modal.task?.Status)?.label }}</div>
                    </div>
                  </div>
                </div>

                <div class="modal-field">
                  <div class="modal-label">Creator ID</div>
                  <div class="modal-value mono-value">{{ modal.task?.CreatorId }}</div>
                </div>

                <div v-if="activeTab === 'created' && modal.task?.AssignedUsers" class="modal-field">
                  <div class="modal-label d-flex align-items-center gap-2">
                    Assigned Users
                    <span class="badge bg-light text-muted border">{{ modal.task.AssignedUsers.length }}</span>
                  </div>
                  <div v-if="modal.task.AssignedUsers.length === 0" class="text-muted small fst-italic mt-1">No users assigned.</div>
                  <div v-else class="d-flex flex-column gap-2 mt-1">
                    <div
                      v-for="user in modal.task.AssignedUsers"
                      :key="user.UserId"
                      class="user-row d-flex flex-column gap-2 p-2 rounded-3 border bg-light"
                    >
                      <!-- Row trên: avatar + email + action buttons -->
                      <div class="d-flex align-items-center gap-2">
                        <div class="user-avatar">{{ userInitial(user.Email) }}</div>
                        <div class="flex-grow-1 min-w-0">
                          <div class="small fw-semibold text-truncate">{{ user.Email }}</div>
                          <div class="user-id text-truncate">{{ user.UserId }}</div>
                        </div>

                        <!-- View mode: badges + edit + delete -->
                        <template v-if="editingPermUserId !== user.UserId">
                          <span v-if="user.CanView" class="perm-badge perm-badge--view"><i class="bi bi-eye-fill"></i> View</span>
                          <span v-if="user.CanEdit" class="perm-badge perm-badge--edit"><i class="bi bi-pencil-fill"></i> Edit</span>
                          <button class="btn-icon-action ms-1" @click="startEditPerm(user)" title="Edit permissions">
                            <i class="bi bi-pencil-fill" style="font-size:11px"></i>
                          </button>
                          <button class="btn-icon-action text-danger-action ms-1" @click="removeUser(user)" title="Remove assignment">
                            <i class="bi bi-trash3-fill" style="font-size:11px"></i>
                          </button>
                        </template>

                        <!-- Edit mode: cancel button -->
                        <template v-else>
                          <button class="btn-icon-action ms-auto" @click="cancelEditPerm" title="Cancel">
                            <i class="bi bi-x-lg" style="font-size:12px"></i>
                          </button>
                        </template>
                      </div>

                      <!-- Row dưới: inline edit quyền (chỉ hiện khi đang edit user đó) -->
                      <div v-if="editingPermUserId === user.UserId" class="d-flex align-items-center gap-3 pt-1 border-top">
                        <label class="perm-check">
                          <input type="checkbox" v-model="editingPerms.canView" />
                          <span>View</span>
                        </label>
                        <label class="perm-check">
                          <input type="checkbox" v-model="editingPerms.canEdit" />
                          <span>Edit</span>
                        </label>
                        <button
                          class="btn btn-sm btn-primary ms-auto px-3"
                          @click="savePermission(user)"
                          :disabled="!editingPerms.canView && !editingPerms.canEdit"
                        >
                          <i class="bi bi-check2 me-1"></i> Save
                        </button>
                      </div>
                    </div>
                  </div>
                </div>

              </div>

              <!-- ── EDIT MODE ── -->
              <div v-else class="task-modal-body">

                <div class="edit-notice d-flex align-items-center gap-2">
                  <i class="bi bi-info-circle-fill" style="color:#6366f1;font-size:13px"></i>
                  <span>Editing task <strong>#{{ modal.task?.Id }}</strong></span>
                </div>

                <div class="modal-field">
                  <label class="modal-label" for="edit-title">Title</label>
                  <input id="edit-title" v-model="editForm.title" type="text" class="form-control form-control-sm" placeholder="Task title" />
                </div>

                <div class="modal-field">
                  <label class="modal-label" for="edit-desc">Description</label>
                  <textarea id="edit-desc" v-model="editForm.description" class="form-control form-control-sm" rows="3" placeholder="Task description"></textarea>
                </div>

                <div class="row g-3">
                  <div class="col-6">
                    <div class="modal-field">
                      <label class="modal-label" for="edit-deadline">Deadline</label>
                      <input id="edit-deadline" v-model="editForm.deadline" type="datetime-local" class="form-control form-control-sm" />
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="modal-field">
                      <label class="modal-label" for="edit-status">Status</label>
                      <select id="edit-status" v-model="editForm.status" class="form-select form-select-sm">
                        <option v-for="col in columns" :key="col.status" :value="col.status">{{ col.label }}</option>
                      </select>
                    </div>
                  </div>
                </div>

              </div>

              
                <div v-if="!editMode && activeTab === 'created'" class="task-modal-footer d-flex align-items-center gap-2 flex-wrap">
                  <div class="search-box flex-grow-1 position-relative">
                    <input
                      v-model="searchKeyword"
                      type="text"
                      class="form-control form-control-sm"
                      placeholder="Search email to assign..."
                      @input="handleSearchUser"
                    />
                    <div v-if="searchResults.length" class="search-dropdown">
                      <div v-for="user in searchResults" :key="user.UserId" class="search-item" @click="selectUser(user)">
                        <i class="bi bi-person text-muted me-2"></i>{{ user.Email }}
                      </div>
                    </div>
                  </div>

                  <!-- Permission checkboxes — hiện ra khi đã chọn user -->
                  <div v-if="selectedUser" class="perm-selector d-flex align-items-center gap-3">
                    <label class="perm-check">
                      <input type="checkbox" v-model="assignPerms.canView" />
                      <span>View</span>
                    </label>
                    <label class="perm-check">
                      <input type="checkbox" v-model="assignPerms.canEdit" />
                      <span>Edit</span>
                    </label>
                  </div>

                  <button class="btn btn-sm btn-primary px-3" @click="assignUser" :disabled="!selectedUser || (!assignPerms.canView && !assignPerms.canEdit)">
                    <i class="bi bi-person-plus-fill me-1"></i> Assign
                  </button>
                  <button class="btn btn-sm btn-outline-secondary px-3" @click="closeModal">Close</button>
                </div>

              <!-- ── FOOTER — View mode, Assigned ── -->
              <div v-else-if="!editMode && activeTab === 'assigned'" class="task-modal-footer d-flex justify-content-end">
                <button class="btn btn-sm btn-outline-secondary px-4" @click="closeModal">Close</button>
              </div>

              <!-- ── FOOTER — Edit mode ── -->
              <div v-else-if="editMode" class="task-modal-footer d-flex justify-content-end gap-2">
                <button class="btn btn-sm btn-outline-secondary px-3" @click="cancelEdit">
                  <i class="bi bi-x me-1"></i> Cancel
                </button>
                <button class="btn btn-sm btn-primary px-3" @click="saveEdit" :disabled="saving">
                  <span v-if="saving" class="tm-spinner me-1"></span>
                  <i v-else class="bi bi-check2 me-1"></i>
                  {{ saving ? 'Saving…' : 'Save changes' }}
                </button>
              </div>

            </div>
          </Transition>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { getAssignedTasks, getCreatedTasks, assignTask, updateTask, updatePermission, removeAssignment, updateStatusTask  } from '../Services/taskService.js'
import { searchUsers } from '../Services/authService.js'

const assignedTasks = ref([])
const createdTasks  = ref([])
const loading       = ref(false)
const saving        = ref(false)
const activeTab     = ref('created')
const modal         = reactive({ open: false, task: null })
const editMode      = ref(false)
const editForm      = reactive({ title: '', description: '', deadline: '', status: '' })
const searchKeyword = ref('')
const searchResults = ref([])
const selectedUser  = ref(null)
const assignPerms = reactive({ canView: true, canEdit: false })
const editingPermUserId = ref(null)
const editingPerms = reactive({ canView: false, canEdit: false })
const draggingTask = ref(null)
const draggingFromStatus = ref(null)
const dragOverCol = ref(null)

const columns = [
  { status: 'ToDo',       label: 'To Do',       color: '#64748B', bgLight: '#F8FAFC', bgMid: '#E2E8F0', borderColor: '#CBD5E1' },
  { status: 'InProgress', label: 'In Progress',  color: '#2563EB', bgLight: '#EFF6FF', bgMid: '#DBEAFE', borderColor: '#BFDBFE' },
  { status: 'Done',       label: 'Done',         color: '#059669', bgLight: '#ECFDF5', bgMid: '#D1FAE5', borderColor: '#A7F3D0' },
  { status: 'Closed',     label: 'Closed',       color: '#EA580C', bgLight: '#FFF7ED', bgMid: '#FED7AA', borderColor: '#FDBA74' },
]

const currentTasks      = computed(() => activeTab.value === 'created' ? createdTasks.value : assignedTasks.value)
const getTasksByStatus   = (status) => currentTasks.value.filter(t => t.Status === status)
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
  searchKeyword.value = ''
  searchResults.value = []
  selectedUser.value = null
  assignPerms.canView = true
  assignPerms.canEdit = false
}

// Convert ISO string → datetime-local format (YYYY-MM-DDTHH:mm)
const toDatetimeLocal = (iso) => {
  if (!iso) return ''
  return iso.slice(0, 16)
}

const toggleEdit = () => {
  if (!editMode.value) {
    // populate form from current task
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
    await updateTask( payload)
    await loadData()
    const updated = createdTasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
    editMode.value = false
  } catch (err) {
    console.error(err)
  } finally {
    saving.value = false
  }
}

const onKeydown = (e) => { if (e.key === 'Escape') closeModal() }
onMounted(() => { window.addEventListener('keydown', onKeydown); loadData() })
onUnmounted(() => { window.removeEventListener('keydown', onKeydown) })

const loadData = async () => {
  loading.value = true
  try {
    const [createdRes, assignedRes] = await Promise.all([getCreatedTasks(), getAssignedTasks()])
    createdTasks.value  = createdRes.data
    assignedTasks.value = assignedRes.data
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}

const handleSearchUser = async () => {
  selectedUser.value = null
  if (searchKeyword.value.length < 2) { searchResults.value = []; return }
  try { const res = await searchUsers(searchKeyword.value); searchResults.value = res.data }
  catch (err) { console.error(err) }
}

const selectUser = (user) => {
  selectedUser.value  = user
  searchKeyword.value = user.Email
  searchResults.value = []
  assignPerms.canView = true   
  assignPerms.canEdit = false
}

const assignUser = async () => {
  if (!selectedUser.value) return
  try {
    await assignTask({
      taskId:  modal.task.Id,
      userId:  selectedUser.value.UserId,
      canView: assignPerms.canView,
      canEdit: assignPerms.canEdit,
    })
    searchKeyword.value = ''; searchResults.value = []; selectedUser.value = null
    assignPerms.canView = true; assignPerms.canEdit = false
    await loadData()
    const updated = createdTasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
  } catch (err) { console.error(err) }
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
    const updated = createdTasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
  } catch (err) { console.error(err) }
}

const removeUser = async (user) => {
  try {
    await removeAssignment({ taskId: modal.task.Id, userId: user.UserId })
    await loadData()
    const updated = createdTasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
  } catch (err) { console.error(err) }
}

const onDragStart = (task) => {
  draggingTask.value = task
  draggingFromStatus.value = task.Status
}

const onDragEnd = () => {
  draggingTask.value = null
  draggingFromStatus.value = null
  dragOverCol.value = null
}

const onDragOver = (e, status) => {
  e.preventDefault()
  dragOverCol.value = status
}

const onDragLeave = () => {
  dragOverCol.value = null
}

const onDrop = async (e, targetStatus) => {
  e.preventDefault()
  dragOverCol.value = null
  const task = draggingTask.value
  if (!task || task.Status === targetStatus) return

  // Optimistic update
  const oldStatus = task.Status
  task.Status = targetStatus

  try {
    await updateStatusTask({ taskId: task.Id, status: targetStatus })
  } catch (err) {
    // Rollback
    task.Status = oldStatus
    console.error('Failed to update status, rolled back:', err)
  }
}

const formatDate      = (d) => d ? new Date(d).toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—'
const formatDateShort = (d) => d ? new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'
const userInitial     = (email) => email ? email[0].toUpperCase() : '?'
</script>

<style scoped>
.page-title { font-size: 1.68rem; letter-spacing: -0.02em; color: #0f172a; }

/* Tab toggle */
.tab-toggle { background: #f1f5f9; border-radius: 12px; padding: 4px; border: 1px solid #e2e8f0; }
.tab-btn { padding: 7px 20px; border: none; background: transparent; color: #64748b; font-size: 0.9rem; font-weight: 500; cursor: pointer; border-radius: 9px; display: flex; align-items: center; gap: 7px; transition: background 0.15s, color 0.15s; white-space: nowrap; }
.tab-btn--active { background: #fff; color: #0f172a; box-shadow: 0 1px 3px rgba(0,0,0,.1); }
.tab-count { font-size: 0.75rem; font-weight: 600; padding: 1px 6px; border-radius: 999px; background: #e2e8f0; color: #64748b; }
.tab-btn--active .tab-count { background: #6366f1; color: #fff; }

/* Refresh */
.ref-btn { width: 44px; height: 44px; padding: 0; display: flex; align-items: center; justify-content: center; border-radius: 10px; border-color: #e2e8f0; color: #64748b; }
.ref-btn:hover:not(:disabled) { background: #f1f5f9; color: #4f46e5; border-color: #c7d2fe; }
.tm-spinner { width: 16px; height: 16px; border: 2.5px solid #cbd5e1; border-top-color: #6366f1; border-radius: 50%; animation: spin 0.7s linear infinite; }

/* Board */
.kanban-board { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; align-items: start; }
@media (max-width: 900px) { .kanban-board { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 560px)  { .kanban-board { grid-template-columns: 1fr; } }

/* Column */
.kanban-col { background: #eef2f7; border-radius: 16px; padding: 14px; }
.kanban-col-header { padding-bottom: 12px; border-bottom: 1px solid #e2e8f0; }
.col-dot  { width: 11px; height: 11px; border-radius: 50%; flex-shrink: 0; }
.col-label { font-size: 0.85rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #475569; }
.col-badge { font-size: 0.78rem; font-weight: 700; padding: 2px 10px; border-radius: 999px; }

/* Skeleton */
.skeleton-card { height: 98px; border-radius: 10px; background: linear-gradient(90deg, #e2e8f0 25%, #cbd5e1 50%, #e2e8f0 75%); background-size: 200% 100%; animation: shimmer 1.3s infinite; }

/* Empty */
.col-empty { padding: 40px 16px; text-align: center; background: #fff; border-radius: 10px; border: 1.5px dashed #cbd5e1; }

/* Task tag */
.task-tag { width: 100%; background: #fff; border: 1px solid #e2e8f0; border-top: 4px solid var(--col-color); border-radius: 10px; padding: 14px 15px; cursor: pointer; transition: box-shadow 0.12s, transform 0.1s; }
.task-tag:hover { box-shadow: 0 6px 20px rgba(0,0,0,.1); transform: translateY(-2px); }
.task-title { font-size: 1.05rem; font-weight: 600; color: #1e293b; line-height: 1.35; }
.task-meta { display: flex; align-items: center; gap: 6px; font-size: 0.82rem; color: #94a3b8; }
.task-meta i { font-size: 13px; flex-shrink: 0; }
.task-meta--overdue { color: #ef4444; font-weight: 600; }
.overdue-chip { font-size: 0.7rem; font-weight: 700; background: #fee2e2; color: #dc2626; padding: 2px 7px; border-radius: 999px; text-transform: uppercase; letter-spacing: 0.04em; }

/* Modal */
.modal-overlay { position: fixed; inset: 0; z-index: 1050; background: rgba(15,23,42,.5); backdrop-filter: blur(3px); }

.task-modal { background: #fff; border-radius: 18px; width: 100%; max-width: 680px; box-shadow: 0 25px 70px rgba(0,0,0,.2); overflow: hidden; display: flex; flex-direction: column; max-height: 92vh; }

.task-modal-header { display: flex; align-items: flex-start; gap: 14px; padding: 24px 28px 22px; border-bottom: 1px solid; }
.modal-badge { font-size: 0.75rem; font-weight: 700; letter-spacing: 0.07em; text-transform: uppercase; padding: 3px 11px; border-radius: 999px; }

/* Header buttons */
.btn-icon-close,
.btn-icon-action { background: transparent; border: none; font-size: 16px; padding: 7px 9px; border-radius: 8px; cursor: pointer; line-height: 1; }
.btn-icon-close { color: #94a3b8; }
.btn-icon-close:hover { background: #f1f5f9; color: #0f172a; }
.btn-icon-action { color: #94a3b8; border: 1px solid transparent; }
.btn-icon-action:hover, .btn-icon-action--active { background: #eef2ff; color: #6366f1; border-color: #c7d2fe; }

/* Modal body */
.task-modal-body { padding: 24px 28px; overflow-y: auto; display: flex; flex-direction: column; gap: 18px; }
.modal-field { display: flex; flex-direction: column; gap: 4px; }
.modal-label { font-size: 0.78rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.07em; color: #94a3b8; margin-bottom: 3px; }
.modal-value { font-size: 1.05rem; color: #1e293b; line-height: 1.5; }
.mono-value { font-family: 'SF Mono', 'Fira Code', monospace; font-size: 0.85rem; color: #64748b; background: #f8fafc; padding: 8px 14px; border-radius: 7px; border: 1px solid #e2e8f0; word-break: break-all; }

/* Edit notice */
.edit-notice { background: #eef2ff; border: 1px solid #c7d2fe; border-radius: 10px; padding: 11px 16px; font-size: 0.9rem; color: #4338ca; }

/* Form */
.form-control, .form-select { font-size: 1.0rem; border-color: #e2e8f0; border-radius: 8px; color: #1e293b; padding: 10px 14px; }
.form-control:focus, .form-select:focus { border-color: #6366f1; box-shadow: 0 0 0 4px rgba(99,102,241,.12); }
textarea.form-control { resize: vertical; min-height: 90px; }

/* Users */
.user-avatar { width: 38px; height: 38px; border-radius: 50%; font-size: 0.9rem; }
.user-id { font-size: 0.75rem; }
.perm-badge { font-size: 0.72rem; padding: 3px 8px; gap: 4px; }

/* Footer */
.task-modal-footer { padding: 16px 28px; border-top: 1px solid #f1f5f9; }

/* Search */
.search-dropdown { max-height: 240px; }
.search-item { padding: 11px 16px; font-size: 1.0rem; }

/* Btn */
.btn-primary { padding: 10px 20px; font-size: 0.95rem; }

/* Transitions giữ nguyên */
.tm-fade-enter-active, .tm-fade-leave-active { transition: opacity 0.2s ease; }
.tm-fade-enter-from, .tm-fade-leave-to { opacity: 0; }
.tm-slide-enter-active { transition: opacity 0.2s ease, transform 0.22s cubic-bezier(.25,.8,.25,1); }
.tm-slide-leave-active { transition: opacity 0.15s ease, transform 0.15s ease; }
.tm-slide-enter-from { opacity: 0; transform: translateY(14px) scale(0.98); }
.tm-slide-leave-to { opacity: 0; transform: translateY(6px) scale(0.99); }
.perm-selector { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 6px 12px; }
.perm-check { display: flex; align-items: center; gap: 5px; font-size: 0.85rem; font-weight: 500; color: #475569; cursor: pointer; user-select: none; }
.perm-check input[type="checkbox"] { width: 14px; height: 14px; accent-color: #6366f1; cursor: pointer; }
.text-danger-action:hover { background: #fee2e2 !important; color: #dc2626 !important; border-color: #fca5a5 !important; } 

/* Drag & drop */
.task-tag[draggable="true"] { cursor: grab; }
.task-tag[draggable="true"]:active { cursor: grabbing; }
.task-tag--dragging { opacity: 0.45; transform: scale(0.97); box-shadow: none !important; }

.kanban-col--dragover { 
  outline: 2px dashed #6366f1; 
  outline-offset: -4px;
  background: #eef0fe;
}
</style>