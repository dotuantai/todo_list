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