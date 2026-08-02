<template>
  <div class="auth-shell min-vh-100 d-flex align-items-stretch position-relative">
    <!-- Floating Language Switcher -->
    <div class="position-absolute top-0 end-0 p-3" style="z-index: 1050;">
      <div class="dropdown">
        <button 
          class="btn btn-light p-0 border rounded-3 d-flex align-items-center justify-content-center" 
          style="width: 36px; height: 36px; outline: none; box-shadow: none;"
          type="button"
          data-bs-toggle="dropdown"
          aria-expanded="false"
          title="Change Language"
        >
          <span class="d-flex align-items-center justify-content-center">
            <svg v-if="locale === 'vi'" viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><circle cx="12" cy="12" r="12" fill="#da251d"/><polygon points="12,6 12.95,9.58 16.71,9.58 13.66,11.8 14.79,15.38 12,13.16 9.21,15.38 10.34,11.8 7.29,9.58 11.05,9.58" fill="#ffff00"/></svg>
            <svg v-else viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-btn-register"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-btn-register)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
          </span>
        </button>
        <ul class="dropdown-menu dropdown-menu-end shadow border-0 mt-2 p-1 rounded-3" style="min-width: 130px; z-index: 1060;">
          <li>
            <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="changeLocale('vi')">
              <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><circle cx="12" cy="12" r="12" fill="#da251d"/><polygon points="12,6 12.95,9.58 16.71,9.58 13.66,11.8 14.79,15.38 12,13.16 9.21,15.38 10.34,11.8 7.29,9.58 11.05,9.58" fill="#ffff00"/></svg>
              <span style="font-size: 0.85rem;">Tiếng Việt</span>
            </button>
          </li>
          <li>
            <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="changeLocale('en')">
              <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-item-register"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-item-register)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
              <span style="font-size: 0.85rem;">English</span>
            </button>
          </li>
        </ul>
      </div>
    </div>


    <!-- ── Left Panel: Illustration ── -->
    <div class="auth-left d-none d-lg-flex flex-column align-items-center justify-content-center p-5">
      <div class="dot-pattern"></div>

      <!-- Brand -->
      <div class="d-flex align-items-center gap-3 mb-5 position-relative">
        <div class="brand-icon d-flex align-items-center justify-content-center">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none">
            <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" stroke="white" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <span class="fw-bold text-white fs-4 brand-name">TutaFlow</span>
      </div>

      <!-- Headline -->
      <div class="text-center mb-5 position-relative">
        <h2 class="text-white fw-bold mb-2" style="font-size: 1.65rem; line-height: 1.3;">
          {{ $t('auth.slogan_title') }}<br><span class="text-teal-light">{{ $t('auth.slogan_highlight') }}</span>
        </h2>
        <p class="text-white-50 small">{{ $t('auth.join_subtitle') }}</p>
      </div>

      <!-- Feature list -->
      <div class="features-list position-relative w-100" style="max-width: 340px;">
        <div class="feature-item">
          <div class="feature-icon-wrap" style="background: rgba(52, 211, 153, 0.15);">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none"><path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" stroke="#34d399" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
          </div>
          <div>
            <div class="fw-semibold text-white" style="font-size: 0.9rem;">{{ $t('auth.feature_kanban_title') }}</div>
            <div class="text-white-50" style="font-size: 0.78rem;">{{ $t('auth.feature_kanban_desc') }}</div>
          </div>
        </div>

        <div class="feature-item">
          <div class="feature-icon-wrap" style="background: rgba(56, 189, 248, 0.15);">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none"><path d="M13 10V3L4 14h7v7l9-11h-7z" stroke="#38bdf8" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
          </div>
          <div>
            <div class="fw-semibold text-white" style="font-size: 0.9rem;">{{ $t('auth.feature_realtime_title') }}</div>
            <div class="text-white-50" style="font-size: 0.78rem;">{{ $t('auth.feature_realtime_desc') }}</div>
          </div>
        </div>

        <div class="feature-item">
          <div class="feature-icon-wrap" style="background: rgba(251, 146, 60, 0.15);">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none"><path d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" stroke="#fb923c" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
          </div>
          <div>
            <div class="fw-semibold text-white" style="font-size: 0.9rem;">{{ $t('auth.feature_notif_title') }}</div>
            <div class="text-white-50" style="font-size: 0.78rem;">{{ $t('auth.feature_notif_desc') }}</div>
          </div>
        </div>

        <div class="feature-item" style="border-bottom: none; padding-bottom: 0;">
          <div class="feature-icon-wrap" style="background: rgba(167, 139, 250, 0.15);">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none"><path d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" stroke="#a78bfa" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
          </div>
          <div>
            <div class="fw-semibold text-white" style="font-size: 0.9rem;">{{ $t('auth.feature_roles_title') }}</div>
            <div class="text-white-50" style="font-size: 0.78rem;">{{ $t('auth.feature_roles_desc') }}</div>
          </div>
        </div>
      </div>

      <!-- Social proof -->
      <div class="text-center mt-5 position-relative">
        <div class="d-flex align-items-center justify-content-center gap-1 mb-2">
          <span v-for="n in 5" :key="n">⭐</span>
        </div>
        <p class="text-white-50 small mb-0">{{ $t('auth.quote') }}</p>
      </div>
    </div>

    <!-- ── Right Panel: Form ── -->
    <div class="auth-right d-flex flex-column align-items-center justify-content-center p-4 p-md-5">
      <!-- Mobile brand -->
      <div class="d-flex d-lg-none align-items-center gap-2 mb-4">
        <div class="brand-icon-sm d-flex align-items-center justify-content-center">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
            <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" stroke="white" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <span class="fw-bold text-body fs-5">TutaFlow</span>
      </div>

      <div class="form-panel w-100">

        <!-- ── Register Form ─────────────────── -->
        <template v-if="!showOtpVerification">
          <div class="mb-4">
            <h1 class="fw-bold text-body mb-1" style="font-size: 1.75rem;">{{ $t('auth.register_title') }}</h1>
            <p class="text-muted small mb-0">{{ $t('auth.join_subtitle') }}</p>
          </div>

          <form @submit.prevent="handleRegister">
            <div class="mb-3">
              <label class="form-label fw-semibold text-body small mb-1">{{ $t('auth.email') }}</label>
              <div class="input-group-clean">
                <span class="input-icon"><i class="bi bi-envelope"></i></span>
                <input
                  v-model="email"
                  type="email"
                  class="form-control form-control-clean"
                  placeholder="you@example.com"
                  required
                />
              </div>
            </div>

            <div class="mb-3">
              <label class="form-label fw-semibold text-body small mb-1">{{ $t('auth.password') }}</label>
              <div class="input-group-clean">
                <span class="input-icon"><i class="bi bi-lock"></i></span>
                <input
                  v-model="password"
                  :type="showPassword ? 'text' : 'password'"
                  class="form-control form-control-clean border-end-0"
                  placeholder="At least 8 characters"
                  required
                />
                <button type="button" class="btn-eye" @click="showPassword = !showPassword" tabindex="-1">
                  <i class="bi" :class="showPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
              <!-- Password hint -->
              <div class="password-hint mt-2 p-2 rounded-2 d-flex gap-2 align-items-start">
                <i class="bi bi-info-circle-fill flex-shrink-0 mt-1" style="font-size: 0.75rem;"></i>
                <span>{{ $t('auth.password_hint') }}</span>
              </div>
            </div>

            <div class="mb-4">
              <label class="form-label fw-semibold text-body small mb-1">{{ $t('auth.confirm_password') }}</label>
              <div class="input-group-clean">
                <span class="input-icon"><i class="bi bi-shield-lock"></i></span>
                <input
                  v-model="confirmPassword"
                  :type="showConfirmPassword ? 'text' : 'password'"
                  class="form-control form-control-clean border-end-0"
                  placeholder="Re-enter your password"
                  required
                />
                <button type="button" class="btn-eye" @click="showConfirmPassword = !showConfirmPassword" tabindex="-1">
                  <i class="bi" :class="showConfirmPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
            </div>

            <div class="d-flex align-items-center gap-3 mb-3">
              <button
                class="btn btn-primary flex-grow-1 py-2 d-flex align-items-center justify-content-center gap-2 btn-sign"
                type="submit"
                :disabled="loading"
              >
                <span v-if="loading" class="spinner-border spinner-border-sm" role="status"></span>
                <span>{{ loading ? $t('auth.creating_acc') : $t('auth.create_account_btn') }}</span>
                <svg v-if="!loading" width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M5 12h14M12 5l7 7-7 7" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
              </button>

              <!-- Google Sign-In Icon Button -->
              <div id="google-login-btn" class="flex-shrink-0" style="width: 46px; height: 46px;"></div>
            </div>

            <div class="text-center text-muted small">
              {{ $t('auth.already_have_account') }}
              <router-link to="/login" class="text-primary fw-semibold text-decoration-none ms-1 hover-underline">{{ $t('auth.login') }}</router-link>
            </div>
          </form>
        </template>

        <!-- ── OTP Verification ─────────────── -->
        <template v-else>
          <div class="mb-4">
            <!-- OTP icon -->
            <div class="otp-icon-wrap mx-auto mb-3">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none"><path d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" stroke="#0d9488" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
            </div>
            <h1 class="fw-bold text-body mb-1 text-center" style="font-size: 1.6rem;">{{ $t('auth.verify_email') }}</h1>
            <p class="text-muted small mb-0 text-center">
              {{ $t('auth.otp_sent', { email: email }) }}
            </p>
          </div>

          <form @submit.prevent="handleVerifyOtp">
            <div class="mb-4">
              <label class="form-label fw-semibold text-body small mb-1 text-center d-block">{{ $t('auth.otp_code') }}</label>
              <div class="input-group-clean">
                <span class="input-icon"><i class="bi bi-shield-check"></i></span>
                <input
                  v-model="otpCode"
                  type="text"
                  class="form-control form-control-clean otp-input text-center"
                  placeholder="000000"
                  maxlength="6"
                  required
                />
              </div>
            </div>

            <button
              class="btn btn-primary w-100 py-2 d-flex align-items-center justify-content-center gap-2 btn-sign mb-3"
              type="submit"
              :disabled="loading"
            >
              <span v-if="loading" class="spinner-border spinner-border-sm" role="status"></span>
              <span>{{ loading ? $t('common.loading') : $t('auth.verify_continue') }}</span>
              <svg v-if="!loading" width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
            </button>

            <div class="d-flex justify-content-between align-items-center mt-2">
              <button type="button" class="btn btn-sm btn-outline-secondary d-flex align-items-center gap-1" @click="showOtpVerification = false">
                <i class="bi bi-arrow-left" style="font-size: 0.8rem;"></i> {{ $t('auth.back') }}
              </button>
              <button type="button" class="btn btn-sm btn-link text-primary text-decoration-none p-0" @click="handleResendOtp" :disabled="resending">
                <span v-if="resending" class="spinner-border spinner-border-sm me-1" role="status"></span>
                {{ $t('auth.resend') }}
              </button>
            </div>
          </form>
        </template>

      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { login as apiLogin, register, verifyOtp, resendOtp, googleLogin } from '../services/authService.js'
