import api from '../api/axios'

export const login = (data) => {
  return api.post('/login', data)
}

export const searchUsers = (keyword) => {
  return api.get('/auth/search', {
    params: {
      q: keyword
    }
  })
}