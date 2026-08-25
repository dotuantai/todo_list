<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3 class="fw-bold mb-0 text-body">{{ $t('admin.projects_title') }}</h3>
    </div>

    <!-- Loading State -->
    <div v-if="projectStore.loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">{{ $t('admin.projects_loading') }}</p>
    </div>

    <!-- Data Table for Projects -->
    <div v-else class="bg-white rounded-4 shadow-sm border overflow-hidden">
      <div class="table-responsive">
        <table class="table table-hover mb-0 align-middle">
          <thead class="bg-light">
            <tr>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.projects_col_name') }}</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.projects_col_owner') }}</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.projects_col_members') }}</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.projects_col_progress') }}</th>
              <th class="border-0 px-4 py-3 text-uppercase text-secondary text-end" style="font-size: 0.75rem; letter-spacing: 0.5px;">{{ $t('admin.projects_col_actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="proj in projectsWithProgress" :key="proj.Id" class="project-row">
              <td class="px-4 py-3">
                <div class="d-flex align-items-center gap-3">
                  <div class="project-logo text-white d-flex align-items-center justify-content-center fw-bold rounded-2 shadow-sm" :style="{ background: getProjectColor(proj.Name) }" style="width: 36px; height: 36px;">
                    {{ proj.Name[0].toUpperCase() }}
                  </div>
                  <div>
                    <h6 class="mb-0 fw-bold">{{ proj.Name }}</h6>
                    <small class="text-muted text-truncate d-inline-block" style="max-width: 200px;">{{ proj.Description || $t('admin.projects_no_desc') }}</small>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3 text-muted small">{{ proj.OwnerEmail }}</td>
              <td class="px-4 py-3">
                <span class="badge bg-secondary-subtle text-secondary rounded-pill px-2 py-1">
                  <i class="bi bi-people-fill me-1"></i>{{ proj.MemberCount || 0 }}
                </span>
              </td>
              <td class="px-4 py-3" style="min-width: 150px;">
                <div class="d-flex align-items-center justify-content-between mb-1">
                  <span class="text-secondary" style="font-size: 0.7rem;">{{ proj.completedTasks }}/{{ proj.totalTasks }}</span>
                  <span class="fw-bold text-body" style="font-size: 0.75rem;">{{ proj.percent }}%</span>
                </div>
                <div class="progress" style="height: 6px; border-radius: 3px;">
                  <div class="progress-bar" :style="{ width: proj.percent + '%', backgroundColor: '#6366F1' }" role="progressbar"></div>
                </div>
              </td>
              <td class="px-4 py-3 text-end">
                <button class="btn btn-sm btn-light border-0 text-primary fw-medium px-3" @click="goToProject(proj.Id)">
                  {{ $t('admin.projects_enter') }} <i class="bi bi-arrow-right ms-1"></i>
                </button>
              </td>
            </tr>
            <tr v-if="projectsWithProgress.length === 0">
              <td colspan="5" class="text-center py-5 text-muted">
                {{ $t('admin.projects_empty') }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useProjectStore } from '../../stores/projectStore.js'

const router = useRouter()
const projectStore = useProjectStore()

onMounted(() => {
  projectStore.fetchProjects()
})

const projectsWithProgress = computed(() => {
  return projectStore.projects.map(p => {
    const total = p.TotalTasks || 0
    const completed = p.CompletedTasks || 0
    const percent = total > 0 ? Math.round((completed / total) * 100) : 0
    return { ...p, totalTasks: total, completedTasks: completed, percent }
  })
})

const goToProject = (projectId) => {
  localStorage.setItem('currentProjectId', projectId)
  projectStore.currentProjectId = projectId
  router.push(`/projects/${projectId}/dashboard`)
}

const getProjectColor = (name) => {
  if (!name) return '#6366F1'
  const colors = ['#6366F1', '#10B981', '#F59E0B', '#EF4444', '#EC4899', '#06B6D4', '#8B5CF6']
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  return colors[Math.abs(hash) % colors.length]
}
</script>

<style scoped>
.project-row {
  transition: background-color 0.15s ease;
}
.project-row:hover {
  background-color: rgba(99, 102, 241, 0.03);
}
</style>
