import api from '../api/axios'

export const getNotifications = (page = 1, pageSize = 20) => {
  return api.get('/notifications', { params: { page, pageSize } })
}

export const markAsRead = (id) => {
  return api.put(`/notifications/${id}/read`)
}

export const markAllAsRead = () => {
  return api.put('/notifications/read-all')
}
