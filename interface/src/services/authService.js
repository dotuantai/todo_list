import api from '../api/axios'

export const loginn = (data) => {
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