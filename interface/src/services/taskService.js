import api from '../api/axios'

export const createTask = (data) => {
  return api.post('/tasks', data)
}

export const updateTask = (id, data) => {
  return api.put(`/tasks/${id}`, data)
}

export const deleteTask = (id) => {
  return api.delete(`/tasks/${id}`)
}

export const assignTask = (data) => {
  return api.post('/tasks/assign', data)
}

export const removeAssignment = (taskId, userId) => {
  return api.delete(`/tasks/${taskId}/assignments/${userId}`)
}

export const updateTaskColumn = (data) => {
  return api.put('/tasks/status', data)
}

export const getComments = (taskId, page = 1, limit = 5) => {
  return api.get(`/tasks/${taskId}/comments?page=${page}&limit=${limit}`)
}

export const addComment = (taskId, data) => {
  return api.post(`/tasks/${taskId}/comments`, data)
}

export const updateComment = (id, data) => {
  return api.put(`/tasks/comments/${id}`, data)
}

export const deleteComment = (id) => {
  return api.delete(`/tasks/comments/${id}`)
}

export const getActivities = (taskId) => {
  return api.get(`/tasks/${taskId}/activities`)
}

export const getTaskFeed = (taskId, page = 1, pageSize = 5) => {
  return api.get(`/tasks/${taskId}/feed`, { params: { page, pageSize } })
}