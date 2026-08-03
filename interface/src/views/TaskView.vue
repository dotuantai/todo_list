<template>
  <div class="p-3 p-md-4">

    <!-- Page header & Toolbar -->
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4" v-if="projectStore.currentProjectId">
      <div class="d-flex flex-wrap align-items-center gap-3">
        <!-- Search -->
        <div class="input-group" style="width: 220px;">
          <span class="input-group-text bg-body border-end-0 text-muted"><i class="bi bi-search"></i></span>
          <input type="text" class="form-control border-start-0 ps-0" placeholder="Search tasks..." v-model="filters.search" @input="onSearchInput" />
        </div>

        <!-- Priority -->
        <div class="dropdown">
          <label class="form-label small text-muted mb-1 d-block" style="font-size: 11px; margin-top: -15px;">Priority</label>
          <button class="btn btn-outline-secondary dropdown-toggle bg-body d-flex align-items-center justify-content-between text-start" type="button" data-bs-toggle="dropdown" style="width: 140px; font-size: 0.85rem; height: 38px;">
            {{ filters.priority || 'All' }}
          </button>
          <ul class="dropdown-menu shadow-sm" style="font-size: 0.85rem; width: 140px;">
            <li><a class="dropdown-item" href="#" @click.prevent="setFilter('priority', null)">All</a></li>
            <li><a class="dropdown-item" href="#" @click.prevent="setFilter('priority', 'High')"><span class="badge bg-danger text-white">High</span></a></li>
            <li><a class="dropdown-item" href="#" @click.prevent="setFilter('priority', 'Medium')"><span class="badge bg-warning text-dark">Medium</span></a></li>
            <li><a class="dropdown-item" href="#" @click.prevent="setFilter('priority', 'Low')"><span class="badge bg-info text-white">Low</span></a></li>
          </ul>
        </div>

        <!-- Assignee -->
        <div class="dropdown">
          <label class="form-label small text-muted mb-1 d-block" style="font-size: 11px; margin-top: -15px;">Assignee</label>
          <button class="btn btn-outline-secondary dropdown-toggle bg-body d-flex align-items-center justify-content-between text-start" type="button" data-bs-toggle="dropdown" data-bs-auto-close="outside" style="width: 180px; font-size: 0.85rem; height: 38px;" @click="assigneeSearch = ''">
            <div class="d-flex align-items-center gap-2 text-truncate">
              <span v-if="!filters.assigneeId">All Assignees</span>
              <span v-else-if="filters.assigneeId === '00000000-0000-0000-0000-000000000000'">Unassigned</span>
              <template v-else>
                <div class="rounded-circle text-white d-flex align-items-center justify-content-center" :style="{ background: getUserColor(getAssigneeName(filters.assigneeId)) }" style="width: 18px; height: 18px; font-size: 9px; min-width: 18px;">
                  {{ getAssigneeName(filters.assigneeId)[0]?.toUpperCase() }}
                </div>
                <span class="text-truncate">{{ getAssigneeName(filters.assigneeId) }}</span>
              </template>
            </div>
          </button>
          <ul class="dropdown-menu shadow-sm p-2" style="width: 240px;">
            <li class="mb-2">
              <div class="input-group input-group-sm">
                <span class="input-group-text bg-body text-muted border-end-0"><i class="bi bi-search"></i></span>
                <input type="text" class="form-control border-start-0 ps-0" placeholder="Search members..." v-model="assigneeSearch" />
              </div>
            </li>
            <div style="max-height: 220px; overflow-y: auto;" class="custom-scrollbar">
              <li><a class="dropdown-item py-2 d-flex align-items-center gap-2 rounded-2" :class="{'active': filters.assigneeId === null}" href="#" @click.prevent="setFilter('assigneeId', null)">
                <div class="rounded-circle bg-secondary text-white d-flex align-items-center justify-content-center" style="width: 24px; height: 24px; font-size: 11px;"><i class="bi bi-people-fill"></i></div>
                <span style="font-size: 0.85rem;">All Assignees</span>
              </a></li>
              <li><a class="dropdown-item py-2 d-flex align-items-center gap-2 rounded-2" :class="{'active': filters.assigneeId === '00000000-0000-0000-0000-000000000000'}" href="#" @click.prevent="setFilter('assigneeId', '00000000-0000-0000-0000-000000000000')">
                <div class="rounded-circle border border-secondary text-secondary d-flex align-items-center justify-content-center bg-body" style="width: 24px; height: 24px; font-size: 11px;"><i class="bi bi-person-dash"></i></div>
                <span style="font-size: 0.85rem;">Unassigned</span>
              </a></li>
              <li><hr class="dropdown-divider"></li>
              <li v-for="user in filteredAssignees" :key="user.UserId">
                <a class="dropdown-item py-2 d-flex align-items-center gap-2 rounded-2" :class="{'active': filters.assigneeId === user.UserId}" href="#" @click.prevent="setFilter('assigneeId', user.UserId)">
                  <div class="rounded-circle text-white d-flex align-items-center justify-content-center" :style="{ background: getUserColor(user.Email) }" style="width: 24px; height: 24px; font-size: 10px; min-width: 24px;">
                    {{ user.Email[0]?.toUpperCase() }}
                  </div>
                  <span class="text-truncate" style="font-size: 0.85rem;" :title="user.Email">{{ user.Email }}</span>
                </a>
              </li>
            </div>
          </ul>
        </div>
        
        <!-- Clear filters -->
        <button v-if="hasActiveFilters" class="btn btn-link text-muted text-decoration-none small align-self-end ms-2" style="font-size: 0.85rem; padding: 0; margin-bottom: 8px;" @click="clearFilters">
          Clear filters
        </button>
      </div>
      
      <!-- Refresh Button -->
      <div class="align-self-end mb-1">
        <button class="btn btn-outline-secondary d-flex align-items-center justify-content-center" @click="refreshAll" :disabled="loading" title="Refresh" style="width: 38px; height: 38px; border-radius: 8px;">
          <i class="bi bi-arrow-clockwise" v-if="!loading"></i>
          <span v-else class="spinner-border spinner-border-sm text-secondary" role="status"></span>
        </button>
      </div>
    </div>

    <!-- Empty Project Selection State -->
    <div v-if="!projectStore.currentProjectId" class="text-center py-5 bg-body rounded-4 shadow-sm border border-dashed p-4">
      <i class="bi bi-folder2-open text-primary" style="font-size: 4rem;"></i>
      <h3 class="fw-bold text-body mt-3">{{ $t('tasks.welcome') }}</h3>
      <p class="text-muted mx-auto" style="max-width: 480px;">{{ $t('tasks.welcome_desc') }}</p>
    </div>

    <!-- Kanban Board -->
    <div v-else class="d-flex gap-3 text-start align-items-start" style="overflow-x: auto; padding-bottom: 1rem; min-height: 70vh;">
      <div v-for="col in columns"
          :key="col.Id"
          style="min-width: 290px; width: 290px;"
        >
        <div 
          class="card bg-body-tertiary border-0 shadow-sm rounded-3 p-3 kanban-col d-flex flex-column"
        >
          <!-- Column Header -->
          <div class="d-flex align-items-center gap-2 mb-3">
            <span class="col-dot rounded-circle" :style="{ background: col.color, width: '10px', height: '10px', display: 'inline-block' }"></span>
            <span class="fw-bold text-uppercase text-secondary" style="font-size: 0.8rem; letter-spacing: 0.05em;">{{ col.Name }}</span>
            <span class="badge rounded-pill ms-auto" :style="{ background: col.bgMid, color: col.color }">
              {{ getTasksByColumnId(col.Id).length }}
            </span>
          </div>

          <!-- Column Cards List -->
          <div v-if="loading" class="d-flex flex-column gap-2">
            <div v-for="n in 3" :key="n" class="skeleton-card bg-body rounded-3 shadow-sm w-100" style="height: 100px;"></div>
          </div>

          <div v-else class="h-100 flex-grow-1">
            <draggable
              :list="getTasksByColumnId(col.Id)"
              item-key="Id"
              group="tasks"
              class="d-flex flex-column gap-2"
              style="min-height: 150px;"
              ghost-class="task-tag-card--ghost"
              drag-class="task-tag-card--dragging"
              :animation="200"
              @change="onChange($event, col.Id)"
            >
              <template #item="{ element: task }">
                <div
                  class="card border-0 border-top border-4 shadow-sm task-tag-card p-3"
                  :style="{ borderTopColor: col.color, cursor: 'grab' }"
                  @click="openModal(task)"
                >
                  <span class="fw-bold text-body mb-2 text-start d-block" style="font-size: 0.95rem; line-height: 1.4;">{{ task.Title }}</span>
                  <div class="d-flex flex-column gap-1 align-items-start">
                    <span v-if="task.Priority" class="badge mb-1" :class="getPriorityBadgeClass(task.Priority)" style="font-size: 0.65rem;">{{ task.Priority }} Priority</span>
                    <span class="text-muted small d-flex align-items-center gap-1.5" style="font-size: 0.75rem;">
                      <i class="bi bi-calendar3"></i>
                      {{ formatDateShort(task.CreatedAt) }}
                    </span>
                    <span v-if="task.Deadline" class="small d-flex align-items-center gap-1.5" :class="isOverdue(task) ? 'text-danger fw-bold' : 'text-muted'" style="font-size: 0.75rem;">
                      <i :class="isOverdue(task) ? 'bi bi-exclamation-circle-fill' : 'bi bi-clock'"></i>
                      {{ formatDateShort(task.Deadline) }}
                      <span v-if="isOverdue(task)" class="badge bg-danger bg-opacity-10 text-danger rounded-pill px-2 py-0.5 ms-1" style="font-size: 0.65rem;">{{ $t('tasks.overdue') }}</span>
                    </span>
                  </div>
                </div>
              </template>
              
              <template #footer>
                <div v-if="getTasksByColumnId(col.Id).length === 0" class="text-center py-4 border border-dashed rounded-3 bg-body text-muted mt-2">
                  <i class="bi bi-inbox d-block mb-1 fs-4 text-secondary opacity-50"></i>
                  <span class="small" style="font-size: 0.85rem;">{{ $t('tasks.no_tasks_col') }}</span>
                </div>
                
                <!-- Column Load More -->
                <div v-if="columnStates[col.Id]?.hasMore" class="mt-2 text-center pb-2">
                  <button class="btn btn-sm w-100 fw-semibold btn-outline-secondary" style="border-radius: 6px;" @click="loadMore(col.Id)" :disabled="columnStates[col.Id]?.loading">
                    <span v-if="columnStates[col.Id]?.loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                    {{ $t('tasks.load_more', 'Load More') }}
                  </button>
                </div>
              </template>
            </draggable>
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
            <div class="modal-header border-bottom p-4" :style="{ background: getColById(modal.task?.ColumnId)?.bgLight }">
              <div class="text-start flex-grow-1">
                <div class="d-flex gap-2 flex-wrap mb-2">
                  <span class="badge text-uppercase font-monospace" :style="{ background: getColById(modal.task?.ColumnId)?.bgMid, color: getColById(modal.task?.ColumnId)?.color }">
                    {{ getColById(modal.task?.ColumnId)?.Name }}
                  </span>
                  <span v-if="modal.task?.Priority" class="badge" :class="getPriorityBadgeClass(modal.task?.Priority)">
                    {{ modal.task?.Priority }} Priority
                  </span>
                  <span v-if="modal.task && isOverdue(modal.task)" class="badge bg-danger bg-opacity-10 text-danger rounded-pill">
                    {{ $t('tasks.overdue') }}
                  </span>
                </div>
                <h5 class="modal-title fw-bold text-body h5 mb-0 text-start">{{ modal.task?.Title }}</h5>
              </div>
              <div class="d-flex gap-1 align-items-center">
                <button v-if="projectStore.userRole === 'Owner' || projectStore.userRole === 'Manager'" class="btn btn-sm btn-light border p-2" :class="{ 'btn-primary text-white': editMode }" @click="toggleEdit" title="Edit task" style="border-radius: 8px; width: 34px; height: 34px; display: flex; align-items: center; justify-content: center;">
                  <i class="bi bi-pencil-fill"></i>
                </button>
                <button class="btn-close ms-2" @click="closeModal" aria-label="Close"></button>
              </div>
            </div>

            <!-- ── VIEW MODE ── -->
            <div v-if="!editMode" class="modal-body p-4 text-start">
              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('tasks.description') }}</label>
                <div class="text-body bg-body-secondary p-3 rounded-3" style="white-space: pre-wrap; font-size: 0.95rem; line-height: 1.6;">
                  {{ modal.task?.Description || $t('tasks.no_description') }}
                </div>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-6 col-md-4">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('tasks.created_at') }}</label>
                  <div class="text-body fw-medium">{{ formatDate(modal.task?.CreatedAt) }}</div>
                </div>
                <div class="col-6 col-md-4">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">{{ $t('tasks.deadline') }}</label>
                  <div class="text-body fw-medium" :class="modal.task && isOverdue(modal.task) ? 'text-danger fw-bold' : ''">
                    {{ modal.task?.Deadline ? formatDate(modal.task.Deadline) : '—' }}
                  </div>
                </div>
                <div class="col-6 col-md-4">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Start Date</label>
                  <div class="text-body fw-medium">
                    {{ modal.task?.StartDate ? formatDate(modal.task.StartDate) : '—' }}
                  </div>
                </div>
                <div class="col-6 col-md-4">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Est. Hours</label>
                  <div class="text-body fw-medium">
                    {{ modal.task?.EstimatedHours != null ? modal.task.EstimatedHours + 'h' : '—' }}
                  </div>
                </div>
                <div class="col-6 col-md-4">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Act. Hours</label>
                  <div class="text-body fw-medium">
                    {{ modal.task?.ActualHours != null ? modal.task.ActualHours + 'h' : '—' }}
                  </div>
                </div>
                <div class="col-6 col-md-4">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Task ID</label>
                  <div class="text-muted font-monospace">#{{ modal.task?.Id }}</div>
                </div>
                <div class="col-6 col-md-4">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Priority</label>
                  <div>
                    <span v-if="modal.task?.Priority" class="badge" :class="getPriorityBadgeClass(modal.task?.Priority)">
                      {{ modal.task?.Priority }}
                    </span>
                    <span v-else class="text-muted">—</span>
                  </div>
                </div>
                <div class="col-6 col-md-4">
                  <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider">Column</label>
                  <div v-if="projectStore.userRole === 'Owner' || projectStore.userRole === 'Manager' || isAssignedToCurrentUser(modal.task)">
                    <select :value="modal.task?.ColumnId" @change="changeTaskColumnFromSelect($event.target.value)" class="form-select form-select-sm" style="border-radius: 8px;">
                      <option v-for="col in columns" :key="col.Id" :value="col.Id">{{ col.Name }}</option>
                    </select>
                  </div>
                  <div v-else>
                    <span class="badge" :style="{ background: getColById(modal.task?.ColumnId)?.bgMid, color: getColById(modal.task?.ColumnId)?.color }">
                      {{ getColById(modal.task?.ColumnId)?.Name }}
                    </span>
                  </div>
                </div>
              </div>

              <!-- Assigned Users list -->
              <div v-if="modal.task?.AssignedUsers" class="mb-2">
                <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider d-flex align-items-center gap-2">
                  {{ $t('tasks.assigned_to') }}
                  <span class="badge bg-body-secondary text-secondary border rounded-pill">{{ modal.task.AssignedUsers.length }}</span>
                </label>
                <div v-if="modal.task.AssignedUsers.length === 0" class="text-muted small fst-italic py-2">{{ $t('tasks.no_assignee') }}</div>
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
                      </div>
                    </div>
                  </div>
                </div>
              </div>

            <!-- Unified Activity Feed -->
            <div class="mt-4 pt-4 border-top">
              <label class="form-label fw-semibold text-secondary small text-uppercase tracking-wider d-flex align-items-center gap-2 mb-3">
                <i class="bi bi-activity"></i> Activity
                <span class="badge bg-body-secondary text-secondary border rounded-pill">{{ unifiedFeed.length }}</span>
              </label>

              <!-- Loading state -->
              <div v-if="loadingFeed" class="text-center py-3 text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span> Loading...
              </div>

              <div v-else class="d-flex flex-column gap-0 mb-4">

                <!-- Load More Comments Button -->
                <div v-if="hasMoreFeed" class="text-center mb-3">
                  <button class="btn btn-sm btn-outline-secondary rounded-pill px-3 py-1" @click="loadMoreFeed" :disabled="loadingFeed" style="font-size: 0.8rem; transition: all 0.2s ease;">
                    <span v-if="loadingFeed" class="spinner-border spinner-border-sm me-1" role="status"></span>
                    <i v-else class="bi bi-arrow-up-circle me-1"></i> Load previous activity
                  </button>
                </div>

                <!-- Empty state -->
                <div v-if="unifiedFeed.length === 0" class="text-center py-3 text-muted small fst-italic">
                  No activity yet. Edit the task or leave a comment to get started!
                </div>

                <!-- Feed items -->
                <div v-for="(item, idx) in unifiedFeed" :key="(item.Type || item.type) + '-' + (item.Id || item.id)">

                  <!-- ── COMMENT ITEM ── -->
                  <div v-if="(item.Type || item.type)?.toLowerCase() === 'comment'" class="d-flex gap-3 py-3" :class="idx > 0 ? 'border-top border-dashed' : ''">
                    <div class="user-avatar text-white d-flex align-items-center justify-content-center fw-bold rounded-circle flex-shrink-0 mt-1"
                      :style="{ background: getUserColor(item.UserName), width: '32px', height: '32px', fontSize: '12px' }">
                      {{ userInitial(item.UserName) }}
                    </div>
                    <div class="flex-grow-1">
                      <div class="d-flex align-items-center gap-2 mb-1">
                        <span class="fw-semibold text-body" style="font-size: 0.9rem;">{{ item.UserName }}</span>
                        <span class="text-muted" style="font-size: 0.75rem;">{{ formatTimeAgo(item.CreatedAt) }}</span>
                        <button v-if="item.UserId === projectStore.user?.Id" class="btn btn-link text-danger p-0 ms-auto text-decoration-none" style="font-size: 0.8rem;" @click="removeComment(item.Id)">
                          Delete
                        </button>
                      </div>
                      <div class="text-body bg-body-secondary p-2 rounded-3 border" style="font-size: 0.9rem; white-space: pre-wrap;">{{ item.Content }}</div>
                    </div>
                  </div>

                  <!-- ── ACTIVITY ITEM ── -->
                  <div v-else-if="(item.Type || item.type)?.toLowerCase() === 'activity'" class="d-flex gap-3 py-3" :class="idx > 0 ? 'border-top border-dashed' : ''">
                    <div class="flex-shrink-0 mt-1 d-flex align-items-center justify-content-center rounded-circle border bg-body-secondary"
                      :style="{ width: '32px', height: '32px' }">
                      <i class="bi bi-pencil text-secondary" style="font-size: 11px;"></i>
                    </div>
                    <div class="flex-grow-1">
                      <div class="d-flex align-items-center gap-2 mb-2">
                        <span class="fw-semibold text-body" style="font-size: 0.9rem;">{{ item.UserName }}</span>
                        <span class="text-muted" style="font-size: 0.75rem;">{{ formatTimeAgo(item.CreatedAt || item.createdAt) }}</span>
                      </div>
                      <!-- Change list -->
                      <ul class="list-unstyled mb-0 d-flex flex-column gap-1">
                        <li v-for="change in item.Changes" :key="change.Field"
                          class="d-flex align-items-baseline gap-2 text-secondary"
                          style="font-size: 0.85rem; line-height: 1.4;">
                          <span class="text-muted flex-shrink-0" style="font-size: 10px;">○</span>
                          <span>
                            <span class="fw-semibold text-body">{{ change.Field }}</span>:
                            <!-- Description special case -->
                            <template v-if="change.NewValue === '__description_changed__'">
                              <span class="text-primary" style="font-style: italic; cursor: default;">Contents of changes</span>
                            </template>
                            <!-- Assignee added (no old value) -->
                            <template v-else-if="change.Field.includes('Assignee Added')">
                              <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-2" style="font-size: 0.75rem;">+ {{ change.NewValue }}</span>
                            </template>
                            <!-- Assignee removed (no new value) -->
                            <template v-else-if="change.Field.includes('Assignee Removed')">
                              <span class="badge bg-danger-subtle text-danger border border-danger-subtle rounded-pill px-2" style="font-size: 0.75rem;">− {{ change.OldValue }}</span>
                            </template>
                            <!-- Standard change: old → new -->
                            <template v-else>
                              <span class="text-muted text-decoration-line-through me-1" v-if="change.OldValue">{{ change.OldValue }}</span>
                              <i class="bi bi-arrow-right text-muted mx-1" style="font-size: 10px;" v-if="change.OldValue && change.NewValue"></i>
                              <span class="fw-medium text-body">{{ change.NewValue ?? '—' }}</span>
                            </template>
                          </span>
                        </li>
                      </ul>
                    </div>
                  </div>

                </div>
              </div>

              <!-- Comment Input -->
              <div class="d-flex gap-3">
                <div class="user-avatar text-white d-flex align-items-center justify-content-center fw-bold rounded-circle flex-shrink-0"
                  :style="{ background: getUserColor(projectStore.user?.Email), width: '32px', height: '32px', fontSize: '12px' }">
                  {{ userInitial(projectStore.user?.Email || 'U') }}
                </div>
                <div class="flex-grow-1 position-relative">
                  <textarea v-model="newComment" class="form-control" rows="2" placeholder="Write a comment..." style="border-radius: 12px; font-size: 0.9rem; padding-bottom: 40px; resize: none;"></textarea>
                  <button class="btn btn-primary btn-sm position-absolute bottom-0 end-0 m-2" @click="submitComment" :disabled="!newComment.trim() || submittingComment" style="border-radius: 8px;">
                    <span v-if="submittingComment" class="spinner-border spinner-border-sm me-1" role="status"></span>
                    <i v-else class="bi bi-send-fill me-1"></i> Send
                  </button>
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
                <label class="form-label fw-semibold text-secondary small text-uppercase">{{ $t('taskModal.task_name') }}</label>
                <input id="edit-title" v-model="editForm.title" type="text" class="form-control" :placeholder="$t('taskModal.task_name')" />
              </div>

              <div class="mb-3">
                <label class="form-label fw-semibold text-secondary small text-uppercase">{{ $t('tasks.description') }}</label>
                <textarea id="edit-desc" v-model="editForm.description" class="form-control" rows="4" :placeholder="$t('tasks.description')"></textarea>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-12 col-md-6">
                  <label class="form-label fw-semibold text-secondary small text-uppercase">Start Date</label>
                  <input id="edit-startdate" v-model="editForm.startDate" type="date" class="form-control" />
                </div>
                <div class="col-12 col-md-6">
                  <label class="form-label fw-semibold text-secondary small text-uppercase">{{ $t('tasks.deadline') }}</label>
                  <input id="edit-deadline" v-model="editForm.deadline" type="date" class="form-control" />
                </div>
                <div class="col-12 col-md-6">
                  <label class="form-label fw-semibold text-secondary small text-uppercase">Est. Hours</label>
                  <input id="edit-esthours" v-model="editForm.estimatedHours" type="number" step="0.5" class="form-control" placeholder="e.g. 2.5" />
                </div>
                <div class="col-12 col-md-6">
                  <label class="form-label fw-semibold text-secondary small text-uppercase">Actual Hours</label>
                  <input id="edit-acthours" v-model="editForm.actualHours" type="number" step="0.5" class="form-control" placeholder="e.g. 3.0" />
                </div>
                <div class="col-12 col-md-6">
                  <label class="form-label fw-semibold text-secondary small text-uppercase">Column</label>
                  <select id="edit-status" v-model="editForm.columnId" class="form-select">
                    <option v-for="col in columns" :key="col.Id" :value="col.Id">{{ col.Name }}</option>
                  </select>
                </div>
                <div class="col-12 col-md-6">
                  <label class="form-label fw-semibold text-secondary small text-uppercase">Priority</label>
                  <select id="edit-priority" v-model="editForm.priority" class="form-select">
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                  </select>
                </div>
              </div>

              <div class="mb-4">
                <label class="form-label fw-semibold text-secondary small text-uppercase">Assignees</label>
                <!-- Display currently assigned users with remove button -->
                <div v-if="editAssignedUsers.length > 0" class="row g-2 mb-3">
                  <div v-for="user in editAssignedUsers" :key="user.UserId" class="col-12 col-md-6">
                    <div class="card bg-body-secondary border p-2 rounded-3 h-100">
                      <div class="d-flex align-items-center gap-2">
                        <div class="user-avatar bg-secondary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width:24px; height:24px; font-size:10px;">
                          {{ userInitial(user.Email) }}
                        </div>
                        <div class="flex-grow-1 min-w-0 text-start">
                          <div class="small fw-semibold text-body text-truncate" :title="user.Email">{{ user.Email }}</div>
                        </div>
                        <button v-if="projectStore.userRole === 'Owner' || projectStore.userRole === 'Manager'" class="btn btn-sm btn-outline-danger p-1 ms-auto" @click.prevent="removeUserLocal(user.UserId)" title="Remove assignment" style="width: 24px; height: 24px; display: inline-flex; align-items: center; justify-content: center;">
                          <i class="bi bi-trash3-fill" style="font-size:10px"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
                
                <!-- Add Assignee section -->
                <div v-if="projectStore.userRole === 'Owner' || projectStore.userRole === 'Manager'" class="d-flex align-items-center gap-2">
                  <select v-model="selectedAssigneeId" @change="assignUserLocal" class="form-select form-select-sm" style="border-radius: 8px;">
                    <option :value="null">-- {{ $t('taskModal.select_assignee') }} --</option>
                    <option v-for="m in projectMembersNotAssignedToEdit" :key="m.UserId" :value="m.UserId">
                      {{ m.Email }} ({{ m.Role }})
                    </option>
                  </select>
                </div>
              </div>

              <div class="mt-4 pt-3 border-top text-end">
                <button class="btn btn-sm btn-outline-danger px-3 py-2 fw-semibold" @click.prevent="handleDeleteTask">
                  <i class="bi bi-trash3 me-1"></i> {{ $t('tasks.delete_task') }}
                </button>
              </div>
            </div>

            <!-- ── FOOTER — View mode ── -->
            <div v-if="!editMode" class="modal-footer p-4 border-top bg-body-secondary text-end">
              <button class="btn btn-sm btn-outline-secondary px-4 py-2" @click="closeModal" style="border-radius: 8px;">{{ $t('tasks.cancel') }}</button>
            </div>

            <!-- ── FOOTER — Edit mode ── -->
            <div v-else class="modal-footer p-4 border-top bg-body-secondary d-flex justify-content-end gap-2">
              <button class="btn btn-sm btn-outline-secondary px-3 py-2 fw-semibold" @click="cancelEdit" style="border-radius: 8px;">
                <i class="bi bi-x me-1"></i> {{ $t('tasks.cancel') }}
              </button>
              <button class="btn btn-sm btn-primary px-3 py-2 fw-semibold" @click="saveEdit" :disabled="saving" style="border-radius: 8px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;">
                <span v-if="saving" class="spinner-border spinner-border-sm me-2" role="status"></span>
                <i v-else class="bi bi-check2 me-1"></i>
                {{ saving ? $t('common.loading') : $t('tasks.save') }}
              </button>
            </div>

          </div>
        </div>
      </div>
    </Teleport>

  </div>