import { toastError, toastSuccess, extractMessage } from '../utils/swal.js'

const router = useRouter()
const { locale, t } = useI18n()

const changeLocale = (lang) => {
  locale.value = lang
  localStorage.setItem('locale', lang)
}

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const otpCode = ref('')
const loading = ref(false)
const resending = ref(false)
const showOtpVerification = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)

const renderGoogleButton = () => {
  if (window.google) {
    window.google.accounts.id.initialize({
      client_id: import.meta.env.VITE_GOOGLE_CLIENT_ID,
      callback: handleGoogleCredentialResponse,
      auto_select: false,
      cancel_on_tap_outside: true,
      locale: locale.value
    });

    const btnContainer = document.getElementById("google-login-btn");
    if (btnContainer) {
      btnContainer.innerHTML = ""; // Clear old button
      window.google.accounts.id.renderButton(
        btnContainer,
        { 
          type: "icon", 
          theme: "filled_blue", 
          size: "large", 
          shape: "circle"
        }
      );
    }
  }
}

onMounted(() => {
  renderGoogleButton();
})

watch(locale, () => {
  renderGoogleButton();
})

const handleGoogleCredentialResponse = async (response) => {
  try {
    loading.value = true
    const idToken = response.credential

    const res = await googleLogin(idToken)

    if (res?.data?.AccessToken) {
      localStorage.setItem('token', res.data.AccessToken)
      router.push('/projects')
    }
  } catch (error) {
    toastError(extractMessage(error, t('auth.google_register_failed')))
  } finally {
    loading.value = false
  }
}

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
    toastError(extractMessage(error, t('errors.default')))
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
    const loginRes = await apiLogin({
      Email: email.value,
      Password: password.value
    })

    if (loginRes?.data?.AccessToken) {
      localStorage.setItem('token', loginRes.data.AccessToken)
    }

    toastSuccess('Account verified and logged in successfully!')
    router.push('/projects')
  } catch (error) {
    toastError(extractMessage(error, t('errors.default')))
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
    toastError(extractMessage(error, t('errors.default')))
  } finally {
    resending.value = false
  }
}
</script>

