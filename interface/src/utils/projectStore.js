import { reactive } from 'vue'

export const projectStore = reactive({
  projects: [],
  currentProjectId: localStorage.getItem('currentProjectId') || null,
  currentProject: null,
  userRole: 'Viewer',
  loading: false,

  setCurrentProjectId(id) {
    this.currentProjectId = id
    if (id) {
      localStorage.setItem('currentProjectId', id)
    } else {
      localStorage.removeItem('currentProjectId')
    }
    this.updateCurrentProject()
  },

  setProjects(list) {
    this.projects = list
    if (list.length > 0) {
      const exists = list.some(p => p.Id === this.currentProjectId)
      if (!exists) {
        this.setCurrentProjectId(list[0].Id)
      } else {
        this.updateCurrentProject()
      }
    } else {
      this.setCurrentProjectId(null)
    }
  },

  updateCurrentProject() {
    if (this.projects.length > 0 && this.currentProjectId) {
      const p = this.projects.find(x => x.Id === this.currentProjectId)
      if (p) {
        this.currentProject = p
        this.userRole = p.UserRole || 'Viewer'
      } else {
        this.currentProject = null
        this.userRole = 'Viewer'
      }
    } else {
      this.currentProject = null
      this.userRole = 'Viewer'
    }
  }
})