</template>

<script setup>
import draggable from 'vuedraggable'
import { ref, reactive, computed, onMounted, onUnmounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { assignTask, updateTask, removeAssignment, updateTaskColumn, deleteTask, addComment, deleteComment, getTaskFeed } from '../services/taskService.js'
import { getMembers, addMember, updateMemberRole, removeMember, getProjectTasks, getProjectColumns } from '../services/projectService.js'
import { useProjectStore } from '../stores/projectStore.js'
const projectStore = useProjectStore()
import { toastSuccess, toastError, confirm, extractMessage } from '../utils/swal.js'
import Swal from 'sweetalert2'

const { t } = useI18n()

const tasks         = ref([])
const loading       = ref(false)
const saving        = ref(false)
const modal         = reactive({ open: false, task: null })
const editMode      = ref(false)
const editForm      = reactive({ title: '', description: '', deadline: '', startDate: '', estimatedHours: null, actualHours: null, columnId: null, priority: 'Medium', assignedUserIds: [] })

// Filter states
const filters = reactive({ search: '', priority: null, assigneeId: null })
const assigneeSearch = ref('')
let searchTimeout = null

const hasActiveFilters = computed(() => {
  return !!filters.search || !!filters.priority || !!filters.assigneeId
})

const filteredAssignees = computed(() => {
  if (!members.value) return []
  if (!assigneeSearch.value) return members.value
  const s = assigneeSearch.value.toLowerCase()
  return members.value.filter(m => m.Email && m.Email.toLowerCase().includes(s))
})

const getAssigneeName = (id) => {
  if (!id) return ''
  if (id === '00000000-0000-0000-0000-000000000000') return 'Unassigned'
  const m = members.value.find(u => u.UserId === id)
  return m ? m.Email : 'Unknown'
}

const onSearchInput = () => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    Object.values(columnStates).forEach(col => { col.page = 1; col.hasMore = true })
    loadData()
  }, 500)
}