<style scoped>
/* ── Shell Layout ───────────────────────────────────────── */
.auth-shell {
  background-color: var(--bs-body-bg);
}

/* ── Left Panel ─────────────────────────────────────────── */
.auth-left {
  flex: 0 0 52%;
  background: linear-gradient(145deg, #0a1f1e 0%, #0f2926 40%, #0d3330 100%);
  position: relative;
  overflow: hidden;
}

.dot-pattern {
  position: absolute;
  inset: 0;
  background-image: radial-gradient(rgba(255, 255, 255, 0.06) 1px, transparent 1px);
  background-size: 28px 28px;
  pointer-events: none;
}

.auth-left::before {
  content: '';
  position: absolute;
  width: 380px;
  height: 380px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(13, 148, 136, 0.18) 0%, transparent 70%);
  top: -80px;
  right: -80px;
  pointer-events: none;
}
.auth-left::after {
  content: '';
  position: absolute;
  width: 280px;
  height: 280px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(167, 139, 250, 0.08) 0%, transparent 70%);
  bottom: -40px;
  left: 30px;
  pointer-events: none;
}

/* Brand */
.brand-icon {
  width: 44px;
  height: 44px;
  background: linear-gradient(135deg, #0d9488, #14b8a6);
  border-radius: 12px;
  box-shadow: 0 4px 14px rgba(13, 148, 136, 0.4);
}
.brand-name {
  letter-spacing: -0.02em;
}
.text-teal-light {
  color: #5eead4;
}

/* Feature list */
.features-list {
  display: flex;
  flex-direction: column;
  gap: 0;
}
.feature-item {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 14px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  animation: fade-in-up 0.5s ease both;
}
.feature-item:nth-child(1) { animation-delay: 0.1s; }
.feature-item:nth-child(2) { animation-delay: 0.2s; }
.feature-item:nth-child(3) { animation-delay: 0.3s; }
.feature-item:nth-child(4) { animation-delay: 0.4s; }

.feature-icon-wrap {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ── Right Panel ────────────────────────────────────────── */
.auth-right {
  flex: 1;
  background-color: var(--bs-body-bg);
  min-height: 100vh;
}

.form-panel {
  max-width: 400px;
}

/* Mobile brand */
.brand-icon-sm {
  width: 34px;
  height: 34px;
  background: linear-gradient(135deg, #0d9488, #14b8a6);
  border-radius: 9px;
}

/* Custom input style */
.input-group-clean {
  position: relative;
}
.input-icon {
  position: absolute;
  left: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--bs-secondary-color);
  z-index: 4;
  font-size: 0.9rem;
  pointer-events: none;
}
.form-control-clean {
  padding-left: 40px !important;
  background-color: var(--bs-secondary-bg) !important;
  border: 1.5px solid var(--bs-border-color) !important;
  border-radius: 10px !important;
  height: 46px;
  font-size: 0.92rem;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}
.form-control-clean:focus {
  border-color: #0d9488 !important;
  box-shadow: 0 0 0 3px rgba(13, 148, 136, 0.12) !important;
  background-color: var(--bs-card-bg) !important;
}
.btn-eye {
  position: absolute;
  right: 0;
  top: 0;
  height: 46px;
  width: 44px;
  background: transparent;
  border: none;
  color: var(--bs-secondary-color);
  cursor: pointer;
  z-index: 4;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0 10px 10px 0;
  transition: color 0.15s ease;
}
.btn-eye:hover { color: var(--bs-body-color); }

/* Password hint */
.password-hint {
  font-size: 0.73rem;
  font-weight: 500;
  line-height: 1.4;
  border: 1px solid rgba(13, 148, 136, 0.15);
  background-color: rgba(13, 148, 136, 0.05);
  color: #0f766e;
}
[data-bs-theme="dark"] .password-hint {
  background-color: rgba(13, 148, 136, 0.1);
  color: #2dd4bf;
  border-color: rgba(13, 148, 136, 0.2);
}

/* OTP */
.otp-input {
  letter-spacing: 8px;
  font-size: 1.4rem;
  font-weight: 700;
  text-align: center;
}
.otp-icon-wrap {
  width: 64px;
  height: 64px;
  background: rgba(13, 148, 136, 0.1);
  border: 2px solid rgba(13, 148, 136, 0.2);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Sign button */
.btn-sign {
  height: 46px;
  border-radius: 10px !important;
  font-weight: 600;
  font-size: 0.95rem;
  letter-spacing: 0.01em;
}

/* Link hover */
.hover-underline:hover {
  text-decoration: underline !important;
}

/* Responsive */
@media (max-width: 991.98px) {
  .auth-right {
    justify-content: flex-start;
    padding-top: 2.5rem !important;
  }
  .form-panel {
    max-width: 460px;
    margin: 0 auto;
  }
}
</style>
