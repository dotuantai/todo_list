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
            <svg v-else viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-btn-login"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-btn-login)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
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
              <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-item-login"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-item-login)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
              <span style="font-size: 0.85rem;">English</span>
            </button>
          </li>
        </ul>
      </div>
    </div>


    <!-- ── Left Panel: Illustration ── -->
    <div class="auth-left d-none d-lg-flex flex-column align-items-center justify-content-center p-5">
      <!-- Dot pattern overlay -->
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

      <!-- Illustration: Kanban Board SVG -->
      <div class="illustration-wrapper position-relative">
        <svg class="kanban-illustration" viewBox="0 0 520 380" fill="none" xmlns="http://www.w3.org/2000/svg">
          <!-- Board shadow -->
          <rect x="20" y="20" width="480" height="340" rx="16" fill="rgba(0,0,0,0.15)"/>
          <!-- Board background -->
          <rect x="16" y="16" width="480" height="340" rx="16" fill="#ffffff" fill-opacity="0.08"/>
          <rect x="16" y="16" width="480" height="340" rx="16" stroke="rgba(255,255,255,0.15)" stroke-width="1"/>

          <!-- Header bar -->
          <rect x="16" y="16" width="480" height="44" rx="16" fill="rgba(255,255,255,0.06)"/>
          <rect x="16" y="44" width="480" height="16" fill="rgba(255,255,255,0.06)"/>
          <circle cx="42" cy="38" r="6" fill="#ef4444" fill-opacity="0.7"/>
          <circle cx="58" cy="38" r="6" fill="#f59e0b" fill-opacity="0.7"/>
          <circle cx="74" cy="38" r="6" fill="#10b981" fill-opacity="0.7"/>
          <text x="210" y="43" font-family="sans-serif" font-size="11" fill="rgba(255,255,255,0.6)" text-anchor="middle">TutaFlow — Task Board</text>

          <!-- Column 1: To Do -->
          <rect x="32" y="72" width="104" height="268" rx="10" fill="rgba(255,255,255,0.05)"/>
          <rect x="40" y="80" width="42" height="6" rx="3" fill="rgba(255,255,255,0.3)"/>
          <circle cx="128" cy="83" r="8" fill="rgba(255,255,255,0.1)"/>
          <text x="128" y="87" font-family="sans-serif" font-size="8" fill="rgba(255,255,255,0.5)" text-anchor="middle">3</text>
          <!-- Cards col 1 -->
          <rect x="38" y="98" width="90" height="56" rx="8" fill="rgba(255,255,255,0.12)"/>
          <rect x="38" y="98" width="4" height="56" rx="2" fill="#94a3b8"/>
          <rect x="48" y="107" width="60" height="5" rx="2.5" fill="rgba(255,255,255,0.5)"/>
          <rect x="48" y="116" width="44" height="4" rx="2" fill="rgba(255,255,255,0.25)"/>
          <rect x="48" y="132" width="30" height="12" rx="6" fill="rgba(148,163,184,0.2)"/>
          <text x="63" y="141" font-family="sans-serif" font-size="7" fill="rgba(255,255,255,0.4)">To Do</text>

          <rect x="38" y="162" width="90" height="50" rx="8" fill="rgba(255,255,255,0.12)"/>
          <rect x="38" y="162" width="4" height="50" rx="2" fill="#94a3b8"/>
          <rect x="48" y="171" width="50" height="5" rx="2.5" fill="rgba(255,255,255,0.5)"/>
          <rect x="48" y="180" width="38" height="4" rx="2" fill="rgba(255,255,255,0.25)"/>
          <rect x="48" y="194" width="26" height="10" rx="5" fill="rgba(148,163,184,0.2)"/>

          <rect x="38" y="220" width="90" height="50" rx="8" fill="rgba(255,255,255,0.12)"/>
          <rect x="38" y="220" width="4" height="50" rx="2" fill="#94a3b8"/>
          <rect x="48" y="229" width="54" height="5" rx="2.5" fill="rgba(255,255,255,0.5)"/>
          <rect x="48" y="238" width="35" height="4" rx="2" fill="rgba(255,255,255,0.25)"/>

          <!-- Column 2: In Progress -->
          <rect x="148" y="72" width="104" height="268" rx="10" fill="rgba(255,255,255,0.05)"/>
          <rect x="156" y="80" width="58" height="6" rx="3" fill="rgba(255,255,255,0.3)"/>
          <circle cx="244" cy="83" r="8" fill="rgba(56,189,248,0.2)"/>
          <text x="244" y="87" font-family="sans-serif" font-size="8" fill="rgba(56,189,248,0.7)" text-anchor="middle">2</text>
          <!-- Cards col 2 - highlighted -->
          <rect x="154" y="98" width="90" height="60" rx="8" fill="rgba(56,189,248,0.12)"/>
          <rect x="154" y="98" width="4" height="60" rx="2" fill="#38bdf8"/>
          <rect x="164" y="107" width="64" height="5" rx="2.5" fill="rgba(255,255,255,0.6)"/>
          <rect x="164" y="116" width="48" height="4" rx="2" fill="rgba(255,255,255,0.3)"/>
          <rect x="164" y="124" width="36" height="4" rx="2" fill="rgba(255,255,255,0.2)"/>
          <rect x="164" y="136" width="38" height="12" rx="6" fill="rgba(56,189,248,0.2)"/>
          <text x="183" y="145" font-family="sans-serif" font-size="7" fill="#38bdf8">In Progress</text>

          <rect x="154" y="166" width="90" height="55" rx="8" fill="rgba(56,189,248,0.08)"/>
          <rect x="154" y="166" width="4" height="55" rx="2" fill="#38bdf8"/>
          <rect x="164" y="175" width="52" height="5" rx="2.5" fill="rgba(255,255,255,0.6)"/>
          <rect x="164" y="184" width="40" height="4" rx="2" fill="rgba(255,255,255,0.25)"/>

          <!-- Column 3: Done -->
          <rect x="264" y="72" width="104" height="268" rx="10" fill="rgba(255,255,255,0.05)"/>
          <rect x="272" y="80" width="36" height="6" rx="3" fill="rgba(255,255,255,0.3)"/>
          <circle cx="360" cy="83" r="8" fill="rgba(52,211,153,0.2)"/>
          <text x="360" y="87" font-family="sans-serif" font-size="8" fill="rgba(52,211,153,0.7)" text-anchor="middle">4</text>
          <!-- Cards done with checkmarks -->
          <rect x="270" y="98" width="90" height="50" rx="8" fill="rgba(52,211,153,0.1)"/>
          <rect x="270" y="98" width="4" height="50" rx="2" fill="#34d399"/>
          <rect x="280" y="107" width="56" height="5" rx="2.5" fill="rgba(255,255,255,0.5)"/>
          <rect x="280" y="116" width="40" height="4" rx="2" fill="rgba(255,255,255,0.25)"/>
          <circle cx="350" cy="113" r="7" fill="rgba(52,211,153,0.2)"/>
          <path d="M346 113l2.5 2.5 4-4" stroke="#34d399" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>

          <rect x="270" y="156" width="90" height="50" rx="8" fill="rgba(52,211,153,0.08)"/>
          <rect x="270" y="156" width="4" height="50" rx="2" fill="#34d399"/>
          <rect x="280" y="165" width="48" height="5" rx="2.5" fill="rgba(255,255,255,0.5)"/>
          <rect x="280" y="174" width="36" height="4" rx="2" fill="rgba(255,255,255,0.2)"/>

          <rect x="270" y="214" width="90" height="45" rx="8" fill="rgba(52,211,153,0.08)"/>
          <rect x="270" y="214" width="4" height="45" rx="2" fill="#34d399"/>
          <rect x="280" y="223" width="52" height="5" rx="2.5" fill="rgba(255,255,255,0.5)"/>

          <!-- Column 4: Closed -->
          <rect x="380" y="72" width="104" height="268" rx="10" fill="rgba(255,255,255,0.05)"/>
          <rect x="388" y="80" width="40" height="6" rx="3" fill="rgba(255,255,255,0.3)"/>
          <circle cx="476" cy="83" r="8" fill="rgba(251,146,60,0.2)"/>
          <text x="476" y="87" font-family="sans-serif" font-size="8" fill="rgba(251,146,60,0.7)" text-anchor="middle">1</text>
          <rect x="386" y="98" width="90" height="50" rx="8" fill="rgba(251,146,60,0.08)"/>
          <rect x="386" y="98" width="4" height="50" rx="2" fill="#fb923c"/>
          <rect x="396" y="107" width="50" height="5" rx="2.5" fill="rgba(255,255,255,0.4)"/>
          <rect x="396" y="116" width="38" height="4" rx="2" fill="rgba(255,255,255,0.2)"/>

          <!-- Floating avatar badge -->
          <g transform="translate(180, 88)">
            <circle r="14" fill="#0d9488" stroke="white" stroke-width="2"/>
            <text font-family="sans-serif" font-size="10" fill="white" text-anchor="middle" dominant-baseline="middle">AJ</text>
          </g>
        </svg>

        <!-- Floating feature badges -->
        <div class="feature-badge badge-top-right">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none"><path d="M13 10V3L4 14h7v7l9-11h-7z" stroke="#34d399" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
          <span>{{ $t('auth.realtime_sync') }}</span>
        </div>
        <div class="feature-badge badge-bottom-left">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none"><path d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0" stroke="#38bdf8" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
          <span>{{ $t('auth.team_collab') }}</span>
        </div>
      </div>

      <!-- Tagline -->
      <div class="text-center mt-5 position-relative">
        <h2 class="text-white fw-bold mb-2" style="font-size: 1.6rem; line-height: 1.3;">
          {{ $t('auth.slogan_title') }}<br><span class="text-teal-light">{{ $t('auth.slogan_highlight') }}</span>
        </h2>
        <p class="text-white-50 small mb-0">{{ $t('auth.slogan_subtitle') }}</p>
      </div>
    </div>

    <!-- ── Right Panel: Form ── -->
    <div class="auth-right d-flex flex-column align-items-center justify-content-center p-4 p-md-5">
      <!-- Mobile brand header -->
      <div class="d-flex d-lg-none align-items-center gap-2 mb-4">
        <div class="brand-icon-sm d-flex align-items-center justify-content-center">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
            <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" stroke="white" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <span class="fw-bold text-body fs-5">TutaFlow</span>
      </div>

      <div class="form-panel w-100">
        <!-- Header -->
        <div class="mb-4">
          <h1 class="fw-bold text-body mb-1" style="font-size: 1.75rem;">{{ $t('auth.welcome_back') }}</h1>
          <p class="text-muted small mb-0">{{ $t('auth.welcome_subtitle') }}</p>
        </div>

        <!-- Login Form -->
        <form @submit.prevent="login">
          <div class="mb-3">
            <label class="form-label fw-semibold text-body small mb-1">{{ $t('auth.email') }}</label>
            <div class="input-group input-group-clean">
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

          <div class="mb-4">
            <label class="form-label fw-semibold text-body small mb-1">{{ $t('auth.password') }}</label>
            <div class="input-group input-group-clean">
              <span class="input-icon"><i class="bi bi-lock"></i></span>
              <input
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                class="form-control form-control-clean ps-icon border-end-0"
                placeholder="Enter your password"
                @keyup.enter="login"
                required
              />
              <button type="button" class="btn-eye" @click="showPassword = !showPassword" tabindex="-1">
                <i class="bi" :class="showPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
              </button>
            </div>
          </div>

          <div class="d-flex align-items-center gap-3">
            <button
              class="btn btn-primary flex-grow-1 py-2 d-flex align-items-center justify-content-center gap-2 btn-sign"
              type="submit"
              :disabled="loading"
            >
              <span v-if="loading" class="spinner-border spinner-border-sm" role="status"></span>
              <span>{{ loading ? $t('auth.signing_in') : $t('auth.login') }}</span>
              <svg v-if="!loading" width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M5 12h14M12 5l7 7-7 7" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
            </button>

            <!-- Google Login Icon Button -->
            <div id="google-login-btn" class="flex-shrink-0" style="width: 46px; height: 46px;"></div>
          </div>
        </form>

        <!-- Divider -->
        <div class="divider-row my-4">
          <span class="divider-line"></span>
          <span class="divider-text">{{ $t('auth.new_here') }}</span>
          <span class="divider-line"></span>
        </div>

        <router-link to="/register" class="btn btn-outline-secondary w-100 py-2 d-flex align-items-center justify-content-center gap-2 text-decoration-none">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M16 21v-2a4 4 0 00-4-4H6a4 4 0 00-4 4v2M12 11a4 4 0 100-8 4 4 0 000 8zM19 8v6M22 11h-6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
          {{ $t('auth.create_free_acc') }}
        </router-link>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { loginn, googleLogin } from '../services/authService.js'
