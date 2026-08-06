<template>
  <div class="p-3 p-md-4 d-flex flex-column h-100">
    <!-- Header -->
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4" v-if="projectStore.currentProjectId">
      <div class="d-flex align-items-center gap-2">
        <h4 class="mb-0 fw-bold text-body" style="font-size: 1.5rem; font-family: 'Inter', sans-serif; letter-spacing: -0.02em;">Sơ đồ Gantt</h4>
        <span class="badge bg-secondary-subtle text-secondary ms-2 rounded-pill px-3">{{ filteredTasks.length }} nhiệm vụ</span>
      </div>
      <div class="d-flex align-items-center gap-2">
        <button class="btn btn-outline-secondary btn-sm rounded-pill px-4 fw-medium shadow-sm" @click="scrollToToday">
          <i class="bi bi-crosshair me-1"></i> Hôm nay
        </button>
      </div>
    </div>

    <!-- Filter bar -->
    <div class="d-flex flex-wrap align-items-center gap-2 mb-3" style="font-family: 'Inter', sans-serif;">
      <span class="badge bg-secondary-subtle text-secondary me-2 py-2 px-3 rounded-pill border"><i class="bi bi-funnel"></i> DỰ ÁN</span>
      
      <span class="badge rounded-pill px-4 py-2 border shadow-sm transition-all" 
            style="cursor: pointer; font-size: 0.85rem;" 
            :class="filterStatus === null ? 'bg-body text-primary border-primary' : 'bg-body text-secondary'" 
            @click="filterStatus = null">
        Tất cả
      </span>
      <span v-for="col in columns" :key="col.Id" 
            class="badge rounded-pill px-4 py-2 border shadow-sm transition-all"
            :style="{ 
              background: filterStatus === col.Id ? col.bgLight : 'var(--bs-body-bg)', 
              color: filterStatus === col.Id ? col.color : 'var(--bs-secondary-color)',
              borderColor: filterStatus === col.Id ? col.color : 'var(--bs-border-color)'
            }"
            style="cursor: pointer; font-size: 0.85rem;"
            @click="filterStatus = col.Id">
        <span class="d-inline-block rounded-circle me-2" :style="{ background: col.color, width: '8px', height: '8px' }"></span>
        {{ col.Name }}
      </span>

      <div class="ms-auto">
        <button 
          class="btn rounded-circle d-flex align-items-center justify-content-center transition-all shadow-sm border"
          :class="filterMyTasks ? 'text-white border-0' : 'btn-outline-secondary text-secondary bg-body'"
          :style="filterMyTasks ? 'background-color: #1a8e9e; width: 36px; height: 36px;' : 'width: 36px; height: 36px;'"
          @click="filterMyTasks = !filterMyTasks"
          title="Assign to myself"
        >
          <i class="bi bi-person-fill fs-5"></i>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex-grow-1 d-flex justify-content-center align-items-center">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <!-- Gantt Chart Container -->
    <div v-else class="gantt-container flex-grow-1 bg-body rounded-4 shadow-sm border overflow-hidden d-flex" style="font-family: 'Inter', sans-serif;">
      
      <!-- Left Sidebar: Task Names -->
      <div class="gantt-sidebar border-end d-flex flex-column bg-body-tertiary" style="width: 280px; flex-shrink: 0; z-index: 20;">
        <div class="gantt-header p-3 border-bottom d-flex align-items-center fw-bold text-muted text-uppercase tracking-wider bg-body-secondary" style="height: 65px; font-size: 0.75rem;">
          Tên công việc
        </div>
        <div class="gantt-rows overflow-hidden d-flex flex-column flex-grow-1" ref="sidebarRows" @scroll="syncScroll('sidebar')">
          <div v-for="task in filteredTasks" :key="task.Id" 
               class="p-2 px-3 border-bottom text-truncate d-flex flex-column justify-content-center bg-body hover-bg transition-all" 
               style="height: 60px; cursor: pointer;" 
               :title="task.Title"
               @click="openTaskDetails(task)">
            <span class="fw-semibold text-body" style="font-size: 0.9rem;">{{ task.Title }}</span>
            <span class="text-muted mt-1" style="font-size: 0.7rem;">
              <i class="bi bi-clock"></i> {{ getTaskDuration(task) }} ngày
            </span>
          </div>
        </div>
      </div>
      
      <!-- Right Timeline -->
      <div class="gantt-timeline overflow-auto flex-grow-1 position-relative" ref="timelineContainer" @scroll="syncScroll('timeline')">
        <div class="gantt-grid" :style="{ display: 'grid', gridTemplateColumns: `repeat(${timelineDays.length}, minmax(45px, 1fr))` }">
          
          <!-- Timeline Header -->
          <div class="gantt-header-row position-sticky top-0 bg-body-secondary border-bottom" style="grid-column: 1 / -1; display: grid; grid-template-columns: subgrid; height: 65px; z-index: 10;">
            <div v-for="(day, idx) in timelineDays" :key="idx" 
                 class="border-end d-flex flex-column align-items-center justify-content-center transition-all" 
                 :class="{'bg-primary-subtle text-primary border-primary border-opacity-50': isToday(day)}">
              <span class="text-muted fw-semibold" style="font-size: 0.65rem; text-transform: uppercase;">{{ getDayName(day) }}</span>
              <span class="fw-bold" style="font-size: 1rem;">{{ day.getDate() }}</span>
              <span class="text-muted" style="font-size: 0.6rem;" v-if="day.getDate() === 1">{{ day.getMonth() + 1 }}/{{ day.getFullYear() }}</span>
            </div>
          </div>
          
          <!-- Timeline Rows -->
          <div class="gantt-task-row position-relative" v-for="(task, index) in filteredTasks" :key="task.Id" 
               style="grid-column: 1 / -1; display: grid; grid-template-columns: subgrid; height: 60px;">
            
            <!-- Grid vertical lines -->
            <div v-for="(day, idx) in timelineDays" :key="'grid-'+index+'-'+idx" class="border-end border-bottom" 
                 :class="{'bg-light opacity-25': !isWeekday(day), 'bg-primary-subtle opacity-10': isToday(day)}"></div>
            
            <!-- Task Bar -->
            <div class="task-bar-wrapper position-absolute top-50 translate-middle-y w-100 px-1" 
                 :style="{ 
                   gridColumn: `${getTaskOffset(task) + 1} / span ${getTaskDuration(task)}`,
                   height: '36px',
                   zIndex: 5
                 }">
               <div class="task-bar h-100 rounded-pill shadow-sm d-flex align-items-center px-3 text-white text-truncate fw-medium transition-all"
                    :style="{ background: getColumnColor(task.ColumnId), fontSize: '0.8rem', cursor: 'pointer' }"
                    @click="openTaskDetails(task)">
                 <span class="text-truncate">{{ task.Title }}</span>
               </div>
            </div>
          </div>
          
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useProjectStore } from '../stores/projectStore.js'
import { getProjectTasks, getProjectColumns } from '../services/projectService.js'
import Swal from 'sweetalert2'

