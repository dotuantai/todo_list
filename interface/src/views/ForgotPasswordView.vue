<template>
  <div class="min-vh-100 d-flex align-items-center justify-content-center bg-light p-3">
    <div class="card shadow-sm border-0 w-100" style="max-width: 450px;">
      <div class="card-body p-4 p-md-5">
        <div class="text-center mb-4">
          <div class="bg-primary-subtle text-primary rounded-circle d-inline-flex align-items-center justify-content-center mb-3" style="width: 60px; height: 60px;">
            <i class="bi bi-key-fill fs-2"></i>
          </div>
          <h4 class="fw-bold text-body" v-if="step === 1">{{ $t('auth.SCR0728') }}</h4>
          <h4 class="fw-bold text-body" v-else>{{ $t('auth.SCR0733') }}</h4>
          
          <p class="text-muted small" v-if="step === 1">{{ $t('auth.SCR0729') }}</p>
          <p class="text-muted small" v-else>{{ $t('auth.SCR0734') }}</p>
        </div>

        <!-- Step 1: Send OTP -->
        <form @submit.prevent="handleSendOtp" v-if="step === 1">
          <div class="mb-4">
            <label class="form-label fw-semibold small">{{ $t('auth.SCR0705') }} <span class="text-danger">*</span></label>
            <input type="email" class="form-control" v-model="email" required :placeholder="$t('auth.SCR0760')">
          </div>

          <button type="submit" class="btn btn-primary w-100 fw-bold py-2 mb-3" :disabled="loading">
            <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
            <span v-if="loading">...</span>
            <span v-else>{{ $t('auth.SCR0730') }}</span>
          </button>
          
          <div class="text-center">
            <router-link to="/login" class="text-decoration-none small fw-medium">
              <i class="bi bi-arrow-left me-1"></i> {{ $t('auth.SCR0731') }}
            </router-link>
          </div>
        </form>

        <!-- Step 2: Reset Password -->
        <form @submit.prevent="handleResetPassword" v-else>
          <div class="mb-3">
            <label class="form-label fw-semibold small">{{ $t('auth.SCR0724') }} <span class="text-danger">*</span></label>
            <input type="text" class="form-control text-center fw-bold letter-spacing-2" v-model="form.otp" required placeholder="123456" maxlength="6">
          </div>

          <div class="mb-3">
            <label class="form-label fw-semibold small">{{ $t('auth.SCR0713') }} <span class="text-danger">*</span></label>
            <input type="password" class="form-control" v-model="form.newPassword" required :placeholder="$t('auth.SCR0714')">
            <div class="form-text small mt-2">
              {{ $t('auth.SCR0708') }}
            </div>
          </div>

          <div class="mb-4">
            <label class="form-label fw-semibold small">{{ $t('auth.SCR0715') }} <span class="text-danger">*</span></label>
            <input type="password" class="form-control" v-model="form.confirmPassword" required :placeholder="$t('auth.SCR0716')">
          </div>

          <button type="submit" class="btn btn-primary w-100 fw-bold py-2 mb-3" :disabled="loading">
            <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
            <span v-if="loading">...</span>
            <span v-else>{{ $t('auth.SCR0733') }}</span>
          </button>
          
          <div class="text-center">
            <a href="#" @click.prevent="step = 1" class="text-decoration-none small fw-medium">
              <i class="bi bi-arrow-left me-1"></i> {{ $t('auth.SCR0726') }}
            </a>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { forgotPassword, resetPassword } from '../services/authService.js'
import { toastSuccess, toastError, extractMessage } from '../utils/swal.js'

const router = useRouter()
const { t } = useI18n()
const loading = ref(false)
const step = ref(1)
const email = ref('')

const form = reactive({
  otp: '',
  newPassword: '',
  confirmPassword: ''
})

const handleSendOtp = async () => {
  if (!email.value) return
  
  loading.value = true
  try {
    await forgotPassword({ Email: email.value })
    toastSuccess(t('auth.SCR0732'))
    step.value = 2
  } catch (error) {
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    loading.value = false
  }
}

const handleResetPassword = async () => {
  if (form.newPassword !== form.confirmPassword) {
    toastError(t('common.SCR0016'))
    return
  }

  loading.value = true
  try {
    await resetPassword({
      Email: email.value,
      Otp: form.otp,
      NewPassword: form.newPassword
    })
    
    toastSuccess(t('auth.SCR0736'))
    router.push('/login')
  } catch (error) {
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.letter-spacing-2 {
  letter-spacing: 0.5rem;
}
</style>
