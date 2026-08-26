import axios from 'axios'
import { useProjectStore } from '../stores/projectStore.js'
import { alertError } from '../utils/swal.js'
import { localizeErrorCode } from '../utils/errorLocalization.js'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  withCredentials: true
})

let refreshPromise = null

const navigateByName = async (name) => {
  const { default: router } = await import('../router/index.js')
  if (router.currentRoute.value.name !== name) {
    await router.push({ name })
  }
}

const getRefreshedAccessToken = () => {
  if (!refreshPromise) {
    refreshPromise = api.post('/auth/refresh')
      .then(response => {
        const newToken = response.data?.AccessToken
        if (!newToken) {
          throw new Error('No token in refresh response')
        }

        localStorage.setItem('token', newToken)
        window.dispatchEvent(new CustomEvent('auth:token-refreshed', { detail: { token: newToken } }))
        return newToken
      })
      .finally(() => {
        refreshPromise = null
      })
  }

  return refreshPromise
}

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
    const errorCode = error.response?.data?.ErrorCode
    if (errorCode) {
      error.errorCode = errorCode
      error.localizedMessage = localizeErrorCode(errorCode, error.response?.data?.Message)
    }
    const originalRequest = error.config

    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !originalRequest.url.includes('/auth/refresh') &&
      !originalRequest.url.includes('/auth/login') &&
      !originalRequest.url.includes('/auth/register')
    ) {
      try {
        originalRequest._retry = true
        const newToken = await getRefreshedAccessToken()
        originalRequest.headers = originalRequest.headers || {}
        originalRequest.headers.Authorization = `Bearer ${newToken}`

        return api.request(originalRequest)

      } catch (refreshError) {
        const store = useProjectStore()
        store.clearStore()
        await navigateByName('login')
        return Promise.reject(refreshError)
      }
    }

    if (error.response?.status === 403) {
      if (window.location.pathname.includes('/projects/')) {
        const store = useProjectStore()
        store.setCurrentProjectId(null)
        await alertError('Access denied', 'You no longer have access to this project.')
        await navigateByName('projects')
      }
    }

    return Promise.reject(error)
  }
)

export default api
