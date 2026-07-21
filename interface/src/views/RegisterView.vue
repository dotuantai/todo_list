<template>
  <AuthLayout
    :title="showOtpVerification ? 'Verify your email' : 'Create your account'"
    :subtitle="showOtpVerification ? 'We have sent a 6-digit OTP verification code to ' + email + '.' : 'Join TaskFlow and start organizing work with clarity.'"
    :switchLabel="showOtpVerification ? '' : 'Already have an account?'"
    :switchText="showOtpVerification ? '' : 'Sign in'"
    :switchTo="showOtpVerification ? '' : '/login'"
  >
    <!-- Register Form -->
    <form v-if="!showOtpVerification" @submit.prevent="handleRegister">
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

      <div class="mb-3">
        <label class="form-label fw-semibold text-secondary small">Password</label>
        <div class="input-group input-group-lg">
          <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: var(--bs-border-color);">
            <i class="bi bi-lock"></i>
          </span>
          <input 
            v-model="password" 
            :type="showPassword ? 'text' : 'password'" 
            class="form-control bg-body-secondary border-start-0 border-end-0 ps-0" 
            placeholder="At least 8 characters" 
            style="font-size: 0.95rem; height: 48px; border-color: var(--bs-border-color);" 
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
        <div class="form-text text-muted" style="font-size: 0.75rem; margin-top: 4px; text-align: left;">
          Password must be at least 8 characters, containing uppercase, lowercase, numbers, and special symbols.
        </div>
      </div>

      <div class="mb-4">
        <label class="form-label fw-semibold text-secondary small">Confirm password</label>
        <div class="input-group input-group-lg">
          <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: var(--bs-border-color);">
            <i class="bi bi-shield-lock"></i>
          </span>
          <input 
            v-model="confirmPassword" 
            :type="showConfirmPassword ? 'text' : 'password'" 
            class="form-control bg-body-secondary border-start-0 border-end-0 ps-0" 
            placeholder="Re-enter password" 
            style="font-size: 0.95rem; height: 48px; border-color: var(--bs-border-color);" 
            required 
          />
          <button 
            type="button" 
            class="input-group-text bg-body-secondary border-start-0 text-muted" 
            style="border-radius: 0 12px 12px 0; border-color: var(--bs-border-color); cursor: pointer;"
            @click="showConfirmPassword = !showConfirmPassword"
          >
            <i class="bi" :class="showConfirmPassword ? 'bi-eye-slash-fill' : 'bi-eye-fill'"></i>
          </button>
        </div>
      </div>

      <button class="btn btn-primary w-100 py-2 fs-6 fw-bold shadow-sm d-flex align-items-center justify-content-center gap-2" type="submit" :disabled="loading" style="border-radius: 12px; height: 48px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;">
        <span v-if="loading" class="spinner-border spinner-border-sm" role="status"></span>
        {{ loading ? 'Creating account...' : 'Create account' }}
      </button>
    </form>

    <!-- OTP Verification Form -->
    <form v-else @submit.prevent="handleVerifyOtp">
      <div class="mb-4">
        <label class="form-label fw-semibold text-secondary small">Verification Code (OTP)</label>
        <div class="input-group input-group-lg">
          <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: var(--bs-border-color);">
            <i class="bi bi-shield-check"></i>
          </span>
          <input 
            v-model="otpCode" 
            type="text" 
            class="form-control bg-body-secondary border-start-0 ps-0 text-center fw-bold" 
            placeholder="123456" 
            maxlength="6"
            style="border-radius: 0 12px 12px 0; font-size: 1.2rem; letter-spacing: 4px; height: 48px; border-color: var(--bs-border-color);" 
            required 
          />
        </div>
      </div>

      <button class="btn btn-primary w-100 py-2 fs-6 fw-bold shadow-sm d-flex align-items-center justify-content-center gap-2 mb-3" type="submit" :disabled="loading" style="border-radius: 12px; height: 48px; background: linear-gradient(135deg, #4f46e5, #6366f1); border: none;">
        <span v-if="loading" class="spinner-border spinner-border-sm" role="status"></span>
        {{ loading ? 'Verifying...' : 'Verify Code' }}
      </button>

      <div class="d-flex justify-content-between align-items-center mt-3">
        <button type="button" class="btn btn-link text-secondary text-decoration-none small p-0" @click="showOtpVerification = false">
          <i class="bi bi-arrow-left"></i> Back
        </button>
        <button type="button" class="btn btn-link text-primary text-decoration-none small p-0 fw-semibold" @click="handleResendOtp" :disabled="resending">
          <span v-if="resending" class="spinner-border spinner-border-sm me-1" role="status"></span>
          Resend code
        </button>
      </div>
    </form>
  </AuthLayout>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import AuthLayout from '../components/AuthLayout.vue'
import { loginn, register, verifyOtp, resendOtp } from '../services/authService.js'
import { toastError, toastSuccess, extractMessage } from '../utils/swal.js'

const router = useRouter()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const otpCode = ref('')
const loading = ref(false)
const resending = ref(false)
const showOtpVerification = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)

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

    toastSuccess('Verification code sent! Please check your email inbox.')
    showOtpVerification.value = true
  } catch (error) {
    toastError(extractMessage(error, 'Registration failed.'))
  } finally {
    loading.value = false
  }
}

const handleVerifyOtp = async () => {
  if (!otpCode.value.trim()) {
    toastError('Please enter the OTP verification code.')
    return
  }

  try {
    loading.value = true
    await verifyOtp({
      Email: email.value,
      Otp: otpCode.value
    })

    // Automatic login in the background
    const loginRes = await loginn({
      Email: email.value,
      Password: password.value
    })

    if (loginRes?.data?.AccessToken) {
      localStorage.setItem('token', loginRes.data.AccessToken)
    }

    toastSuccess('Account verified and logged in successfully!')
    router.push('/projects')
  } catch (error) {
    toastError(extractMessage(error, 'Verification or login failed.'))
  } finally {
    loading.value = false
  }
}

const handleResendOtp = async () => {
  try {
    resending.value = true
    await resendOtp(email.value)
    toastSuccess('A new verification code has been sent to your email.')
  } catch (error) {
    toastError(extractMessage(error, 'Failed to resend code.'))
  } finally {
    resending.value = false
  }
}
</script>
