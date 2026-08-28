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
            <svg v-else viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-btn-auth"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-btn-auth)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
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
              <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-item-auth"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-item-auth)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
              <span style="font-size: 0.85rem;">{{ $t('common.SCR0020') }}</span>
            </button>
          </li>
        </ul>
      </div>

      <ThemeToggle />
    </div>

    <div class="card border-0 shadow-lg rounded-4 overflow-hidden w-100" style="max-width: 1000px;">
      <div class="row g-0">
        <!-- Hero Column -->
        <div class="col-lg-6 bg-primary text-white p-4 p-md-5 d-flex flex-column justify-content-center auth-hero">
          <div class="brand-badge bg-white bg-opacity-25 rounded-3 d-flex align-items-center justify-content-center fw-bold text-white mb-3" style="width: 48px; height: 48px; font-size: 1.25rem;">
            TT
          </div>
          <h1 class="fw-bold h2 mb-2 text-white">TutaFlow</h1>
          <p class="text-white text-opacity-75 mb-4">
            Organize work with a calm, modern workspace that feels as polished as your tasks.
          </p>
          <ul class="list-unstyled mb-0 d-grid gap-2">
            <li class="d-flex align-items-center gap-2">
              <i class="bi bi-check-circle-fill text-white"></i> Track work in one place
            </li>
            <li class="d-flex align-items-center gap-2">
              <i class="bi bi-check-circle-fill text-white"></i> Assign and update tasks easily
            </li>
            <li class="d-flex align-items-center gap-2">
              <i class="bi bi-check-circle-fill text-white"></i> Keep deadlines visible and clear
            </li>
          </ul>
        </div>

        <!-- Panel Column -->
        <div class="col-lg-6 p-4 p-md-5 bg-body d-flex flex-column justify-content-center">
          <div class="mb-4">
            <span class="badge bg-primary bg-opacity-10 text-primary px-3 py-2 rounded-pill fw-bold mb-3" style="font-size: 0.8rem;">
              TutaFlow
            </span>
            <h2 class="fw-bold text-dark-override h4 mb-1">{{ title }}</h2>
            <p class="text-muted small mb-0">{{ subtitle }}</p>
          </div>

          <slot />

          <div class="mt-4 text-center text-muted small">
            <span>{{ switchLabel }} </span>
            <router-link :to="switchTo" class="text-primary fw-semibold text-decoration-none hover-underline">{{ switchText }}</router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useI18n } from 'vue-i18n'
import ThemeToggle from './ThemeToggle.vue'

const { locale } = useI18n()

const changeLocale = (lang) => {
  locale.value = lang
  localStorage.setItem('locale', lang)
}

defineProps({
  title: { type: String, default: 'Welcome back' },
  subtitle: { type: String, default: 'Sign in to continue' },
  switchLabel: { type: String, default: 'Need an account?' },
  switchText: { type: String, default: 'Create account' },
  switchTo: { type: String, default: '/register' }
})
</script>

<style scoped>
.auth-shell {
  background: radial-gradient(circle at top left, rgba(16,185,129,0.15), transparent 40%),
              linear-gradient(135deg, #f8faff 0%, #eef2ff 100%);
}
[data-bs-theme="dark"] .auth-shell {
  background: radial-gradient(circle at top left, rgba(0, 255, 136, 0.1), transparent 50%),
              radial-gradient(circle at bottom right, rgba(255, 0, 255, 0.08), transparent 50%),
              linear-gradient(135deg, #0a0a0f 0%, #12121a 100%);
}
[data-bs-theme="dark"] .auth-hero {
  background: linear-gradient(135deg, #0a0a0f 0%, #161626 100%) !important;
  border-right: 1px solid #2a2a3a;
  position: relative;
}
[data-bs-theme="dark"] .auth-hero::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: radial-gradient(circle at center, rgba(0, 255, 136, 0.12) 0%, transparent 70%);
  pointer-events: none;
}
[data-bs-theme="dark"] .brand-badge {
  background: rgba(0, 255, 136, 0.15) !important;
  border: 1px solid #00ff88 !important;
  color: #00ff88 !important;
  box-shadow: 0 0 10px rgba(0, 255, 136, 0.3);
}
.auth-hero {
  background: linear-gradient(135deg, #059669 0%, #10b981 100%) !important;
}
.hover-underline:hover {
  text-decoration: underline !important;
}
.text-dark-override {
  color: var(--bs-heading-color, #212529);
}
</style>
