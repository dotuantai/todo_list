<template>
  <AuthLayout
    title="Create your account"
    subtitle="Join TaskFlow and start organizing work with clarity."
    switchLabel="Already have an account?"
    switchText="Sign in"
    switchTo="/login"
  >
    <form class="auth-form" @submit.prevent="handleRegister">
      <div class="form-group">
        <label>Email</label>
        <div class="input-wrap">
          <i class="bi bi-envelope"></i>
          <input v-model="email" type="email" placeholder="you@example.com" required />
        </div>
      </div>

      <div class="form-group">
        <label>Password</label>
        <div class="input-wrap">
          <i class="bi bi-lock"></i>
          <input v-model="password" type="password" placeholder="At least 6 characters" required />
        </div>
      </div>

      <div class="form-group">
        <label>Confirm password</label>
        <div class="input-wrap">
          <i class="bi bi-shield-lock"></i>
          <input v-model="confirmPassword" type="password" placeholder="Re-enter password" required />
        </div>
      </div>

      <button class="submit-btn" type="submit" :disabled="loading">
        <span v-if="loading" class="spinner"></span>
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

<style scoped>
.auth-form {
  display: grid;
  gap: 14px;
}

.form-group {
  display: grid;
  gap: 8px;
}

.form-group label {
  font-size: 0.92rem;
  font-weight: 600;
  color: #334155;
}

.input-wrap {
  display: flex;
  align-items: center;
  gap: 10px;
  border: 1px solid #e2e8f0;
  background: #f8fafc;
  border-radius: 14px;
  padding: 0 14px;
  height: 48px;
}

.input-wrap i {
  color: #64748b;
}

.input-wrap input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  font-size: 0.95rem;
}

.submit-btn {
  height: 48px;
  border: none;
  border-radius: 14px;
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  color: white;
  font-weight: 700;
  box-shadow: 0 10px 24px rgba(79,70,229,0.23);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  cursor: pointer;
}

.submit-btn:disabled {
  opacity: 0.75;
  cursor: not-allowed;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255,255,255,0.4);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
