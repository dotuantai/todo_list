<template>
  <div class="p-3 p-md-4 d-flex flex-column h-100">
    <!-- Header -->
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4" v-if="projectStore.currentProjectId">
      <div class="d-flex align-items-center gap-2">
        <button class="btn btn-outline-secondary btn-sm rounded-3 shadow-sm" style="width: 32px; height: 32px;" @click="prevMonth"><i class="bi bi-chevron-left"></i></button>
        <button class="btn btn-outline-secondary btn-sm rounded-3 shadow-sm" style="width: 32px; height: 32px;" @click="nextMonth"><i class="bi bi-chevron-right"></i></button>
        <h4 class="mb-0 fw-bold mx-2 text-body" style="font-size: 1.25rem;">{{ $t('calendar.SCR1001', { month: currentMonth + 1, year: currentYear }) }}</h4>
        <button class="btn btn-outline-secondary btn-sm rounded-pill px-3 shadow-sm fw-medium" @click="goToToday">{{ $t('calendar.SCR1002') }}</button>
      </div>
      <div class="d-flex align-items-center gap-3">
        <div class="d-flex flex-column align-items-end lh-1">
          <span class="text-primary fw-bold" style="font-size: 1.5rem;">{{ filteredTasks.length }}</span>
          <span class="text-muted" style="font-size: 0.75rem;">{{ $t('calendar.SCR1003') }}</span>
        </div>
      </div>
    </div>

    <!-- Filter bar -->
    <div class="d-flex flex-wrap align-items-center gap-2 mb-3">
      <span class="badge bg-secondary-subtle text-secondary me-2 py-2 px-3 rounded-pill border"><i class="bi bi-funnel"></i> {{ $t('calendar.SCR1004') }}</span>
      
      <span class="badge rounded-pill px-3 py-2 border shadow-sm transition-all" 
            style="cursor: pointer; font-size: 0.8rem;" 
            :class="filterStatus === null ? 'bg-body text-primary border-primary' : 'bg-body text-secondary'" 
            @click="filterStatus = null">
        {{ $t('calendar.SCR1005') }}
      </span>
      <span v-for="col in columns" :key="col.Id" 
            class="badge rounded-pill px-3 py-2 border shadow-sm transition-all"
            :style="{ 
              background: filterStatus === col.Id ? col.bgLight : 'var(--bs-body-bg)', 
              color: filterStatus === col.Id ? col.color : 'var(--bs-secondary-color)',
              borderColor: filterStatus === col.Id ? col.color : 'var(--bs-border-color)'
            }"
            style="cursor: pointer; font-size: 0.8rem;"
            @click="filterStatus = col.Id">
        <span class="d-inline-block rounded-circle me-1" :style="{ background: col.color, width: '8px', height: '8px' }"></span>
        {{ col.Name }}
      </span>
      <div class="ms-auto">
        <button 
          class="btn rounded-circle d-flex align-items-center justify-content-center transition-all shadow-sm border"
          :class="filterMyTasks ? 'text-white border-0' : 'btn-outline-secondary text-secondary bg-body'"
          :style="filterMyTasks ? 'background-color: #1a8e9e; width: 36px; height: 36px;' : 'width: 36px; height: 36px;'"
          @click="filterMyTasks = !filterMyTasks"
          :title="$t('calendar.SCR1006')"
        >
          <i class="bi bi-person-fill fs-5"></i>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex-grow-1 d-flex justify-content-center align-items-center">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">{{ $t('calendar.SCR1007') }}</span>
      </div>
    </div>

    <!-- Calendar Grid -->
    <div v-else class="calendar-wrapper flex-grow-1 bg-body rounded-4 shadow-sm border overflow-hidden d-flex flex-column">
      <!-- Weekdays Header -->
      <div class="calendar-header d-flex border-bottom text-center py-3 text-muted fw-bold text-uppercase bg-body-tertiary" style="font-size: 0.8rem; letter-spacing: 1px;">
        <div class="flex-grow-1" style="flex-basis: 0;">{{ $t('calendar.SCR1008') }}</div>
        <div class="flex-grow-1" style="flex-basis: 0;">{{ $t('calendar.SCR1009') }}</div>
        <div class="flex-grow-1" style="flex-basis: 0;">{{ $t('calendar.SCR1010') }}</div>
        <div class="flex-grow-1" style="flex-basis: 0;">{{ $t('calendar.SCR1011') }}</div>
        <div class="flex-grow-1" style="flex-basis: 0;">{{ $t('calendar.SCR1012') }}</div>
        <div class="flex-grow-1" style="flex-basis: 0;">{{ $t('calendar.SCR1013') }}</div>
        <div class="flex-grow-1 text-danger" style="flex-basis: 0;">{{ $t('calendar.SCR1014') }}</div>
      </div>
      
      <!-- Days Grid -->
      <div class="calendar-body flex-grow-1 d-flex flex-column">
        <div class="calendar-row d-flex flex-grow-1 border-bottom" v-for="(week, wIdx) in calendarGrid" :key="wIdx">
          <div class="calendar-cell flex-grow-1 border-end position-relative p-2 transition-all hover-bg" 
               v-for="(day, dIdx) in week" :key="dIdx" 
               :class="{
                 'bg-light text-muted opacity-50': !day.isCurrentMonth, 
                 'bg-primary-subtle border-primary border-opacity-50': isToday(day.date)
               }"
               style="flex-basis: 0; min-height: 120px; min-width: 0;">
            
            <div class="d-flex justify-content-between align-items-center mb-2 px-1">
              <span class="fw-bold rounded-circle d-flex align-items-center justify-content-center" 
                    :class="{'bg-primary text-white': isToday(day.date), 'text-body': !isToday(day.date)}" 
                    style="font-size: 0.9rem; width: 28px; height: 28px;">
                {{ day.date.getDate() }}
              </span>
            </div>

            <!-- Task Pills -->
            <div class="task-list custom-scrollbar pe-1" style="max-height: 85px; overflow-y: auto;">
              <div v-for="task in getTasksForDate(day.date)" :key="task.Id"
                   class="task-pill rounded-3 px-2 py-1 mb-1 text-truncate text-white fw-medium shadow-sm transition-all"
                   :style="{ background: getColumnColor(task.ColumnId), fontSize: '0.75rem', cursor: 'pointer' }"
                   @click.stop="openTaskDetails(task)"
                   :title="task.Title">
                <i v-if="task.Priority === 'High'" class="bi bi-exclamation-circle-fill me-1" style="font-size: 0.65rem;"></i>
                {{ task.Title }}
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useProjectStore } from '../stores/projectStore.js'
import { getProjectTasks, getProjectColumns } from '../services/projectService.js'
import Swal from 'sweetalert2'
import { useI18n } from 'vue-i18n'

