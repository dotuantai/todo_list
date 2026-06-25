import axios from 'axios'

const api = axios.create({
  baseURL: 'https://localhost:44355/api',
  withCredentials: true
})

api.interceptors.request.use(config => {

  const token = localStorage.getItem('token')

  if (token) {
    config.headers.Authorization =
      `Bearer ${token}`
  }

  return config
})

api.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config

    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !originalRequest.url.includes('/auth/refresh')
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
        window.location.href = '/'
        return Promise.reject(refreshError)
      }
    }

    return Promise.reject(error)
  }
)

export default api