<template>
  <div class="p-3 p-md-4 text-start">

    <!-- Header Section -->
    <div class="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-3">
     
      <!-- <button 
        class="btn btn-outline-secondary d-flex align-items-center justify-content-center" 
        @click="loadDashboardData" 
        :disabled="loading" 
        title="Refresh Data"
        style="width: 38px; height: 38px; border-radius: 8px;"
      >
        <span v-if="loading" class="spinner-border spinner-border-sm text-secondary" role="status"></span>
        <i v-else class="bi bi-arrow-clockwise fs-5"></i>
      </button> -->
    </div>

    <!-- Loading State -->
    <div v-if="loading && projectTasks.length === 0" class="text-center py-5 my-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">{{ $t('dashboard.loading') }}</p>
    </div>

    <!-- Dashboard Content -->
    <div v-else>
      <!-- Stats Cards Grid -->
      <div class="row row-cols-1 row-cols-md-2 row-cols-lg-4 g-3 mb-4">
        <!-- Total Tasks -->
        <div class="col">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100 bg-body">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <span class="text-muted small fw-bold text-uppercase tracking-wider">{{ $t('dashboard.total_tasks') }}</span>
                <h3 class="fw-bold text-body mt-2 mb-0">{{ stats.totalTasks }}</h3>
              </div>
              <div class="bg-primary bg-opacity-10 text-primary rounded-3 p-2 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;">
                <i class="bi bi-list-task fs-4"></i>
              </div>
            </div>
          </div>
        </div>

        <!-- Tasks in Progress -->
        <div class="col">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100 bg-body">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <span class="text-muted small fw-bold text-uppercase tracking-wider">{{ $t('dashboard.in_progress') }}</span>
                <h3 class="fw-bold text-body mt-2 mb-0">{{ stats.inProgressTasks }}</h3>
              </div>
              <div class="bg-warning bg-opacity-10 text-warning rounded-3 p-2.5 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;">
                <i class="bi bi-clock-history fs-4"></i>
              </div>
            </div>
          </div>
        </div>

        <!-- Completed Tasks -->
        <div class="col">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100 bg-body">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <span class="text-muted small fw-bold text-uppercase tracking-wider">{{ $t('dashboard.completed') }}</span>
                <h3 class="fw-bold text-body mt-2 mb-0">{{ stats.completedTasks }}</h3>
              </div>
              <div class="bg-success bg-opacity-10 text-success rounded-3 p-2.5 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;">
                <i class="bi bi-check-circle-fill fs-4"></i>
              </div>
            </div>
          </div>
        </div>

        <!-- Team Members Count -->
        <div class="col">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100 bg-body">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <span class="text-muted small fw-bold text-uppercase tracking-wider">{{ $t('dashboard.team_members') }}</span>
                <h3 class="fw-bold text-body mt-2 mb-0">{{ stats.memberCount }}</h3>
              </div>
              <div class="bg-primary bg-opacity-10 text-primary rounded-3 p-2.5 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;">
                <i class="bi bi-people-fill fs-4"></i>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Layout Rows -->
      <div class="row g-4">
        <!-- Recent Tasks Table (Col-lg-8) -->
        <div class="col-12 col-lg-8">
          <div class="card border-0 shadow-sm p-4 rounded-3 h-100 bg-body">
            <div class="d-flex align-items-center justify-content-between mb-3">
              <h4 class="fw-bold text-body h5 mb-0">{{ $t('dashboard.recent_tasks') }}</h4>
              <router-link :to="`/projects/${projectId}/tasks`" class="btn btn-sm btn-link text-primary fw-semibold text-decoration-none p-0">{{ $t('dashboard.view_all') }}</router-link>
            </div>
            
            <div v-if="projectTasks.length === 0" class="text-center py-5 border border-dashed rounded-3 text-muted">
              <i class="bi bi-clipboard-x fs-2 opacity-50"></i>
              <p class="small mt-2 mb-0">{{ $t('dashboard.no_tasks') }}</p>
            </div>

            <div v-else class="table-responsive">
              <table class="table table-hover align-middle border-0 mb-0">
                <thead class="table-light">
                  <tr>
                    <th scope="col" class="border-0 rounded-start">{{ $t('dashboard.title_col') }}</th>
                    <th scope="col" class="border-0">{{ $t('dashboard.deadline_col') }}</th>
                    <th scope="col" class="border-0 rounded-end">{{ $t('dashboard.status_col') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="task in recentTasks" :key="task.Id" class="border-bottom">
                    <td>
                      <div class="fw-semibold text-body text-truncate" style="max-width: 350px;">{{ task.Title }}</div>
                    </td>
                    <td>
                      <span class="small" :class="isOverdue(task) ? 'text-danger fw-bold' : 'text-muted'">
                        {{ task.Deadline ? formatDateShort(task.Deadline) : '—' }}
                      </span>
                    </td>
                    <td>
                      <span class="badge text-uppercase font-monospace" :class="getStatusBadgeClass(task.ColumnId)" style="font-size: 9px; padding: 4px 8px;">
                        {{ getStatusLabel(task.ColumnId) }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- Task Breakdown and Members (Col-lg-4) -->
        <div class="col-12 col-lg-4 d-flex flex-column gap-4">
          <!-- Tasks Breakdown Card -->
          <div class="card border-0 shadow-sm p-4 rounded-3 bg-body">
            <h4 class="fw-bold text-body h5 mb-3">{{ $t('dashboard.breakdown') }}</h4>
            <div class="d-flex flex-column gap-3">
              <div v-for="col in statusCounts" :key="col.status" class="border rounded-3 p-2.5">
                <div class="d-flex align-items-center justify-content-between mb-1.5">
                  <div class="small fw-semibold text-body d-flex align-items-center gap-2">
                    <span class="rounded-circle" :style="{ background: col.color, width: '8px', height: '8px', display: 'inline-block' }"></span>
                    {{ col.label }}
                  </div>
                  <span class="fw-bold text-body small">{{ col.count }} tasks ({{ col.percent }}%)</span>
                </div>
                <div class="progress" style="height: 5px;">
                  <div 
                    class="progress-bar" 
                    role="progressbar" 
                    :style="{ width: col.percent + '%', backgroundColor: col.color }" 
                    :aria-valuenow="col.percent" 
                    aria-valuemin="0" 
                    aria-valuemax="100"
                  ></div>
                </div>
              </div>
            </div>
          </div>

          <!-- Members Quick List Card -->
          <div class="card border-0 shadow-sm p-4 rounded-3 bg-body">
            <h4 class="fw-bold text-body h5 mb-3">{{ $t('dashboard.team_members') }}</h4>
            <div class="d-flex flex-column gap-2" style="max-height: 250px; overflow-y: auto;">
              <div 
                v-for="member in projectMembers" 
                :key="member.UserId" 
                class="d-flex align-items-center justify-content-between py-2 border-bottom last-border-0"
              >
                <div class="d-flex align-items-center gap-2 min-w-0">
                  <div class="user-avatar bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width:30px; height:30px; font-size: 11px; background: linear-gradient(135deg, #4f46e5, #6366f1) !important;">
                    {{ userInitial(member.Email) }}
                  </div>
                  <div class="text-start text-truncate" style="max-width: 140px;" :title="member.Email">
                    <span class="small fw-semibold text-body d-block text-truncate">{{ member.Email }}</span>
                  </div>
                </div>
                <span class="badge text-uppercase font-monospace" :class="getRoleBadgeClass(member.Role)" style="font-size: 8px; padding: 2px 4px;">{{ member.Role }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { getProjectTasks, getProjectTaskStats, getMembers, getProjectColumns } from '../services/projectService.js'
import { useProjectStore } from '../stores/projectStore.js'
import { toastError } from '../utils/swal.js'

const route = useRoute()
const projectStore = useProjectStore()
const { t } = useI18n()
const loading = ref(false)
const projectTasks = ref([])
const taskStats = ref(null)
const projectMembers = ref([])
const projectColumns = ref([])

const projectId = computed(() => route.params.projectId)

const loadDashboardData = async () => {
  if (!projectId.value) return
  loading.value = true
  try {
    const [tasksRes, statsRes, membersRes, columnsRes] = await Promise.all([
      getProjectTasks(projectId.value),
      getProjectTaskStats(projectId.value),
      getMembers(projectId.value),
      getProjectColumns(projectId.value)
    ])
    projectTasks.value = tasksRes?.data?.Items || []
    taskStats.value = statsRes?.data || null
    projectMembers.value = membersRes?.data || []
    projectColumns.value = columnsRes?.data || []
  } catch (error) {
    console.error('Error loading dashboard data', error)
    toastError('Failed to load dashboard data.')
  } finally {
    loading.value = false
  }
}

const stats = reactive({
  totalTasks: computed(() => taskStats.value?.TotalTasks || 0),
  inProgressTasks: computed(() => Math.max(0, (taskStats.value?.TotalTasks || 0) - (taskStats.value?.CompletedTasks || 0))),
  completedTasks: computed(() => taskStats.value?.CompletedTasks || 0),
  memberCount: computed(() => projectMembers.value.length)
})

const recentTasks = computed(() => {
  return [...projectTasks.value]
    .sort((a, b) => new Date(b.CreatedAt) - new Date(a.CreatedAt))
    .slice(0, 5)
})

const statusCounts = computed(() => {
  const total = taskStats.value?.TotalTasks || 1
  const cols = taskStats.value?.ColumnStats || []
  const colors = ['#64748b', '#6366f1', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6'];
  return cols.map((c, i) => {
    return { 
      label: c.ColumnName, 
      status: c.ColumnId, 
      count: c.TaskCount, 
      percent: Math.round((c.TaskCount / total) * 100), 
      color: colors[i % colors.length] 
    }
  })
})

const isOverdue = (task) => {
  if (!task.Deadline) return false
  const col = projectColumns.value.find(c => c.Id === task.ColumnId)
  if (col && col.IsCompletedStage) return false
  return new Date(task.Deadline) < new Date()
}

const getStatusBadgeClass = (columnId) => {
  const index = projectColumns.value.findIndex(c => c.Id === columnId)
  if (index === -1) return 'bg-secondary-subtle text-secondary border border-secondary-subtle'
  
  const classes = [
    'bg-secondary-subtle text-secondary border border-secondary-subtle',
    'bg-primary-subtle text-primary border border-primary-subtle',
    'bg-success-subtle text-success border border-success-subtle',
    'bg-warning-subtle text-warning border border-warning-subtle',
    'bg-info-subtle text-info border border-info-subtle',
    'bg-danger-subtle text-danger border border-danger-subtle'
  ]
  return classes[index % classes.length]
}

const getStatusLabel = (columnId) => {
  const col = projectColumns.value.find(c => c.Id === columnId)
  return col ? col.Name : 'Unknown'
}

const userInitial = (email) => email ? email[0].toUpperCase() : '?'

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

const formatDateShort = (d) => d ? new Date(d).toLocaleDateString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'

watch(projectId, () => {
  loadDashboardData()
})

onMounted(() => {
  loadDashboardData()
})
</script>

<style scoped>
.page-title {
  font-size: 1.68rem;
  letter-spacing: -0.02em;
  font-weight: 700;
}
.border-dashed {
  border-style: dashed !important;
  border-color: var(--bs-border-color) !important;
}
.last-border-0:last-child {
  border-bottom: 0 !important;
}
.card {
  transition: transform 0.2s cubic-bezier(0.16, 1, 0.3, 1), box-shadow 0.2s ease, border-color 0.2s ease;
}
.card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md) !important;
  border-color: rgba(13, 148, 136, 0.2) !important;
}
.progress {
  background-color: var(--bs-secondary-bg);
  border-radius: 4px;
}
.table > :not(caption) > * > * {
  padding: 12px 16px;
}
</style>