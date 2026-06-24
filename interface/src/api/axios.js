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

        const refreshResponse =
          await api.post('/auth/refresh')

        localStorage.setItem(
          'token',
          refreshResponse.data.accessToken || refreshResponse.data.AccessToken
        )

        originalRequest.headers.Authorization =
          `Bearer ${refreshResponse.data.AccessToken}`

        return api(originalRequest)

      } catch {

        localStorage.removeItem('token')

        window.location.href = '/'
      }
    }

    return Promise.reject(error)
  }
)

export default api