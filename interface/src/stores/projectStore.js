import { defineStore } from 'pinia'
import { getProjects } from '../services/projectService.js'

export const useProjectStore = defineStore('project', {
  state: () => ({
    projects: [],
    currentProjectId: localStorage.getItem('currentProjectId') || null,
    currentProject: null,
    userRole: 'Member',
    loading: false,
    currentUserEmail: localStorage.getItem('userEmail') || null,
    currentUserId: localStorage.getItem('userId') || null,
    token: localStorage.getItem('token') || null
  }),

  getters: {
    isAuthenticated() {
      const token = this.token
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
          this.userRole = p.UserRole || 'Member'
        } else {
          this.currentProject = null
          this.userRole = 'Member'
        }
      } else {
        this.currentProject = null
        this.userRole = 'Member'
      }
    },

    decodeToken() {
      const token = localStorage.getItem('token')
      this.token = token
      if (!token) {
        this.currentUserEmail = null
        this.currentUserId = null
        localStorage.removeItem('userEmail')
        localStorage.removeItem('userId')
        return
      }
      try {
        const base64Url = token.split('.')[1]
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''))
        const payload = JSON.parse(jsonPayload)
        const email = payload.email || 
                      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || 
                      payload.unique_name || 
                      payload.sub || 
                      'User'
        const userId = payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
        this.currentUserEmail = email
        this.currentUserId = userId
        localStorage.setItem('userEmail', email)
        if (userId) {
          localStorage.setItem('userId', userId)
        }
      } catch (e) {
        console.error('Error decoding token', e)
        this.currentUserEmail = 'User'
        this.currentUserId = null
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
      this.userRole = 'Member'
      this.currentUserEmail = null
      this.currentUserId = null
      this.token = null
      localStorage.removeItem('currentProjectId')
      localStorage.removeItem('userEmail')
      localStorage.removeItem('userId')
    }
  }
})