const projectStore = useProjectStore()
const tasks = ref([])
const columns = ref([])
const filterStatus = ref(null)
const filterMyTasks = ref(false)
const loading = ref(false)

const timelineContainer = ref(null)
const sidebarRows = ref(null)

let isSyncingLeft = false;
let isSyncingRight = false;

const syncScroll = (source) => {
  if (!timelineContainer.value || !sidebarRows.value) return;
  
  if (source === 'sidebar') {
    if (!isSyncingLeft) {
      isSyncingRight = true;
      timelineContainer.value.scrollTop = sidebarRows.value.scrollTop;
    }
    isSyncingLeft = false;
  } else {
    if (!isSyncingRight) {
      isSyncingLeft = true;
      sidebarRows.value.scrollTop = timelineContainer.value.scrollTop;
    }
    isSyncingRight = false;
  }
}

const filteredTasks = computed(() => {
  let result = tasks.value
  if (filterStatus.value !== null) {
    result = result.filter(t => t.ColumnId === filterStatus.value)
  }
  if (filterMyTasks.value) {
    result = result.filter(t => t.AssignedUsers && t.AssignedUsers.some(u => u.UserId === projectStore.currentUserId))
  }
  return result
})

const isToday = (date) => {
  const today = new Date()
  return date.getDate() === today.getDate() &&
         date.getMonth() === today.getMonth() &&
         date.getFullYear() === today.getFullYear()
}