const setFilter = (key, value) => {
  filters[key] = value
  Object.values(columnStates).forEach(col => { col.page = 1; col.hasMore = true })
  loadData()
}

const clearFilters = () => {
  filters.search = ''
  filters.priority = null
  filters.assigneeId = null
  Object.values(columnStates).forEach(col => { col.page = 1; col.hasMore = true })
  loadData()
}

const getUserColor = (name) => {
  if (!name) return '#6b7280'
  const colors = ['#f59e0b', '#ef4444', '#ec4899', '#8b5cf6', '#06b6d4', '#10b981', '#3b82f6']
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return colors[Math.abs(hash) % colors.length]
}

// Pagination states
const columnStates = reactive({})
const columns = ref([])
const pageSize = 15

// Project members states
const members        = ref([])
const loadingMembers = ref(false)

// Assignee selection states
const selectedAssigneeId = ref(null)

const tasksByColumnId = computed(() => {
  const map = {}
  tasks.value.forEach(t => {
    if (!map[t.ColumnId]) map[t.ColumnId] = []
    map[t.ColumnId].push(t)
  })
  return map
})
const getTasksByColumnId = (colId) => tasksByColumnId.value[colId] || []
const getColById     = (colId) => {
  const col = columns.value.find(c => c.Id === colId)
  if (!col) return null
  return col
}

