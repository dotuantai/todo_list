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

export const getProjectTasks = (projectId, columnId = null, page = 1, pageSize = 20) => {
  let url = `/projects/${projectId}/tasks?page=${page}&pageSize=${pageSize}`
  if (columnId !== null && columnId !== undefined) url += `&columnId=${columnId}`
  return api.get(url)
}

export const getProjectTaskStats = (projectId) => {
  return api.get(`/projects/${projectId}/tasks/stats`)
}

export const createProjectTask = (projectId, data) => {
  return api.post(`/projects/${projectId}/tasks`, data)
}

export const getProjectColumns = (projectId) => {
  return api.get(`/projects/${projectId}/columns`)
}

export const createProjectColumn = (projectId, data) => {
  return api.post(`/projects/${projectId}/columns`, data)
}

export const updateProjectColumn = (projectId, columnId, data) => {
  return api.put(`/projects/${projectId}/columns/${columnId}`, data)
}

export const deleteProjectColumn = (projectId, columnId) => {
  return api.delete(`/projects/${projectId}/columns/${columnId}`)
}
