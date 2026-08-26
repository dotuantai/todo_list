<template>
  <div>
    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">{{ $t('admin.SCR0922') }}</p>
    </div>

    <div v-else-if="stats" class="row g-4">
      <!-- Summary Cards -->
      <div class="col-12 col-sm-6 col-xl-3">
        <div class="dashboard-card card border-0 rounded-4 overflow-hidden h-100" style="background-color: #F0F9FF; box-shadow: 0 4px 15px rgba(0,0,0,0.02);">
          <div class="card-body p-4 d-flex flex-column h-100">
            <div class="d-flex align-items-start gap-3 mb-4">
              <div class="bg-white text-primary rounded-3 d-flex align-items-center justify-content-center shadow-sm" style="width: 48px; height: 48px;">
                <i class="bi bi-clipboard2-check-fill fs-5"></i>
              </div>
              <div>
                <h6 class="fw-semibold text-secondary mb-1" style="font-size: 0.85rem;">{{ $t('admin.SCR0976') }}</h6>
                <h2 class="fw-bold mb-0 text-dark lh-1" style="font-size: 2rem;">{{ stats.TotalTasks }}</h2>
              </div>
            </div>
            <div class="mt-auto pt-3 border-top d-flex justify-content-between align-items-center" style="border-color: rgba(2, 132, 199, 0.1) !important;">
              <span class="text-primary small fw-medium">{{ $t('admin.SCR0977', { percent: Math.round((stats.TaskStatusDistribution?.Done / (stats.TotalTasks || 1)) * 100) }) }}</span>
              <router-link to="/admin/projects" class="text-primary small fw-bold text-decoration-none">{{ $t('admin.SCR0978') }}</router-link>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 col-sm-6 col-xl-3">
        <div class="dashboard-card card border-0 rounded-4 overflow-hidden h-100" style="background-color: #FEF9C3; box-shadow: 0 4px 15px rgba(0,0,0,0.02);">
          <div class="card-body p-4 d-flex flex-column h-100">
            <div class="d-flex align-items-start gap-3 mb-4">
              <div class="bg-white text-warning rounded-3 d-flex align-items-center justify-content-center shadow-sm" style="width: 48px; height: 48px;">
                <i class="bi bi-clock-fill fs-5"></i>
              </div>
              <div>
                <h6 class="fw-semibold text-secondary mb-1" style="font-size: 0.85rem;">{{ $t('admin.SCR0979') }}</h6>
                <h2 class="fw-bold mb-0 text-dark lh-1" style="font-size: 2rem;">{{ stats.TaskStatusDistribution?.InProgress || 0 }}</h2>
              </div>
            </div>
            <div class="mt-auto pt-3 border-top d-flex justify-content-between align-items-center" style="border-color: rgba(202, 138, 4, 0.1) !important;">
              <span class="text-warning small fw-medium" style="color: #CA8A04 !important;"><i class="bi bi-activity"></i> {{ $t('admin.SCR0980') }}</span>
              <router-link to="/admin/projects" class="text-warning small fw-bold text-decoration-none" style="color: #CA8A04 !important;">{{ $t('admin.SCR0978') }}</router-link>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 col-sm-6 col-xl-3">
        <div class="dashboard-card card border-0 rounded-4 overflow-hidden h-100" style="background-color: #DCFCE7; box-shadow: 0 4px 15px rgba(0,0,0,0.02);">
          <div class="card-body p-4 d-flex flex-column h-100">
            <div class="d-flex align-items-start gap-3 mb-4">
              <div class="bg-white text-success rounded-3 d-flex align-items-center justify-content-center shadow-sm" style="width: 48px; height: 48px;">
                <i class="bi bi-check-circle-fill fs-5"></i>
              </div>
              <div>
                <h6 class="fw-semibold text-secondary mb-1" style="font-size: 0.85rem;">{{ $t('admin.SCR0981') }}</h6>
                <h2 class="fw-bold mb-0 text-dark lh-1" style="font-size: 2rem;">{{ stats.TaskStatusDistribution?.Done || 0 }}</h2>
              </div>
            </div>
            <div class="mt-auto pt-3 border-top d-flex justify-content-between align-items-center" style="border-color: rgba(22, 163, 74, 0.1) !important;">
              <span class="text-success small fw-medium"><i class="bi bi-check2-all"></i> {{ $t('admin.SCR0982') }}</span>
              <router-link to="/admin/projects" class="text-success small fw-bold text-decoration-none">{{ $t('admin.SCR0978') }}</router-link>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 col-sm-6 col-xl-3">
        <div class="dashboard-card card border-0 rounded-4 overflow-hidden h-100" style="background-color: #FEE2E2; box-shadow: 0 4px 15px rgba(0,0,0,0.02);">
          <div class="card-body p-4 d-flex flex-column h-100">
            <div class="d-flex align-items-start gap-3 mb-4">
              <div class="bg-white text-danger rounded-3 d-flex align-items-center justify-content-center shadow-sm" style="width: 48px; height: 48px;">
                <i class="bi bi-exclamation-triangle-fill fs-5"></i>
              </div>
              <div>
                <h6 class="fw-semibold text-secondary mb-1" style="font-size: 0.85rem;">{{ $t('admin.SCR0983') }}</h6>
                <h2 class="fw-bold mb-0 text-dark lh-1" style="font-size: 2rem;">0</h2>
              </div>
            </div>
            <div class="mt-auto pt-3 border-top d-flex justify-content-between align-items-center" style="border-color: rgba(220, 38, 38, 0.1) !important;">
              <span class="text-danger small fw-medium"><i class="bi bi-exclamation-circle"></i> {{ $t('admin.SCR0984') }}</span>
              <router-link to="/admin/projects" class="text-danger small fw-bold text-decoration-none">{{ $t('admin.SCR0978') }}</router-link>
            </div>
          </div>
        </div>
      </div>

      <!-- Charts Section -->
      <div class="col-12 col-lg-8">
        <div class="card border-0 rounded-4 h-100" style="box-shadow: 0 4px 15px rgba(0,0,0,0.02);">
          <div class="card-body p-4 p-md-5">
            <div class="d-flex align-items-center justify-content-between mb-1">
              <div class="d-flex align-items-center gap-2">
                <i class="bi bi-bar-chart-fill text-primary"></i>
                <h5 class="fw-bold mb-0 text-dark">{{ $t('admin.SCR0914') }}</h5>
              </div>
              <small class="text-muted d-none d-md-block"><i class="bi bi-arrows-expand"></i> {{ $t('admin.SCR0985') }}</small>
            </div>
            <p class="text-muted small mb-4">{{ $t('admin.SCR0915') }}</p>
            <div class="chart-scroll-wrapper custom-scrollbar">
              <div class="chart-container" :style="{ position: 'relative', height: '320px', minWidth: chartMinWidth }">
                <Bar v-if="!loading" :data="healthChartData" :options="healthChartOptions" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 col-lg-4">
        <div class="card border-0 rounded-4 h-100" style="box-shadow: 0 4px 15px rgba(0,0,0,0.02);">
          <div class="card-body p-4 p-md-5">
            <div class="d-flex align-items-center gap-2 mb-1">
              <i class="bi bi-pie-chart-fill text-success"></i>
              <h6 class="fw-bold text-dark mb-0">{{ $t('admin.SCR0916') }}</h6>
            </div>
            <p class="text-muted small mb-4">{{ $t('admin.SCR0986') }}</p>
            <div class="chart-container d-flex justify-content-center align-items-center" style="position: relative; height: 320px; width: 100%">
              <Doughnut v-if="!loading" :data="doughnutChartData" :options="doughnutChartOptions" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { getAdminDashboardStats } from '../../services/adminService.js'