const isOverdue = (task) => {
  if (!task.Deadline) return false
  const col = getColById(task.ColumnId)
  if (col && col.IsCompletedStage) return false
  return new Date(task.Deadline) < new Date()
}

const openModal = (task) => {
  modal.task = task
  modal.open = true
  editMode.value = false
  document.body.style.overflow = 'hidden'
  loadFeed(task.Id)
}

// Unified Feed logic
const unifiedFeed = ref([])
const newComment = ref('')
const loadingFeed = ref(false)
const submittingComment = ref(false)
const feedPage = ref(1)
const hasMoreFeed = ref(false)

const formatTimeAgo = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  const now = new Date()
  const diff = Math.floor((now - d) / 1000)
  if (diff < 60) return 'Just now'
  if (diff < 3600) return `${Math.floor(diff/60)}m ago`
  if (diff < 86400) return `${Math.floor(diff/3600)}h ago`
  return d.toLocaleDateString()
}

const loadFeed = async (taskId) => {
  loadingFeed.value = true
  feedPage.value = 1
  try {
    const res = await getTaskFeed(taskId, feedPage.value, 5)
    unifiedFeed.value = res.data?.Items || []
    hasMoreFeed.value = (res.data?.Page < res.data?.TotalPages)
  } catch (err) {
    console.error('Failed to load feed', err)
  } finally {
    loadingFeed.value = false
  }
}

