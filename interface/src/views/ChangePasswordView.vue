<template>
  <div class="container-fluid min-vh-100 d-flex align-items-center justify-content-center p-3 p-md-4 auth-shell position-relative">
    <!-- Floating Language & Theme Switcher -->
    <div class="position-absolute top-0 end-0 p-3 d-flex align-items-center gap-2" style="z-index: 1050;">
      <div class="dropdown">
        <button 
          class="btn btn-light p-0 border rounded-3 d-flex align-items-center justify-content-center" 
          style="width: 36px; height: 36px; outline: none; box-shadow: none;" 
          type="button"
          data-bs-toggle="dropdown"
          aria-expanded="false"
          :title="$t('common.SCR0018')"
        >
          <span class="d-flex align-items-center justify-content-center">
            <svg v-if="locale === 'vi'" viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><circle cx="12" cy="12" r="12" fill="#da251d"/><polygon points="12,6 12.95,9.58 16.71,9.58 13.66,11.8 14.79,15.38 12,13.16 9.21,15.38 10.34,11.8 7.29,9.58 11.05,9.58" fill="#ffff00"/></svg>
            <svg v-else viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-btn-chpass"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-btn-chpass)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
          </span>
        </button>
        <ul class="dropdown-menu dropdown-menu-end shadow border-0 mt-2 p-1 rounded-3" style="min-width: 130px; z-index: 1060;">
          <li>
            <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="changeLocale('vi')">
              <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><circle cx="12" cy="12" r="12" fill="#da251d"/><polygon points="12,6 12.95,9.58 16.71,9.58 13.66,11.8 14.79,15.38 12,13.16 9.21,15.38 10.34,11.8 7.29,9.58 11.05,9.58" fill="#ffff00"/></svg>
              <span style="font-size: 0.85rem;">{{ $t('common.SCR0019') }}</span>
            </button>
          </li>
          <li>
            <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="changeLocale('en')">
              <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-item-chpass"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-item-chpass)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
              <span style="font-size: 0.85rem;">{{ $t('common.SCR0020') }}</span>
            </button>
          </li>
        </ul>
      </div>

      <ThemeToggle />
    </div>

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
import ThemeToggle from '../components/ThemeToggle.vue'

const router = useRouter()
const { t, locale } = useI18n()

const changeLocale = (lang) => {
  locale.value = lang
  localStorage.setItem('locale', lang)
}

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
