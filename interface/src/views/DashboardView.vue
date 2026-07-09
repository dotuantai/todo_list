<template>
  <div class="p-3 p-md-4 text-start">

    <!-- Header Section -->
    <div class="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-3">
      <div>
        <h2 class="fw-bold mb-0 page-title text-body">Dashboard</h2>
        <p class="text-muted small mb-0 mt-1">Tổng quan thông tin công việc, thành viên và tiến độ dự án của bạn.</p>
      </div>
      <button 
        class="btn btn-outline-secondary d-flex align-items-center justify-content-center" 
        @click="loadDashboardData" 
        :disabled="loading" 
        title="Refresh Data"
        style="width: 38px; height: 38px; border-radius: 8px;"
      >
        <span v-if="loading" class="spinner-border spinner-border-sm text-secondary" role="status"></span>
        <i v-else class="bi bi-arrow-clockwise fs-5"></i>
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="loading && allTasks.length === 0" class="text-center py-5 my-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">Đang tải dữ liệu dashboard...</p>
    </div>

    <!-- Empty State -->
    <div v-else-if="projectStore.projects.length === 0" class="text-center py-5 bg-body rounded-4 shadow-sm border border-dashed p-4">
      <i class="bi bi-folder2-open text-primary" style="font-size: 4rem;"></i>
      <h3 class="fw-bold text-body mt-3">Welcome to TaskFlow Pro</h3>
      <p class="text-muted mx-auto" style="max-width: 480px;">Bạn chưa tham gia dự án nào. Hãy bấm nút tạo dự án mới ở thanh bên để bắt tác quản lý công việc của bạn.</p>
    </div>

    <!-- Dashboard Content -->
    <div v-else>
      <!-- Stats Cards Grid -->
      <div class="row row-cols-1 row-cols-md-2 row-cols-lg-4 g-3 mb-4">
        <!-- Total Projects -->
        <div class="col">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <span class="text-muted small fw-bold text-uppercase tracking-wider">Dự án tham gia</span>
                <h3 class="fw-bold text-body mt-2 mb-0">{{ stats.totalProjects }}</h3>
              </div>
              <div class="bg-primary bg-opacity-10 text-primary rounded-3 p-2.5 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;">
                <i class="bi bi-folder-fill fs-4"></i>
              </div>
            </div>
          </div>
        </div>

        <!-- Total Tasks -->
        <div class="col">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <span class="text-muted small fw-bold text-uppercase tracking-wider">Tổng số công việc</span>
                <h3 class="fw-bold text-body mt-2 mb-0">{{ stats.totalTasks }}</h3>
              </div>
              <div class="bg-indigo-light text-indigo rounded-3 p-2.5 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;">
                <i class="bi bi-list-task fs-4"></i>
              </div>
            </div>
          </div>
        </div>

        <!-- Tasks in Progress -->
        <div class="col">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <span class="text-muted small fw-bold text-uppercase tracking-wider">Đang xử lý</span>
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
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <span class="text-muted small fw-bold text-uppercase tracking-wider">Đã hoàn thành</span>
                <h3 class="fw-bold text-body mt-2 mb-0">{{ stats.completedTasks }}</h3>
              </div>
              <div class="bg-success bg-opacity-10 text-success rounded-3 p-2.5 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;">
                <i class="bi bi-check-circle-fill fs-4"></i>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Layout Rows -->
      <div class="row g-4">
        <!-- Recent Tasks Table (Col-lg-8) -->
        <div class="col-12 col-lg-8">
          <div class="card border-0 shadow-sm p-4 rounded-3 h-100">
            <div class="d-flex align-items-center justify-content-between mb-3">
              <h4 class="fw-bold text-body h5 mb-0">Công việc mới nhất</h4>
              <router-link to="/tasks" class="btn btn-sm btn-link text-primary fw-semibold text-decoration-none p-0">Xem tất cả</router-link>
            </div>
            
            <div v-if="allTasks.length === 0" class="text-center py-5 border border-dashed rounded-3 text-muted">
              <i class="bi bi-clipboard-x fs-2 opacity-50"></i>
              <p class="small mt-2 mb-0">Chưa có công việc nào được tạo.</p>
            </div>

            <div v-else class="table-responsive">
              <table class="table table-hover align-middle border-0 mb-0">
                <thead class="table-light">
                  <tr>
                    <th scope="col" class="border-0 rounded-start">Tiêu đề</th>
                    <th scope="col" class="border-0">Dự án</th>
                    <th scope="col" class="border-0">Hạn chót</th>
                    <th scope="col" class="border-0 rounded-end">Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="task in recentTasks" :key="task.Id" class="border-bottom">
                    <td>
                      <div class="fw-semibold text-body text-truncate" style="max-width: 250px;">{{ task.Title }}</div>
                    </td>
                    <td>
                      <span class="small text-muted">{{ task.projectName }}</span>
                    </td>
                    <td>
                      <span class="small" :class="isOverdue(task) ? 'text-danger fw-bold' : 'text-muted'">
                        {{ task.Deadline ? formatDateShort(task.Deadline) : '—' }}
                      </span>
                    </td>
                    <td>
                      <span class="badge text-uppercase font-monospace" :class="getStatusBadgeClass(task.Status)" style="font-size: 9px; padding: 4px 8px;">
                        {{ getStatusLabel(task.Status) }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- Projects Progress List (Col-lg-4) -->
        <div class="col-12 col-lg-4">
          <div class="card border-0 shadow-sm p-4 rounded-3 h-100">
            <h4 class="fw-bold text-body h5 mb-3">Tiến độ dự án</h4>

            <div v-if="projectProgressList.length === 0" class="text-center py-5 text-muted">
              <p class="small mb-0">Chưa có dự án nào có tiến độ.</p>
            </div>

            <div v-else class="d-flex flex-column gap-3">
              <div 
                v-for="proj in projectProgressList" 
                :key="proj.projId" 
                class="border rounded-3 p-3"
              >
                <div class="d-flex align-items-center justify-content-between mb-2">
                  <div class="fw-semibold text-body text-truncate" style="max-width: 180px;">{{ proj.projName }}</div>
                  <span class="badge text-uppercase font-monospace" :class="getRoleBadgeClass(proj.role)" style="font-size: 8px; padding: 2px 4px;">{{ proj.role }}</span>
                </div>
                <div class="text-muted small text-truncate mb-3" style="font-size: 11px;">
                  {{ proj.projDesc || 'Không có mô tả.' }}
                </div>
                <div class="d-flex align-items-center gap-2">
                  <div class="progress flex-grow-1" style="height: 6px;">
                    <div 
                      class="progress-bar bg-primary" 
                      role="progressbar" 
                      :style="{ width: proj.percent + '%' }" 
                      :aria-valuenow="proj.percent" 
                      aria-valuemin="0" 
                      aria-valuemax="100"
                    ></div>
                  </div>
                  <span class="fw-bold text-body small" style="font-size: 11px; min-width: 30px;">{{ proj.percent }}%</span>
                </div>
                <div class="d-flex align-items-center justify-content-between mt-2" style="font-size: 10px; color: #94a3b8;">
                  <span>{{ proj.total }} Tasks</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { getProjects, getProjectTasks } from '../services/projectService.js'
import { projectStore } from '../utils/projectStore.js'
import { toastError } from '../utils/swal.js'

const loading = ref(false)
const allTasks = ref([])
const projectProgressList = ref([])

const loadDashboardData = async () => {
  loading.value = true
  try {
    // 1. Load projects list
    let list = projectStore.projects
    if (list.length === 0) {
      const res = await getProjects()
      list = res?.data || []
      projectStore.setProjects(list)
    }

    // 2. Fetch tasks for each project
    const tasksPromise = list.map(async (proj) => {
      try {
        const res = await getProjectTasks(proj.Id)
        const projectTasks = res?.data || []
        const completedTasks = projectTasks.filter(t => t.Status === 'Done' || t.Status === 'Closed').length
        const total = projectTasks.length
        const percent = total > 0 ? Math.round((completedTasks / total) * 100) : 0
        return {
          projId: proj.Id,
          projName: proj.Name,
          projDesc: proj.Description,
          role: proj.UserRole,
          total,
          percent,
          tasks: projectTasks
        }
      } catch (err) {
        console.error('Error fetching tasks for project ' + proj.Id, err)
        return null
      }
    })

    const results = await Promise.all(tasksPromise)
    const filteredResults = results.filter(r => r !== null)
    projectProgressList.value = filteredResults

    // 3. Compile all tasks
    const tempTasks = []
    filteredResults.forEach(r => {
      r.tasks.forEach(t => {
        tempTasks.push({
          ...t,
          projectName: r.projName
        })
      })
    })
    allTasks.value = tempTasks
  } catch (error) {
    console.error('Error loading dashboard data', error)
    toastError('Không thể tải dữ liệu dashboard.')
  } finally {
    loading.value = false
  }
}

const stats = reactive({
  totalProjects: computed(() => projectStore.projects.length),
  totalTasks: computed(() => allTasks.value.length),
  inProgressTasks: computed(() => allTasks.value.filter(t => t.Status === 'InProgress').length),
  completedTasks: computed(() => allTasks.value.filter(t => t.Status === 'Done' || t.Status === 'Closed').length)
})

const recentTasks = computed(() => {
  return [...allTasks.value]
    .sort((a, b) => new Date(b.CreatedAt) - new Date(a.CreatedAt))
    .slice(0, 5)
})

const isOverdue = (task) => {
  if (!task.Deadline || task.Status === 'Done' || task.Status === 'Closed') return false
  return new Date(task.Deadline) < new Date()
}

const getStatusBadgeClass = (status) => {
  switch (status) {
    case 'ToDo':
      return 'bg-secondary-subtle text-secondary border border-secondary-subtle'
    case 'InProgress':
      return 'bg-primary-subtle text-primary border border-primary-subtle'
    case 'Done':
      return 'bg-success-subtle text-success border border-success-subtle'
    case 'Closed':
      return 'bg-warning-subtle text-warning border border-warning-subtle'
    default:
      return 'bg-light text-dark'
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'ToDo': return 'To Do'
    case 'InProgress': return 'In Progress'
    case 'Done': return 'Done'
    case 'Closed': return 'Closed'
    default: return status
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

const formatDateShort = (d) => d ? new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'

onMounted(() => {
  loadDashboardData()
})
</script>

<style scoped>
.page-title {
  font-size: 1.68rem;
  letter-spacing: -0.02em;
}
.bg-indigo-light {
  background-color: rgba(99, 102, 241, 0.1);
}
.text-indigo {
  color: #6366f1;
}
.border-dashed {
  border-style: dashed !important;
}
</style>