const loadMoreFeed = async () => {
  if (!hasMoreFeed.value || loadingFeed.value) return
  loadingFeed.value = true
  feedPage.value++
  try {
    const res = await getTaskFeed(modal.task.Id, feedPage.value, 5)
    const newItems = res.data?.Items || []
    unifiedFeed.value = [...newItems, ...unifiedFeed.value]
    hasMoreFeed.value = (res.data?.Page < res.data?.TotalPages)
  } catch (err) {
    console.error('Failed to load more feed', err)
  } finally {
    loadingFeed.value = false
  }
}

const submitComment = async () => {
  if (!newComment.value.trim() || !modal.task) return
  submittingComment.value = true
  try {
    await addComment(modal.task.Id, { content: newComment.value })
    newComment.value = ''
    await loadFeed(modal.task.Id)
  } catch (err) {
    toastError('Failed to add comment')
  } finally {
    submittingComment.value = false
  }
}

const removeComment = async (id) => {
  const ok = await Swal.fire({
    title: 'Delete Comment?',
    text: 'You cannot undo this action.',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Yes, delete it!'
  })
  if (!ok.isConfirmed) return
  try {
    await deleteComment(id)
    await loadFeed(modal.task.Id)
  } catch (err) {
    toastError('Failed to delete comment')
  }
}

