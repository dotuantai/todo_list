import api from '../api/axios'


export const getAssignedTasks = () => {
  return api.get('/tasks/my-assigned')
}

export const getCreatedTasks = () => {
  return api.get('/tasks/my-created')
}

export const createTask = (data) => {
  return api.post('/tasks', data)
}

export const updateTask = ( data) => {
  return api.put('/tasks', data)
}

export const deleteTask = (id) => {
  return api.delete(`/tasks/${id}`)
}

export const assignTask = (data) => {
  return api.post('/tasks/assign', data)
}
export const updatePermission  = (data) => {
  return api.put('/tasks/assign', data)
}
export const removeAssignment = (data) => {
  return api.delete('/tasks/assign', { data })
}