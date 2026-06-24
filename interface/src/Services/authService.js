import api from '../api/axios'

export const loginn = (data) => {
  return api.post('/auth/login', data)
}
export const logout = () => {
  return api.post('/auth/logout')
}

export const searchUsers = (keyword) => {
  return api.get('/auth/search', {
    params: {
      q: keyword
    }
  })
}