const isWeekday = (date) => {
  const day = date.getDay()
  return day !== 0 && day !== 6
}

const getDayName = (date) => {
  const days = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7']
  return days[date.getDay()]
}

// Calculate the timeline boundaries based on tasks
const timelineStartDate = computed(() => {
  if (!tasks.value.length) return new Date(new Date().getFullYear(), new Date().getMonth(), 1)
  let min = new Date()
  tasks.value.forEach(t => {
    const d = new Date(t.StartDate || t.CreatedAt || new Date())
    if (d < min) min = d
  })
  // Pad with 7 days before
  const startDate = new Date(min)
  startDate.setDate(startDate.getDate() - 7)
  return startDate
})

const timelineEndDate = computed(() => {
  if (!tasks.value.length) return new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)
  let max = new Date()
  tasks.value.forEach(t => {
    const d = new Date(t.Deadline || t.StartDate || t.CreatedAt || new Date())
    if (d > max) max = d
  })
  // Pad with 14 days after
  const endDate = new Date(max)
  endDate.setDate(endDate.getDate() + 14)
  return endDate
})

const timelineDays = computed(() => {
  const days = []
  let current = new Date(timelineStartDate.value)
  const end = timelineEndDate.value
  
  // Prevent infinite loops just in case
  let safeguard = 0
  while (current <= end && safeguard < 1000) {
    days.push(new Date(current))
    current.setDate(current.getDate() + 1)
    safeguard++
  }
  return days
})

const getTaskStartEndDates = (task) => {
  let start = new Date(task.StartDate || task.CreatedAt || new Date())
  let end = new Date(task.Deadline || task.StartDate || task.CreatedAt || new Date())
  
  // Ensure start is before end
  if (start > end) {
    const temp = start
    start = end
    end = temp
  }
  
  // Normalize time to midnight for accurate day counting
  start.setHours(0,0,0,0)
  end.setHours(0,0,0,0)
  return { start, end }
}

const getTaskOffset = (task) => {
  const { start } = getTaskStartEndDates(task)
  const index = timelineDays.value.findIndex(d => 
    d.getDate() === start.getDate() && 
    d.getMonth() === start.getMonth() && 
    d.getFullYear() === start.getFullYear()
  )
  return index === -1 ? 0 : index
}

const getTaskDuration = (task) => {
  const { start, end } = getTaskStartEndDates(task)
  const diffTime = Math.abs(end - start)
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return diffDays === 0 ? 1 : diffDays + 1
}

const scrollToToday = () => {
  if (!timelineContainer.value) return
  const today = new Date()
  const index = timelineDays.value.findIndex(d => 
    d.getDate() === today.getDate() && 
    d.getMonth() === today.getMonth() && 
    d.getFullYear() === today.getFullYear()
  )
  if (index !== -1) {
    // 45px per column + padding
    const scrollPos = index * 45 - (timelineContainer.value.clientWidth / 2)
    timelineContainer.value.scrollTo({ left: Math.max(0, scrollPos), behavior: 'smooth' })
  }
}