import { toastError, extractMessage } from '../../utils/swal.js'
import { useI18n } from 'vue-i18n'

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js'
import { Line, Bar, Doughnut } from 'vue-chartjs'
import { useProjectStore } from '../../stores/projectStore.js'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
)

const { t, locale } = useI18n()
const loading = ref(true)
const stats = ref(null)
const projectStore = useProjectStore()

const formattedDate = computed(() => {
  const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }
  return new Intl.DateTimeFormat(locale.value === 'vi' ? 'vi-VN' : 'en-US', options).format(new Date())
})

const chartMinWidth = computed(() => {
  if (!stats.value || !stats.value.ProjectHealthList) return '100%'
  const count = stats.value.ProjectHealthList.length
  const calculatedWidth = count * 90
  return `max(100%, ${calculatedWidth}px)`
})

onMounted(async () => {
  await fetchStats()
})

const fetchStats = async () => {
  loading.value = true
  try {
    const res = await getAdminDashboardStats()
    if (res?.data) {
      stats.value = res.data
    }
  } catch (error) {
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    loading.value = false
  }
}

// Chart Configurations
const healthChartData = computed(() => {
  if (!stats.value || !stats.value.ProjectHealthList) return { labels: [], datasets: [] }
  
  const labels = stats.value.ProjectHealthList.map(p => p.ProjectName)
  
  return {
    labels,
    datasets: [
      {
        label: t('admin.SCR0919'),
        backgroundColor: '#22C55E', // Green
        data: stats.value.ProjectHealthList.map(p => p.Done),
        borderRadius: 4
      },
      {
        label: t('admin.SCR0918'),
        backgroundColor: '#FACC15', // Yellow
        data: stats.value.ProjectHealthList.map(p => p.InProgress),
        borderRadius: 4
      },
      {
        label: t('admin.SCR0917'),
        backgroundColor: '#94A3B8', // Slate (Grey)
        data: stats.value.ProjectHealthList.map(p => p.ToDo),
        borderRadius: 4
      },
      {
        label: t('admin.SCR0983'),
        backgroundColor: '#EF4444', // Red
        data: stats.value.ProjectHealthList.map(p => p.Overdue),
        borderRadius: 4
      }
    ]
  }
})

const healthChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  layout: {
    padding: {
      left: 10,
      right: 20,
      top: 10,
      bottom: 10
    }
  },
  plugins: {
    legend: {
      position: 'top',
      labels: {
        usePointStyle: true,
        padding: 15,
        font: { family: "'Nunito Sans', sans-serif", size: 12 }
      }
    },
    tooltip: {
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      titleColor: '#1E293B',
      bodyColor: '#475569',
      borderColor: '#E2E8F0',
      borderWidth: 1,
      padding: 12,
      usePointStyle: true,
      boxPadding: 6,
      titleFont: { weight: 'bold', size: 13 },
      bodyFont: { size: 12 }
    }
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: {
        font: { family: "'Nunito Sans', sans-serif", size: 11 },
        maxRotation: 45,
        minRotation: 0,
        autoSkip: false
      }
    },
    y: {
      beginAtZero: true,
      grid: { color: '#F1F5F9', borderDash: [5, 5] },
      ticks: {
        precision: 0,
        font: { family: "'Nunito Sans', sans-serif", size: 11 }
      }
    }
  }
}

const doughnutChartData = computed(() => {
  if (!stats.value) return { labels: [], datasets: [] }
  
  return {
    labels: [
      t('admin.SCR0917'),
      t('admin.SCR0918'),
      t('admin.SCR0919')
    ],
    datasets: [
      {
        label: t('admin.SCR0913'),
        backgroundColor: ['#94A3B8', '#3B82F6', '#22C55E'],
        borderWidth: 0,
        hoverOffset: 4,
        data: [
          stats.value.TaskStatusDistribution.ToDo,
          stats.value.TaskStatusDistribution.InProgress,
          stats.value.TaskStatusDistribution.Done
        ]
      }
    ]
  }
})

const doughnutChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '75%',
  plugins: {
    legend: {
      position: 'bottom',
      labels: {
        usePointStyle: true,
        padding: 20,
        font: { family: "'Nunito Sans', sans-serif", size: 12 }
      }
    },
    tooltip: {
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      titleColor: '#1E293B',
      bodyColor: '#475569',
      borderColor: '#E2E8F0',
      borderWidth: 1,
      padding: 12,
      usePointStyle: true,
      boxPadding: 6,
      titleFont: { weight: 'bold', size: 13 },
      bodyFont: { size: 12 }
    }
  }
}
</script>

<style scoped>
.dashboard-card {
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}
.dashboard-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 20px -5px rgba(0, 0, 0, 0.05) !important;
}

.chart-scroll-wrapper {
  overflow-x: auto;
  padding-bottom: 8px;
}

.custom-scrollbar::-webkit-scrollbar {
  height: 6px;
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: #F1F5F9;
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #CBD5E1;
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94A3B8;
}
</style>
