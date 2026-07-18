import { defineStore } from 'pinia'
import { getProjects } from '../services/projectService.js'

export const useProjectStore = defineStore('project', {
  state: () => ({
    projects: [],
    currentProjectId: localStorage.getItem('currentProjectId') || null,
    currentProject: null,
    userRole: 'Viewer',
    loading: false,
    currentUserEmail: localStorage.getItem('userEmail') || null
  }),

  getters: {
    isAuthenticated() {
      const token = localStorage.getItem('token')
      return !!token && token !== 'null' && token !== 'undefined' && token.split('.').length === 3
    },
    currentInitial() {
      return this.currentUserEmail ? this.currentUserEmail[0].toUpperCase() : '?'
    }
  },

  actions: {
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
      this.updateCurrentProject()
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
    },

    decodeToken() {
      const token = localStorage.getItem('token')
      if (!token) {
        this.currentUserEmail = null
        localStorage.removeItem('userEmail')
        return
      }
      try {
        const base64Url = token.split('.')[1]
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''))
        const payload = JSON.parse(jsonPayload)
        const email = payload.email || payload.unique_name || payload.sub || 'User'
        this.currentUserEmail = email
        localStorage.setItem('userEmail', email)
      } catch (e) {
        console.error('Error decoding token', e)
        this.currentUserEmail = 'User'
      }
    },

    async fetchProjects() {
      if (!this.isAuthenticated) return
      this.loading = true
      try {
        const res = await getProjects()
        this.setProjects(res?.data || [])
      } catch (err) {
        console.error('Failed to fetch projects', err)
      } finally {
        this.loading = false
      }
    },

    clearStore() {
      this.projects = []
      this.currentProjectId = null
      this.currentProject = null
      this.userRole = 'Viewer'
      this.currentUserEmail = null
      localStorage.removeItem('currentProjectId')
      localStorage.removeItem('userEmail')
    }
  }
})