const closeModal = () => {
  modal.open = false
  editMode.value = false
  document.body.style.overflow = ''
  selectedAssigneeId.value = null
}

const toDateLocal = (iso) => {
  if (!iso) return ''
  return iso.slice(0, 10)
}

const toggleEdit = () => {
  if (!editMode.value) {
    editForm.title       = modal.task?.Title || ''
    editForm.description = modal.task?.Description || ''
    editForm.deadline    = toDateLocal(modal.task?.Deadline)
    editForm.startDate   = toDateLocal(modal.task?.StartDate)
    editForm.estimatedHours = modal.task?.EstimatedHours ?? null
    editForm.actualHours = modal.task?.ActualHours ?? null
    editForm.columnId    = modal.task?.ColumnId || (columns.value.length > 0 ? columns.value[0].Id : null)
    editForm.priority    = modal.task?.Priority || 'Medium'
    editForm.assignedUserIds = modal.task?.AssignedUsers?.map(u => u.UserId) || []
  }
  editMode.value = !editMode.value
}

const cancelEdit = () => { editMode.value = false }

const saveEdit = async () => {
  saving.value = true
  try {
    const payload = {
      title:       editForm.title,
      description: editForm.description,
      Deadline:    editForm.deadline ? new Date(editForm.deadline).toISOString() : null,
      StartDate:   editForm.startDate ? new Date(editForm.startDate).toISOString() : null,
      EstimatedHours: editForm.estimatedHours,
      ActualHours: editForm.actualHours,
      ColumnId:    editForm.columnId,
      Priority:    editForm.priority,
      AssignedUserIds: editForm.assignedUserIds
    }
    await updateTask(modal.task.Id, payload)
    await loadData()
    const updated = tasks.value.find(t => t.Id === modal.task.Id)
    if (updated) modal.task = updated
    editMode.value = false
    toastSuccess('Task updated successfully!')
    await loadFeed(modal.task.Id)
  } catch (err) {
    console.error(err)
    toastError(extractMessage(err, t('errors.default')))
  } finally {
    saving.value = false
  }
}