import { toastError, extractMessage } from '../utils/swal.js'

const router = useRouter()
const { locale, t } = useI18n()

const changeLocale = (lang) => {
  locale.value = lang
  localStorage.setItem('locale', lang)
}

const email = ref('')
const password = ref('')
const loading = ref(false)
const showPassword = ref(false)

const renderGoogleButton = () => {
  console.log('google loaded:', !!window.google, 'client_id:', import.meta.env.VITE_GOOGLE_CLIENT_ID)
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
          type: "icon", // Chỉ hiển thị logo Google
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
    toastError(extractMessage(error, t('auth.google_login_failed')))
  } finally {
    loading.value = false
  }
}

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

/* Ambient glow blobs */
.auth-left::before {
  content: '';
  position: absolute;
  width: 400px;
  height: 400px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(13, 148, 136, 0.18) 0%, transparent 70%);
  top: -80px;
  right: -80px;
  pointer-events: none;
}
.auth-left::after {
  content: '';
  position: absolute;
  width: 320px;
  height: 320px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(52, 211, 153, 0.1) 0%, transparent 70%);
  bottom: -60px;
  left: 20px;
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

/* Illustration */
.illustration-wrapper {
  width: 100%;
  max-width: 520px;
  position: relative;
}
.kanban-illustration {
  width: 100%;
  height: auto;
  filter: drop-shadow(0 20px 40px rgba(0,0,0,0.35));
  animation: float 6s ease-in-out infinite;
}
@keyframes float {
  0%, 100% { transform: translateY(0px); }
  50% { transform: translateY(-8px); }
}

/* Feature badges */
.feature-badge {
  position: absolute;
  display: flex;
  align-items: center;
  gap: 6px;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 999px;
  padding: 6px 12px;
  font-size: 0.72rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.85);
  white-space: nowrap;
  animation: fade-in-up 0.6s ease both;
}
.badge-top-right {
  top: 12px;
  right: -12px;
  animation-delay: 0.4s;
}
.badge-bottom-left {
  bottom: 16px;
  left: -8px;
  animation-delay: 0.7s;
}
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(10px); }
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

/* Sign in button */
.btn-sign {
  height: 46px;
  border-radius: 10px !important;
  font-weight: 600;
  font-size: 0.95rem;
  letter-spacing: 0.01em;
}

/* Divider */
.divider-row {
  display: flex;
  align-items: center;
  gap: 12px;
}
.divider-line {
  flex: 1;
  height: 1px;
  background-color: var(--bs-border-color);
}
.divider-text {
  font-size: 0.8rem;
  color: var(--bs-secondary-color);
  white-space: nowrap;
  font-weight: 500;
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