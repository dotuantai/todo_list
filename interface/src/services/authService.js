import api from '../api/axios'

export const login = (data) => {
  return api.post('/auth/login', data)
}

export const register = (data) => {
  return api.post('/auth/register', data)
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

export const verifyOtp = (data) => {
  return api.post('/auth/verify-otp', data)
}

export const resendOtp = (email) => {
  return api.post('/auth/resend-otp', null, {
    params: { email }
  })
}

export const changePassword = (data) => {
  return api.post('/auth/change-password', data)
}

export const forgotPassword = (data) => {
  return api.post('/auth/forgot-password', data)
}

export const resetPassword = (data) => {
  return api.post('/auth/reset-password', data)
}

export const googleLogin = (idToken) => {
  return api.post('/auth/google-login', { idToken })
}

export const getAllUsers = () => {
  return api.get('/user')
}

export const createUser = (data) => {
  return api.post('/user', data)
}

export const resetTemporaryPassword = (userId) => {
  return api.post(`/user/${userId}/reset-temporary-password`)
}

export const updateUserRole = (userId, role) => {
  return api.put(`/user/${userId}/role`, { role })
}

export const updateUserStatus = (userId, isActive) => {
  return api.put(`/user/${userId}/status`, { isActive })
}