const handleDeleteTask = async () => {
  const ok = await confirm(
    t('tasks.delete_confirm_title'),
    t('tasks.delete_confirm_desc'),
    t('tasks.delete_confirm_btn')
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
    toastError(extractMessage(err, t('errors.default')))
  } finally {
    saving.value = false
  }
}

// Members computed list (members not assigned to the active task)
const projectMembersNotAssigned = computed(() => {
  if (!members.value) return []
  const assigned = modal.task?.AssignedUsers?.map(u => u.UserId) || []
  return members.value.filter(m => !assigned.includes(m.UserId))
})

const projectMembersNotAssignedToEdit = computed(() => {
  if (!members.value) return []
  return members.value.filter(m => !editForm.assignedUserIds.includes(m.UserId))
})

const editAssignedUsers = computed(() => {
  if (!members.value) return []
  return members.value.filter(m => editForm.assignedUserIds.includes(m.UserId))
})

const loadColumnData = async (colId, append = false) => {
  if (!projectStore.currentProjectId) return
  
  const colState = columnStates[colId]
  if (!colState) return
  
  colState.loading = true
  try {
    const res = await getProjectTasks(projectStore.currentProjectId, colId, colState.page, pageSize, filters.search, filters.priority, filters.assigneeId)
    const pageData = res?.data
    if (pageData) {
      if (append) {
        const newItems = (pageData.Items || []).filter(item => !tasks.value.some(t => t.Id === item.Id))
        tasks.value = [...tasks.value, ...newItems]
      } else {
        const otherTasks = tasks.value.filter(t => t.ColumnId !== colId)
        tasks.value = [...otherTasks, ...(pageData.Items || [])]
      }
      colState.hasMore = pageData.Page < pageData.TotalPages
    } else {
      if (!append) tasks.value = tasks.value.filter(t => t.ColumnId !== colId)
      colState.hasMore = false
    }
  } catch (e) {
    console.error(e)
    toastError(extractMessage(e, t('errors.default')))
  } finally {
    colState.loading = false
  }
}

const loadMore = async (colId) => {
  const colState = columnStates[colId]
  if (!colState || !colState.hasMore || colState.loading) return
  colState.page++
  await loadColumnData(colId, true)
}

const loadColumns = async () => {
  if (!projectStore.currentProjectId) {
    columns.value = []
    return
  }
  try {
    const res = await getProjectColumns(projectStore.currentProjectId)
    columns.value = res?.data || []
    
    // Initialize states
    Object.keys(columnStates).forEach(k => delete columnStates[k])
    columns.value.forEach(col => {
      // Generate standard colors
      if (col.IsCompletedStage) {
         col.color = 'var(--status-done-color)'
         col.bgLight = 'var(--status-done-bg-light)'
         col.bgMid = 'var(--status-done-bg-mid)'
      } else if (col.Order === 0) {
         col.color = 'var(--status-todo-color)'
         col.bgLight = 'var(--status-todo-bg-light)'
         col.bgMid = 'var(--status-todo-bg-mid)'
      } else {
         col.color = 'var(--status-inprogress-color)'
         col.bgLight = 'var(--status-inprogress-bg-light)'
         col.bgMid = 'var(--status-inprogress-bg-mid)'
      }
      
      columnStates[col.Id] = { page: 1, hasMore: true, loading: false }
    })
  } catch(e) {
    console.error(e)
  }
}

