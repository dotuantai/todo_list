<template>
  <AuthLayout
    title="Create your account"
    subtitle="Join TaskFlow and start organizing work with clarity."
    switchLabel="Already have an account?"
    switchText="Sign in"
    switchTo="/login"
  >
    <form @submit.prevent="handleRegister">
      <div class="mb-3">
        <label class="form-label fw-semibold text-secondary small">Email</label>
        <div class="input-group input-group-lg">
          <span class="input-group-text bg-light border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: #e2e8f0;">
            <i class="bi bi-envelope"></i>
          </span>
          <input 
            v-model="email" 
            type="email" 
            class="form-control bg-light border-start-0 ps-0 text-dark" 
            placeholder="you@example.com" 
            style="border-radius: 0 12px 12px 0; font-size: 0.95rem; height: 48px; border-color: #e2e8f0;" 
            required 
          />
        </div>
      </div>

      <div class="mb-3">
        <label class="form-label fw-semibold text-secondary small">Password</label>
        <div class="input-group input-group-lg">
          <span class="input-group-text bg-light border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: #e2e8f0;">
            <i class="bi bi-lock"></i>
          </span>
          <input 
            v-model="password" 
            type="password" 
            class="form-control bg-light border-start-0 ps-0 text-dark" 
            placeholder="At least 6 characters" 
            style="border-radius: 0 12px 12px 0; font-size: 0.95rem; height: 48px; border-color: #e2e8f0;" 
            required 
          />
        </div>
      </div>

      <div class="mb-4">
        <label class="form-label fw-semibold text-secondary small">Confirm password</label>
        <div class="input-group input-group-lg">
          <span class="input-group-text bg-light border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: #e2e8f0;">
            <i class="bi bi-shield-lock"></i>
          </span>
          <input 
            v-model="confirmPassword" 
            type="password" 
            class="form-control bg-light border-start-0 ps-0 text-dark" 
            placeholder="Re-enter password" 
            style="border-radius: 0 12px 12px 0; font-size: 0.95rem; height: 48px; border-color: #e2e8f0;" 
            required 
          />
        </div>
      </div>

      <button class="btn btn-primary w-100 py-2 fs-6 fw-bold shadow-sm d-flex align-items-center justify-content-center gap-2" type="submit" :disabled="loading" style="border-radius: 12px; height: 48px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;">
        <span v-if="loading" class="spinner-border spinner-border-sm" role="status"></span>
        {{ loading ? 'Creating account...' : 'Create account' }}
      </button>
    </form>
  </AuthLayout>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import AuthLayout from '../components/AuthLayout.vue'
import { register } from '../Services/authService.js'
import { toastError, toastSuccess, extractMessage } from '../utils/swal.js'

const router = useRouter()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)

const handleRegister = async () => {
  if (!email.value.trim() || !password.value || !confirmPassword.value) {
    toastError('Please complete all fields.')
    return
  }

  if (password.value !== confirmPassword.value) {
    toastError('Passwords do not match.')
    return
  }

  try {
    loading.value = true
    await register({
      Email: email.value,
      Password: password.value
    })

    toastSuccess('Account created successfully.')
    router.push('/login')
  } catch (error) {
    toastError(extractMessage(error, 'Registration failed.'))
  } finally {
    loading.value = false
  }
}
</script>
