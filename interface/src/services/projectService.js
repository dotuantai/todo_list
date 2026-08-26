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

export const getProjectTasks = (projectId, columnId = null, page = 1, pageSize = 20, search = null, priority = null, assigneeId = null) => {
  let url = `/projects/${projectId}/tasks?page=${page}&pageSize=${pageSize}`
  if (columnId !== null && columnId !== undefined) url += `&columnId=${columnId}`
  if (search) url += `&search=${encodeURIComponent(search)}`
  if (priority) url += `&priority=${encodeURIComponent(priority)}`
  if (assigneeId) {
    if (assigneeId === 'unassigned') url += `&assigneeId=` // We'll pass empty for unassigned, wait. Actually, API doesn't support 'unassigned' natively. I should send a special GUID or handle it in backend. Or just let backend handle it?
    // Wait! In the API, assigneeId is Guid?. I should handle "unassigned" in the backend or frontend. Let's send a fake Guid for unassigned, or update API.
    // Let's send "00000000-0000-0000-0000-000000000000" for unassigned.
    url += `&assigneeId=${encodeURIComponent(assigneeId)}`
  }
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

export const downloadTaskTemplate = () => {
  return api.get('/projects/tasks/template', { responseType: 'blob' })
}

export const importTasks = (projectId, file) => {
  const formData = new FormData()
  formData.append('file', file)
  return api.post(`/projects/${projectId}/tasks/import`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}

export const getProjectExplorer = (projectId, folderId = null, taskId = null) => {
  let url = `/projects/${projectId}/files/explorer`
  const params = []
  if (folderId) params.push(`folderId=${folderId}`)
  if (taskId) params.push(`taskId=${taskId}`)
  if (params.length > 0) url += `?${params.join('&')}`
  return api.get(url)
}

export const getProjectFiles = (projectId, folderId = null, taskId = null) => {
  let url = `/projects/${projectId}/files`
  const params = []
  if (folderId) params.push(`folderId=${folderId}`)
  if (taskId) params.push(`taskId=${taskId}`)
  if (params.length > 0) url += `?${params.join('&')}`
  return api.get(url)
}

export const uploadProjectFile = (projectId, file, folderId = null, taskId = null, onUploadProgress = null) => {
  const formData = new FormData()
  formData.append('file', file)
  if (folderId) formData.append('folderId', folderId)
  if (taskId) formData.append('taskId', taskId)
  return api.post(`/projects/${projectId}/files`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    },
    onUploadProgress
  })
}

export const updateFileVersion = (projectId, fileId, file, changeNote = null, onUploadProgress = null) => {
  const formData = new FormData()
  formData.append('file', file)
  if (changeNote) formData.append('changeNote', changeNote)
  return api.post(`/projects/${projectId}/files/${fileId}/version`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    },
    onUploadProgress
  })
}

export const getFileVersionHistory = (projectId, fileId) => {
  return api.get(`/projects/${projectId}/files/${fileId}/history`)
}

export const downloadProjectFile = (projectId, fileId, versionId = null) => {
  const url = `/projects/${projectId}/files/${fileId}/download${versionId ? `?versionId=${versionId}` : ''}`
  return api.get(url, {
    responseType: 'blob'
  })
}

export const renameProjectFile = (projectId, fileId, fileName) => {
  return api.put(`/projects/${projectId}/files/${fileId}/rename`, { fileName })
}

export const deleteProjectFile = (projectId, fileId) => {
  return api.delete(`/projects/${projectId}/files/${fileId}`)
}

export const createProjectFolder = (projectId, name, parentFolderId = null) => {
  return api.post(`/projects/${projectId}/files/folders`, { name, parentFolderId })
}

export const renameProjectFolder = (projectId, folderId, name) => {
  return api.put(`/projects/${projectId}/files/folders/${folderId}/rename`, { name })
}

export const deleteProjectFolder = (projectId, folderId) => {
  return api.delete(`/projects/${projectId}/files/folders/${folderId}`)
}

export const getProjectFileActivities = (projectId) => {
  return api.get(`/projects/${projectId}/files/activities`)
}

export const batchDownloadProjectFiles = (projectId, fileIds = [], folderIds = []) => {
  return api.post(`/projects/${projectId}/files/batch-download`, { fileIds, folderIds }, {
    responseType: 'blob'
  })
}

export const batchDeleteProjectFiles = (projectId, fileIds = [], folderIds = []) => {
  return api.post(`/projects/${projectId}/files/batch-delete`, { fileIds, folderIds })
}
