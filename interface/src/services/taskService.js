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

export const getComments = (taskId) => {
  return api.get(`/tasks/${taskId}/comments`)
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