<template>
  <div class="min-vh-100 d-flex align-items-center justify-content-center bg-light p-3">
    <div class="card shadow-sm border-0 w-100" style="max-width: 450px;">
      <div class="card-body p-4 p-md-5">
        <div class="text-center mb-4">
          <div class="bg-primary-subtle text-primary rounded-circle d-inline-flex align-items-center justify-content-center mb-3" style="width: 60px; height: 60px;">
            <i class="bi bi-shield-lock-fill fs-2"></i>
          </div>
          <h4 class="fw-bold text-body">{{ $t('auth.SCR0709') }}</h4>
          <p class="text-muted small">{{ $t('auth.SCR0710') }}</p>
        </div>

        <form @submit.prevent="handleSubmit">
          <div class="mb-3">
            <label class="form-label fw-semibold small">{{ $t('auth.SCR0711') }} <span class="text-danger">*</span></label>
            <input type="password" class="form-control" v-model="form.currentPassword" required :placeholder="$t('auth.SCR0712')">
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

          <button type="submit" class="btn btn-primary w-100 fw-bold py-2" :disabled="loading">
            <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
            <span v-if="loading">...</span>
            <span v-else>{{ $t('auth.SCR0717') }}</span>
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { changePassword } from '../services/authService.js'
import { toastSuccess, toastError, extractMessage } from '../utils/swal.js'
import { useProjectStore } from '../stores/projectStore.js'

const router = useRouter()
const store = useProjectStore()
const { t } = useI18n()
const loading = ref(false)

const form = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const handleSubmit = async () => {
  if (form.newPassword !== form.confirmPassword) {
    toastError(t('common.SCR0016'))
    return
  }

  loading.value = true
  try {
    await changePassword({
      CurrentPassword: form.currentPassword,
      NewPassword: form.newPassword
    })
    
    // Xóa cờ requiresPasswordChange khỏi localStorage
    localStorage.removeItem('requiresPasswordChange')
    toastSuccess(t('changePassword.SCR0812'))
    
    // Redirect based on role
    if (store.appRole === 'Admin') {
      router.push('/admin/projects')
    } else {
      router.push('/projects')
    }
  } catch (error) {
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    loading.value = false
  }
}
</script>