const loadData = async () => {
  if (!projectStore.currentProjectId) {
    tasks.value = []
    return
  }
  loading.value = true
  await loadColumns()
  await Promise.all(columns.value.map(col => loadColumnData(col.Id)))
  loading.value = false
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
  Object.values(columnStates).forEach(col => { col.page = 1; col.hasMore = true })
  await Promise.all([loadData(), loadMembers()])
}

// Note: Project actions & member management are handled in SettingsView and MembersView respectively.

// Task assignment Actions
const isAssignedToCurrentUser = (task) => {
  if (!task || !task.AssignedUsers) return false
  return task.AssignedUsers.some(u => u.Email === projectStore.currentUserEmail)
}

const changeTaskColumnFromSelect = async (newColumnId) => {
  if (!modal.task) return
  const oldColumnId = modal.task.ColumnId
  modal.task.ColumnId = parseInt(newColumnId)
  try {
    await updateTaskColumn({
      taskId: modal.task.Id,
      columnId: parseInt(newColumnId)
    })
    toastSuccess('Task column updated successfully!')
  } catch (err) {
    modal.task.ColumnId = oldColumnId
    console.error(err)
    toastError(extractMessage(err, t('errors.default')))
  }
}

// Task assignment Actions
const assignUserLocal = () => {
  if (selectedAssigneeId.value && !editForm.assignedUserIds.includes(selectedAssigneeId.value)) {
    editForm.assignedUserIds.push(selectedAssigneeId.value)
  }
  selectedAssigneeId.value = null
}

const removeUserLocal = (userId) => {
  editForm.assignedUserIds = editForm.assignedUserIds.filter(id => id !== userId)
}

// Drag & drop handled by vuedraggable
const onChange = async (evt, colId) => {
  if (evt.added) {
    const task = evt.added.element
    if (!task || task.ColumnId === colId) return

    const oldColumnId = task.ColumnId
    task.ColumnId = colId

    try {
      await updateTaskColumn({ taskId: task.Id, columnId: colId })
      toastSuccess('Status updated!')
    } catch (err) {
      task.ColumnId = oldColumnId
      console.error('Failed to update status, rolled back:', err)
      toastError(extractMessage(err, t('errors.default')))
    }
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
    case 'manager':
      return 'bg-primary-subtle text-primary border border-primary-subtle'
    case 'member':
    default:
      return 'bg-secondary-subtle text-secondary border border-secondary-subtle'
  }
}

const getPriorityBadgeClass = (priority) => {
  switch (priority) {
    case 'High': return 'bg-danger text-white'
    case 'Medium': return 'bg-warning text-dark'
    case 'Low': return 'bg-info text-white'
    default: return 'bg-secondary text-white'
  }
}

const onKeydown = (e) => { if (e.key === 'Escape') closeModal() }
const onTaskCreated = (e) => {
  const task = e?.detail
  if (task) {
    if (!tasks.value.some(t => t.Id === task.Id)) {
      tasks.value.push(task)
    }
  } else {
    loadData()
  }
}

const onTaskUpdated = (e) => {
  const task = e.detail
  if (!task) return
  const idx = tasks.value.findIndex(t => t.Id === task.Id)
  if (idx !== -1) {
    tasks.value.splice(idx, 1, task)
  }
  if (modal.open && modal.task && modal.task.Id === task.Id) {
    modal.task = task
  }
}

const onTaskDeleted = (e) => {
  const taskId = e.detail
  tasks.value = tasks.value.filter(t => t.Id !== taskId)
  if (modal.open && modal.task && modal.task.Id === taskId) {
    closeModal()
    Swal.fire({
      title: 'Task Deleted',
      text: 'This task has been deleted by another user.',
      icon: 'info',
      confirmButtonText: 'OK'
    })
  }
}

onMounted(() => {
  window.addEventListener('keydown', onKeydown)
  window.addEventListener('task-created', onTaskCreated)
  window.addEventListener('task-updated', onTaskUpdated)
  window.addEventListener('task-deleted', onTaskDeleted)
  refreshAll()
})

onUnmounted(() => {
  window.removeEventListener('keydown', onKeydown)
  window.removeEventListener('task-created', onTaskCreated)
  window.removeEventListener('task-updated', onTaskUpdated)
  window.removeEventListener('task-deleted', onTaskDeleted)
})

const formatDate      = (d) => d ? new Date(d).toLocaleString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—'
const formatDateShort = (d) => d ? new Date(d).toLocaleDateString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'
const userInitial     = (email) => email ? email[0].toUpperCase() : '?'
</script>

<style scoped>
.page-title {
  font-size: 1.68rem;
  letter-spacing: -0.02em;
  font-weight: 700;
  color: var(--bs-heading-color) !important;
}
.task-tag-card {
  transition: transform 0.25s cubic-bezier(0.16, 1, 0.3, 1), box-shadow 0.25s ease;
  cursor: grab;
  border-top-width: 4px !important;
}
.task-tag-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md) !important;
}
.task-tag-card--dragging {
  opacity: 0.5;
  transform: scale(0.96);
  cursor: grabbing !important;
}
.task-tag-card--ghost {
  opacity: 0.4;
  background: var(--bs-secondary-bg);
  border: 1px dashed var(--bs-border-color);
}
.kanban-col {
  min-height: 580px;
  background-color: rgba(var(--bs-secondary-bg-rgb, 241, 245, 249), 0.35) !important;
  border: 1px solid var(--bs-border-color) !important;
  border-radius: var(--radius-lg) !important;
  transition: all 0.2s ease;
}
.kanban-col--dragover { 
  background-color: rgba(99, 102, 241, 0.05) !important;
  border-color: rgba(99, 102, 241, 0.4) !important;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.08);
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