const projectStore = useProjectStore()
const { t, locale } = useI18n()
const currentDate = ref(new Date())
const tasks = ref([])
const columns = ref([])
const filterStatus = ref(null)
const filterMyTasks = ref(false)
const loading = ref(false)

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const filteredTasks = computed(() => {
  if (filterStatus.value === null) return tasks.value
  return tasks.value.filter(t => t.ColumnId === filterStatus.value)
})

const isToday = (date) => {
  const today = new Date()
  return date.getDate() === today.getDate() &&
         date.getMonth() === today.getMonth() &&
         date.getFullYear() === today.getFullYear()
}

const prevMonth = () => {
  currentDate.value = new Date(currentYear.value, currentMonth.value - 1, 1)
}

const nextMonth = () => {
  currentDate.value = new Date(currentYear.value, currentMonth.value + 1, 1)
}

const goToToday = () => {
  currentDate.value = new Date()
}

const calendarGrid = computed(() => {
  const year = currentYear.value
  const month = currentMonth.value
  
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  
  const grid = []
  let currentWeek = []
  
  let startDayOfWeek = firstDay.getDay() - 1
  if (startDayOfWeek === -1) startDayOfWeek = 6 // Sunday is 6
  
  const prevMonthLastDay = new Date(year, month, 0).getDate()
  for (let i = startDayOfWeek - 1; i >= 0; i--) {
    currentWeek.push({
      date: new Date(year, month - 1, prevMonthLastDay - i),
      isCurrentMonth: false
    })
  }
  
  for (let i = 1; i <= lastDay.getDate(); i++) {
    currentWeek.push({
      date: new Date(year, month, i),
      isCurrentMonth: true
    })
    if (currentWeek.length === 7) {
      grid.push(currentWeek)
      currentWeek = []
    }
  }
  
  if (currentWeek.length > 0) {
    let nextMonthDay = 1
    while (currentWeek.length < 7) {
      currentWeek.push({
        date: new Date(year, month + 1, nextMonthDay++),
        isCurrentMonth: false
      })
    }
    grid.push(currentWeek)
  }
  
  return grid
})

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
  } catch(e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const getTasksForDate = (date) => {
  return filteredTasks.value.filter(t => {
    if (filterMyTasks.value) {
      if (!t.AssignedUsers || !t.AssignedUsers.some(u => u.UserId === projectStore.currentUserId)) {
        return false
      }
    }
    if (!t.Deadline) return false
    const d = new Date(t.Deadline)
    return d.getDate() === date.getDate() &&
           d.getMonth() === date.getMonth() &&
           d.getFullYear() === date.getFullYear()
  })
}

const getColumnColor = (colId) => {
  const col = columns.value.find(c => c.Id === colId)
  if (!col) return 'var(--bs-primary)'
  return col.color
}

const formatPriority = (priority) => ({
  Low: t('tasks.SCR0224'),
  Medium: t('tasks.SCR0225'),
  High: t('tasks.SCR0226')
})[priority] || priority

const openTaskDetails = (task) => {
  const col = columns.value.find(c => c.Id === task.ColumnId)
  Swal.fire({
    title: task.Title,
    html: `
      <div class="text-start mt-3">
        <div class="mb-2"><span class="badge" style="background: ${col?.color || 'gray'}">${col?.Name || t('calendar.SCR1015')}</span>
        ${task.Priority ? `<span class="badge bg-secondary ms-1">${t('calendar.SCR1016', { priority: formatPriority(task.Priority) })}</span>` : ''}</div>
        <div class="mt-3 text-muted" style="white-space: pre-wrap; font-size: 0.9rem;">${task.Description || t('calendar.SCR1017')}</div>
        <hr>
        <div class="d-flex justify-content-between small text-muted">
          <span><strong>${t('calendar.SCR1018')}</strong> ${task.Deadline ? new Date(task.Deadline).toLocaleDateString(locale.value === 'vi' ? 'vi-VN' : 'en-US') : t('calendar.SCR1019')}</span>
          <span><strong>${t('calendar.SCR1020')}</strong> ${task.EstimatedHours || 0}h</span>
        </div>
      </div>
    `,
    showConfirmButton: true,
    confirmButtonText: t('calendar.SCR1021'),
    confirmButtonColor: 'var(--bs-primary)',
    width: '500px'
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
.transition-all {
  transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
}
.hover-bg:hover {
  background-color: var(--bs-secondary-bg) !important;
}
.task-pill:hover {
  transform: translateY(-1px);
  filter: brightness(1.1);
}
.calendar-row:last-child {
  border-bottom: none !important;
}
.calendar-row .calendar-cell:last-child {
  border-right: none !important;
}
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(0, 0, 0, 0.1);
  border-radius: 4px;
}
[data-bs-theme="dark"] .custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(255, 255, 255, 0.1);
}
</style>
