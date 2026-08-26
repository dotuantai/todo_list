<template>
  <div class="d-flex vh-100 overflow-hidden bg-body-tertiary" style="font-family: 'Nunito Sans', sans-serif;">
    <!-- Admin Sidebar -->
    <nav class="d-flex flex-column flex-shrink-0 bg-white border-end" style="width: 260px; height: 100vh; z-index: 1045;">
      <div class="px-4 py-4 d-flex align-items-center gap-3">
        <div class="bg-primary text-white rounded-3 d-flex align-items-center justify-content-center shadow-sm" style="width: 40px; height: 40px;">
          <i class="bi bi-clipboard2-check-fill fs-5"></i>
        </div>
        <div class="text-start min-w-0">
          <h1 class="mb-0 fs-6 fw-bold lh-1 text-truncate text-dark">{{ $t('admin.SCR0901') }}</h1>
          <p class="small mb-0 mt-1 text-muted" style="font-size: 11px;">{{ $t('admin.SCR0902') }}</p>
        </div>
      </div>
      
      <div class="flex-grow-1 px-3 py-2 overflow-auto custom-scrollbar">
        <div class="small fw-bold text-uppercase mb-2 ps-3 text-muted" style="letter-spacing: 0.5px; font-size: 0.7rem;">{{ $t('admin.SCR0974') }}</div>
        <ul class="nav flex-column gap-1 mb-4">
          <li class="nav-item">
            <router-link to="/admin/dashboard" class="nav-link admin-link d-flex align-items-center gap-3 px-4 py-2 rounded-pill text-body" active-class="active-admin">
              <i class="bi bi-bar-chart-fill fs-5"></i>
              <span class="fw-semibold" style="font-size: 0.95rem;">{{ $t('admin.SCR0910') }}</span>
            </router-link>
          </li>
        </ul>

        <div class="small fw-bold text-uppercase mb-2 ps-3 text-muted mt-2" style="letter-spacing: 0.5px; font-size: 0.7rem;">{{ $t('admin.SCR0975') }}</div>
        <ul class="nav flex-column gap-1 mb-4">
          <li class="nav-item">
            <router-link to="/admin/projects" class="nav-link admin-link d-flex align-items-center gap-3 px-4 py-2 rounded-pill text-body" active-class="active-admin">
              <i class="bi bi-folder-fill fs-5"></i>
              <span class="fw-semibold" style="font-size: 0.95rem;">{{ $t('admin.SCR0904') }}</span>
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/admin/users" class="nav-link admin-link d-flex align-items-center gap-3 px-4 py-2 rounded-pill text-body" active-class="active-admin">
              <i class="bi bi-people-fill fs-5"></i>
              <span class="fw-semibold" style="font-size: 0.95rem;">{{ $t('admin.SCR0905') }}</span>
            </router-link>
          </li>
        </ul>
      </div>

      <div class="p-3 border-top bg-white">
        <div class="card border-0 bg-light rounded-4">
          <div class="card-body p-3 d-flex align-items-center gap-3">
             <div class="user-avatar-small text-white d-flex align-items-center justify-content-center fw-bold rounded-circle shadow-sm" style="width: 40px; height: 40px; font-size: 16px; background-color: #2563EB;">
                {{ projectStore.currentInitial }}
             </div>
             <div class="flex-grow-1 min-w-0">
               <div class="fw-bold text-truncate text-dark" style="font-size: 0.9rem;">{{ projectStore.currentUserEmail.split('@')[0] }}</div>
               <div class="text-muted text-truncate" style="font-size: 0.75rem;">{{ $t('common.SCR0027') }}</div>
             </div>
          </div>
          <div class="border-top px-3 py-2">
            <button @click="handleLogout" class="btn btn-link text-decoration-none text-muted d-flex align-items-center gap-2 px-0 w-100 logout-btn" style="font-size: 0.85rem;">
              <i class="bi bi-box-arrow-right"></i>
              <span class="fw-medium">{{ $t('admin.SCR0906') }}</span>
            </button>
          </div>
        </div>
      </div>
    </nav>
    
    <!-- Main Content Area -->
    <main class="flex-grow-1 d-flex flex-column overflow-hidden bg-body-tertiary">
      <!-- Admin Header -->
      <header class="px-4 py-3 border-bottom d-flex justify-content-between align-items-center flex-shrink-0 bg-white" style="height: 70px;">
        <h2 class="h5 mb-0 fw-bold text-dark">{{ $t('admin.SCR0907') }}</h2>
        <div class="d-flex align-items-center gap-3">
          <!-- Language Switcher -->
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
                <svg v-else viewBox="0 0 24 24" width="20" height="20" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-btn-admin"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-btn-admin)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
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
                  <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg" class="rounded-circle"><clipPath id="uk-circle-item-admin"><circle cx="12" cy="12" r="12"/></clipPath><g clip-path="url(#uk-circle-item-admin)"><rect width="24" height="24" fill="#012169"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#fff" stroke-width="4"/><path d="M0,0 L24,24 M24,0 L0,24" stroke="#C8102E" stroke-width="2"/><path d="M0,12 H24 M12,0 V24" stroke="#fff" stroke-width="6"/><path d="M0,12 H24 M12,0 V24" stroke="#C8102E" stroke-width="4"/></g></svg>
                  <span style="font-size: 0.85rem;">{{ $t('common.SCR0020') }}</span>
                </button>
              </li>
            </ul>
          </div>

          <div class="text-end lh-1 d-none d-sm-block">
            <div class="fw-semibold text-body small">{{ projectStore.currentUserEmail }}</div>
            <div class="text-muted" style="font-size: 11px;">{{ $t('admin.SCR0908') }}</div>
          </div>
          
          <div class="dropdown">
            <button class="btn btn-link p-0 border-0 text-decoration-none d-flex align-items-center" type="button" data-bs-toggle="dropdown" aria-expanded="false">
              <div class="user-avatar-small text-white d-flex align-items-center justify-content-center fw-bold rounded-circle shadow-sm" style="width: 38px; height: 38px; font-size: 15px; background-color: #6366F1 !important; transition: transform 0.2s;">
                {{ projectStore.currentInitial }}
              </div>
            </button>
            <ul class="dropdown-menu dropdown-menu-end shadow-sm border-0 mt-2" style="border-radius: 12px; min-width: 220px; padding: 8px 0;">
              <li class="px-3 py-2 border-bottom mb-2 d-sm-none">
                <div class="fw-bold text-body" style="font-size: 0.9rem;">{{ projectStore.currentUserEmail }}</div>
                <div class="text-muted small">{{ $t('common.SCR0036') }}</div>
              </li>
              <li>
                <router-link to="/change-password" class="dropdown-item py-2 px-3 d-flex align-items-center gap-3 text-secondary fw-medium">
                  <i class="bi bi-key text-muted fs-5"></i> {{ $t('admin.SCR0909') }}
                </router-link>
              </li>
              <li><hr class="dropdown-divider my-1"></li>
              <li>
                <button @click="handleLogout" class="dropdown-item py-2 px-3 d-flex align-items-center gap-3 text-danger fw-medium">
                  <i class="bi bi-box-arrow-right fs-5"></i> {{ $t('admin.SCR0906') }}
                </button>
              </li>
            </ul>
          </div>
        </div>
      </header>
      
      <!-- Content View -->
      <div class="flex-grow-1 p-4 p-md-5 overflow-auto custom-scrollbar" style="background-color: #F8FAFC;">
        <router-view />
      </div>
    </main>
  </div>
</template>

<script setup>
import { useProjectStore } from '../stores/projectStore.js'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { logout } from '../services/authService.js'

const projectStore = useProjectStore()
const router = useRouter()
const { locale } = useI18n()

const changeLocale = (lang) => {
  locale.value = lang
  localStorage.setItem('locale', lang)
}

const handleLogout = async () => {
  try {
    await logout()
  } catch (error) {
    console.error(error)
  } finally {
    projectStore.clearStore()
    router.push('/login')
  }
}
</script>

<style scoped>
.admin-link {
  color: #64748B !important;
  transition: all 0.2s ease;
}
.admin-link:hover {
  background-color: #F1F5F9;
  color: #0F172A !important;
}
.active-admin {
  background-color: #E0F2FE !important;
  color: #0284C7 !important;
}
.logout-btn {
  transition: all 0.2s ease;
}
.logout-btn:hover {
  color: #EF4444 !important;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(0,0,0,0.1);
  border-radius: 10px;
}
.custom-scrollbar:hover::-webkit-scrollbar-thumb {
  background-color: rgba(0,0,0,0.2);
}
</style>
