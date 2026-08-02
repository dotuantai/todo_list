import axios from 'axios'
import { useProjectStore } from '../stores/projectStore.js'
import { alertError } from '../utils/swal.js'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  withCredentials: true
})

api.interceptors.request.use(config => {

  const token = localStorage.getItem('token')

  if (token && token !== 'null' && token !== 'undefined' && token.split('.').length === 3) {
    config.headers.Authorization =
      `Bearer ${token}`
  }

  return config
})

api.interceptors.response.use(
  response => {
    const wrapper = response.data
    if (
      wrapper &&
      typeof wrapper === 'object' &&
      Object.prototype.hasOwnProperty.call(wrapper, 'Success') &&
      Object.prototype.hasOwnProperty.call(wrapper, 'Data')
    ) {
      response.data = wrapper.Data
    }
    return response
  },
  async error => {
    const originalRequest = error.config

    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !originalRequest.url.includes('/auth/refresh') &&
      !originalRequest.url.includes('/auth/login') &&
      !originalRequest.url.includes('/auth/register')
    ) {
      originalRequest._retry = true

      try {
        const refreshResponse = await api.post('/auth/refresh')
        
        const newToken = refreshResponse.data?.AccessToken
        if (!newToken) {
          throw new Error('No token in refresh response')
        }
        
        localStorage.setItem('token', newToken)
        originalRequest.headers.Authorization = `Bearer ${newToken}`

        return api.request(originalRequest)

      } catch (refreshError) {
        localStorage.removeItem('token')
        if (window.location.pathname !== '/login' && window.location.pathname !== '/register') {
          window.location.href = '/login'
        }
        return Promise.reject(refreshError)
      }
    }

    if (error.response?.status === 403) {
      if (window.location.pathname.includes('/projects/')) {
        const store = useProjectStore()
        store.setCurrentProjectId(null)
        await alertError('Access denied', 'You no longer have access to this project.')
        window.location.href = '/projects'
      }
    }

    return Promise.reject(error)
  }
)

export default api