const loadData = async () => {
  if (!projectStore.currentProjectId) return
  loading.value = true
  try {
    const colRes = await getProjectColumns(projectStore.currentProjectId)
    const allCols = colRes?.data || []
    
    allCols.forEach(col => {
      if (col.IsCompletedStage) {
         col.color = 'var(--status-done-color, #10b981)'
         col.bgLight = 'var(--status-done-bg-light, #d1fae5)'
      } else if (col.Order === 0) {
         col.color = 'var(--status-todo-color, #6b7280)'
         col.bgLight = 'var(--status-todo-bg-light, #f3f4f6)'
      } else {
         col.color = 'var(--status-inprogress-color, #3b82f6)'
         col.bgLight = 'var(--status-inprogress-bg-light, #dbeafe)'
      }
    })
    columns.value = allCols

    const taskRes = await getProjectTasks(projectStore.currentProjectId, null, 1, 1000)
    tasks.value = taskRes?.data?.Items || []
    
    // Sort tasks by start date
    tasks.value.sort((a, b) => {
      const startA = new Date(a.StartDate || a.CreatedAt || new Date())
      const startB = new Date(b.StartDate || b.CreatedAt || new Date())
      return startA - startB
    })

    nextTick(() => {
      scrollToToday()
    })
  } catch(e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const getColumnColor = (colId) => {
  const col = columns.value.find(c => c.Id === colId)
  if (!col) return 'var(--bs-primary)'
  return col.color
}

const openTaskDetails = (task) => {
  const col = columns.value.find(c => c.Id === task.ColumnId)
  Swal.fire({
    title: `<div style="font-family: 'Inter', sans-serif;">${task.Title}</div>`,
    html: `
      <div class="text-start mt-3" style="font-family: 'Inter', sans-serif;">
        <div class="mb-3">
          <span class="badge rounded-pill px-3 py-2 text-white" style="background: ${col?.color || 'gray'}">${col?.Name || 'Unknown'}</span>
          ${task.Priority ? `<span class="badge bg-secondary-subtle text-secondary rounded-pill px-3 py-2 ms-2">${task.Priority} Priority</span>` : ''}
        </div>
        <div class="mt-3 text-body bg-body-secondary p-3 rounded-4" style="white-space: pre-wrap; font-size: 0.95rem; line-height: 1.6;">${task.Description || 'No description provided.'}</div>
        <div class="row mt-4 pt-3 border-top g-3">
          <div class="col-6">
            <label class="text-muted small text-uppercase fw-bold mb-1">Start Date</label>
            <div class="fw-medium">${task.StartDate ? new Date(task.StartDate).toLocaleDateString() : new Date(task.CreatedAt).toLocaleDateString()}</div>
          </div>
          <div class="col-6">
            <label class="text-muted small text-uppercase fw-bold mb-1">Deadline</label>
            <div class="fw-medium text-danger">${task.Deadline ? new Date(task.Deadline).toLocaleDateString() : 'None'}</div>
          </div>
        </div>
      </div>
    `,
    showConfirmButton: true,
    confirmButtonText: 'Đóng',
    confirmButtonColor: 'var(--bs-primary)',
    width: '600px',
    customClass: {
      popup: 'rounded-4 shadow-lg border-0'
    }
  })
}

onMounted(() => {
  loadData()
})

watch(() => projectStore.currentProjectId, () => {
  loadData()
})

</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;900&display=swap');

.transition-all {
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}
.hover-bg:hover {
  background-color: var(--bs-secondary-bg) !important;
}
.task-bar {
  opacity: 0.95;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06) !important;
}
.task-bar:hover {
  transform: translateY(-2px);
  filter: brightness(1.1);
  opacity: 1;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05) !important;
}
.tracking-wider {
  letter-spacing: 0.05em;
}

/* Custom Scrollbar for Timeline */
.gantt-timeline::-webkit-scrollbar,
.gantt-sidebar .gantt-rows::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
.gantt-timeline::-webkit-scrollbar-track,
.gantt-sidebar .gantt-rows::-webkit-scrollbar-track {
  background: transparent;
}
.gantt-timeline::-webkit-scrollbar-thumb,
.gantt-sidebar .gantt-rows::-webkit-scrollbar-thumb {
  background-color: rgba(156, 163, 175, 0.5);
  border-radius: 10px;
}
.gantt-timeline::-webkit-scrollbar-thumb:hover,
.gantt-sidebar .gantt-rows::-webkit-scrollbar-thumb:hover {
  background-color: rgba(107, 114, 128, 0.8);
}
</style>
