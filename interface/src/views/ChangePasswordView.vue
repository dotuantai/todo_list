<template>
  <div class="container-fluid min-vh-100 d-flex align-items-center justify-content-center p-3 p-md-4 auth-shell">
    <div class="card border-0 shadow-lg rounded-4 overflow-hidden w-100" style="max-width: 500px; background: var(--bs-card-bg);">
      <div class="card-body p-4 p-md-5">
        
        <!-- Header -->
        <div class="text-center mb-4">
          <div class="brand-badge bg-primary text-white mx-auto rounded-3 d-flex align-items-center justify-content-center fw-bold mb-3" style="width: 48px; height: 48px; font-size: 1.25rem; background: linear-gradient(135deg, #0d9488, #14b8a6) !important;">
            <i class="bi bi-shield-lock-fill"></i>
          </div>
          <h2 class="fw-bold h4 mb-1 text-body">{{ $t('changePassword.SCR0801') }}</h2>
          <p class="text-muted small">{{ $t('changePassword.SCR0802') }}</p>
        </div>

        <!-- Form -->
        <form @submit.prevent="handleChangePassword">
          <!-- Current Password -->
          <div class="mb-3">
            <label class="form-label fw-semibold text-secondary small text-start d-block">{{ $t('changePassword.SCR0803') }}</label>
            <div class="input-group">
              <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: var(--bs-border-color);">
                <i class="bi bi-key-fill"></i>
              </span>
              <input 
                v-model="form.currentPassword" 
                :type="showCurrent ? 'text' : 'password'" 
                class="form-control bg-body-secondary border-start-0 border-end-0 ps-0" 
                :placeholder="$t('changePassword.SCR0804')"
                style="font-size: 0.95rem; height: 48px; border-color: var(--bs-border-color);" 
                required 
              />
              <button 
                type="button" 
                class="input-group-text bg-body-secondary border-start-0 text-muted" 
                style="border-radius: 0 12px 12px 0; border-color: var(--bs-border-color); cursor: pointer;"
                @click="showCurrent = !showCurrent"
              >
                <i class="bi" :class="showCurrent ? 'bi-eye-slash-fill' : 'bi-eye-fill'"></i>
              </button>
            </div>
          </div>

          <!-- New Password -->
          <div class="mb-3">
            <label class="form-label fw-semibold text-secondary small text-start d-block">{{ $t('changePassword.SCR0805') }}</label>
            <div class="input-group">
              <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: var(--bs-border-color);">
                <i class="bi bi-lock-fill"></i>
              </span>
              <input 
                v-model="form.newPassword" 
                :type="showNew ? 'text' : 'password'" 
                class="form-control bg-body-secondary border-start-0 border-end-0 ps-0" 
                :placeholder="$t('changePassword.SCR0806')"
                style="font-size: 0.95rem; height: 48px; border-color: var(--bs-border-color);" 
                required 
              />
              <button 
                type="button" 
                class="input-group-text bg-body-secondary border-start-0 text-muted" 
                style="border-radius: 0 12px 12px 0; border-color: var(--bs-border-color); cursor: pointer;"
                @click="showNew = !showNew"
              >
                <i class="bi" :class="showNew ? 'bi-eye-slash-fill' : 'bi-eye-fill'"></i>
              </button>
            </div>
          </div>

          <!-- Confirm New Password -->
          <div class="mb-4">
            <label class="form-label fw-semibold text-secondary small text-start d-block">{{ $t('changePassword.SCR0807') }}</label>
            <div class="input-group">
              <span class="input-group-text bg-body-secondary border-end-0 text-muted" style="border-radius: 12px 0 0 12px; border-color: var(--bs-border-color);">
                <i class="bi bi-shield-check"></i>
              </span>
              <input 
                v-model="form.confirmPassword" 
                :type="showConfirm ? 'text' : 'password'" 
                class="form-control bg-body-secondary border-start-0 border-end-0 ps-0" 
                :placeholder="$t('changePassword.SCR0808')"
                style="font-size: 0.95rem; height: 48px; border-color: var(--bs-border-color);" 
                required 
              />
              <button 
                type="button" 
                class="input-group-text bg-body-secondary border-start-0 text-muted" 
                style="border-radius: 0 12px 12px 0; border-color: var(--bs-border-color); cursor: pointer;"
                @click="showConfirm = !showConfirm"
              >
                <i class="bi" :class="showConfirm ? 'bi-eye-slash-fill' : 'bi-eye-fill'"></i>
              </button>
            </div>
          </div>

          <!-- Actions -->
          <div class="d-grid gap-2">
            <button 
              class="btn btn-primary py-2.5 fs-6 fw-bold shadow-sm d-flex align-items-center justify-content-center gap-2" 
              type="submit" 
              :disabled="loading" 
              style="border-radius: 12px; height: 48px; background: linear-gradient(135deg, #0d9488, #14b8a6); border: none;"
            >
              <span v-if="loading" class="spinner-border spinner-border-sm" role="status"></span>
              {{ loading ? $t('changePassword.SCR0810') : $t('changePassword.SCR0801') }}
            </button>
            
            <button 
              type="button" 
              class="btn btn-outline-secondary py-2.5 fs-6 fw-semibold" 
              style="border-radius: 12px; height: 48px;" 
              @click="goBack" 
              :disabled="loading"
            >
              {{ $t('changePassword.SCR0811') }}
            </button>
          </div>
        </form>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { changePassword } from '../services/authService.js'
import { toastSuccess, toastError, extractMessage } from '../utils/swal.js'

const router = useRouter()
const { t } = useI18n()
const loading = ref(false)

const form = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const showCurrent = ref(false)
const showNew = ref(false)
const showConfirm = ref(false)

const goBack = () => {
  router.back()
}

const handleChangePassword = async () => {
  if (form.newPassword !== form.confirmPassword) {
    toastError(t('common.SCR0016'))
    return
  }

  try {
    loading.value = true
    await changePassword({
      CurrentPassword: form.currentPassword,
      NewPassword: form.newPassword
    })
    toastSuccess(t('changePassword.SCR0812'))
    router.push('/projects')
  } catch (error) {
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-shell {
  background: radial-gradient(circle at top right, rgba(13, 148, 136, 0.08), transparent 40%),
              radial-gradient(circle at bottom left, rgba(13, 148, 136, 0.05), transparent 40%),
              var(--bs-body-bg);
}
</style>
