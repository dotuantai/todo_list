<template>
  <AuthLayout
    title="Welcome back"
    subtitle="Sign in to continue managing your tasks with a smoother workflow."
    switchLabel="New here?"
    switchText="Create an account"
    switchTo="/register"
  >
    <form @submit.prevent="login">
      <div class="mb-3">
        <label class="form-label fw-semibold text-secondary small">Email</label>
        <div class="input-group input-group-lg">
          <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: var(--bs-border-color);">
            <i class="bi bi-envelope"></i>
          </span>
          <input 
            v-model="email" 
            type="email" 
            class="form-control bg-body-secondary border-start-0 ps-0" 
            placeholder="you@example.com" 
            style="border-radius: 0 12px 12px 0; font-size: 0.95rem; height: 48px; border-color: var(--bs-border-color);" 
            required 
          />
        </div>
      </div>

      <div class="mb-4">
        <label class="form-label fw-semibold text-secondary small">Password</label>
        <div class="input-group input-group-lg">
          <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: var(--bs-border-color);">
            <i class="bi bi-lock"></i>
          </span>
          <input 
            v-model="password" 
            :type="showPassword ? 'text' : 'password'" 
            class="form-control bg-body-secondary border-start-0 border-end-0 ps-0" 
            placeholder="At least 6 characters" 
            style="font-size: 0.95rem; height: 48px; border-color: var(--bs-border-color);" 
            @keyup.enter="login" 
            required 
          />
          <button 
            type="button" 
            class="input-group-text bg-body-secondary border-start-0 text-muted" 
            style="border-radius: 0 12px 12px 0; border-color: var(--bs-border-color); cursor: pointer;"
            @click="showPassword = !showPassword"
          >
            <i class="bi" :class="showPassword ? 'bi-eye-slash-fill' : 'bi-eye-fill'"></i>
          </button>
        </div>
      </div>

      <button class="btn btn-primary w-100 py-2 fs-6 fw-bold shadow-sm d-flex align-items-center justify-content-center gap-2" type="submit" :disabled="loading" style="border-radius: 12px; height: 48px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;">
        <span v-if="loading" class="spinner-border spinner-border-sm" role="status"></span>
        {{ loading ? 'Signing in...' : 'Sign in' }}
      </button>
    </form>
  </AuthLayout>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import AuthLayout from '../components/AuthLayout.vue'
import { loginn } from '../services/authService.js'
import { toastError, extractMessage } from '../utils/swal.js'

const router = useRouter()

const email = ref('')
const password = ref('')
const loading = ref(false)
const showPassword = ref(false)

const login = async () => {
  try {
    loading.value = true

    const response = await loginn({
      Email: email.value,
      Password: password.value
    })

    if (response?.data?.AccessToken) {
      localStorage.setItem('token', response.data.AccessToken)
    }

    router.push('/projects')
  } catch (error) {
    toastError(extractMessage(error, 'Login failed.'))
  } finally {
    loading.value = false
  }
}
</script>