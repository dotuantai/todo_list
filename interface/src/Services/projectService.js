import api from '../api/axios'

export const getProjects = () => {
  return api.get('/projects')
}

export const createProject = (data) => {
  return api.post('/projects', data)
}

export const getProjectDetail = (id) => {
  return api.get(`/projects/${id}`)
}

export const updateProject = (id, data) => {
  return api.put(`/projects/${id}`, data)
}

export const deleteProject = (id) => {
  return api.delete(`/projects/${id}`)
}

export const getMembers = (projectId) => {
  return api.get(`/projects/${projectId}/members`)
}

export const addMember = (projectId, email, role) => {
  return api.post(`/projects/${projectId}/members`, { email, role })
}

export const updateMemberRole = (projectId, userId, role) => {
  return api.put(`/projects/${projectId}/members/${userId}`, { role })
}

export const removeMember = (projectId, userId) => {
  return api.delete(`/projects/${projectId}/members/${userId}`)
}

export const getProjectTasks = (projectId) => {
  return api.get(`/projects/${projectId}/tasks`)
}

export const createProjectTask = (projectId, data) => {
  return api.post(`/projects/${projectId}/tasks`, data)
}
