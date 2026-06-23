<template>
  <div class="login-page">
    <div class="login-card">
      <h2>Login</h2>

      <div class="form-group">
        <label>Email</label>
        <input
          v-model="email"
          type="email"
          placeholder="Enter email"
        />
      </div>

      <div class="form-group">
        <label>Password</label>
        <input
          v-model="password"
          type="password"
          placeholder="Enter password"
        />
      </div>

      <button @click="login" :disabled="loading">
        {{ loading ? 'Loading...' : 'Login' }}
      </button>

      <p v-if="errorMessage" class="error">
        {{ errorMessage }}
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'

const router = useRouter()

const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

const login = async () => {
  try {
    loading.value = true
    errorMessage.value = ''

    const response = await axios.post(
      'https://localhost:44355/api/auth/login',
      {
        Email: email.value,
        Password: password.value
      }
    )

    console.log(response.data)

    if (response.data.token) {
      localStorage.setItem(
        'token',
        response.data.token
      )
    }

    router.push('/tasks')
  } catch (error) {
    console.error(error)

    errorMessage.value =
      error.response?.data?.message ||
      'Login failed'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: #f5f5f5;
}

.login-card {
  width: 400px;
  background: white;
  padding: 24px;
  border-radius: 10px;
  box-shadow: 0 0 10px rgba(0,0,0,.1);
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 6px;
}

.form-group input {
  width: 100%;
  padding: 10px;
  box-sizing: border-box;
}

button {
  width: 100%;
  padding: 10px;
  cursor: pointer;
}

.error {
  color: red;
  margin-top: 10px;
}
</style>