<template>
  <div class="d-flex vh-100 overflow-hidden bg-body-tertiary">
    <!-- Admin Sidebar -->
    <nav class="d-flex flex-column flex-shrink-0 text-white shadow" style="width: 260px; height: 100vh; background-color: #1E1B4B; z-index: 1045;">
      <div class="px-4 py-3 d-flex align-items-center border-bottom" style="border-color: rgba(255,255,255,0.1) !important;">
        <div class="text-start min-w-0">
          <h1 class="mb-0 fs-5 fw-bold lh-1 text-truncate">TutaFlow Admin</h1>
          <p class="small mb-0 mt-1" style="color: #818CF8; font-size: 11px;">Enterprise System</p>
        </div>
      </div>
      
      <div class="flex-grow-1 px-3 py-4 overflow-auto">
        <div class="small fw-bold text-uppercase mb-3 ps-3" style="color: #6366F1; letter-spacing: 1px; font-size: 0.75rem;">Workspace</div>
        <ul class="nav flex-column gap-2">
          <li class="nav-item">
            <router-link to="/admin/projects" class="nav-link admin-link d-flex align-items-center gap-3 px-3 py-2 rounded-3 text-white" active-class="active-admin">
              <i class="bi bi-briefcase-fill fs-5"></i>
              <span class="fw-medium">Quản lý Dự án</span>
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/admin/users" class="nav-link admin-link d-flex align-items-center gap-3 px-3 py-2 rounded-3 text-white" active-class="active-admin">
              <i class="bi bi-people-fill fs-5"></i>
              <span class="fw-medium">Danh sách Nhân sự</span>
            </router-link>
          </li>
        </ul>
      </div>

      <div class="px-3 py-4 border-top" style="border-color: rgba(255,255,255,0.1) !important;">
        <button @click="handleLogout" class="btn btn-link text-decoration-none d-flex align-items-center gap-3 px-3 w-100 logout-btn rounded-3" style="color: #ef4444;">
          <i class="bi bi-box-arrow-right fs-5"></i>
          <span class="fw-medium">Đăng xuất</span>
        </button>
      </div>
    </nav>
    
    <!-- Main Content Area -->
    <main class="flex-grow-1 d-flex flex-column overflow-hidden">
      <!-- Admin Header -->
      <header class="px-4 py-3 bg-white border-bottom d-flex justify-content-between align-items-center flex-shrink-0" style="height: 60px;">
        <h2 class="h5 mb-0 fw-bold text-body">TutaFlow Admin Dashboard</h2>
        <div class="d-flex align-items-center gap-3">
          <div class="text-end lh-1">
            <div class="fw-semibold text-body small">{{ projectStore.currentUserEmail }}</div>
            <div class="text-muted" style="font-size: 11px;">Quản trị viên hệ thống</div>
          </div>
          <div class="user-avatar-small bg-primary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle" style="width: 36px; height: 36px; font-size: 14px; background-color: #6366F1 !important;">
            {{ projectStore.currentInitial }}
          </div>
        </div>
      </header>
      
      <!-- Content View -->
      <div class="flex-grow-1 p-4 overflow-auto" style="background-color: #F5F3FF;">
        <router-view />
      </div>
    </main>
  </div>
</template>

<script setup>
import { useProjectStore } from '../stores/projectStore.js'
import { useRouter } from 'vue-router'
import { logout } from '../services/authService.js'

const projectStore = useProjectStore()
const router = useRouter()

const handleLogout = async () => {
  try {
    await logout()
  } catch (error) {
    console.error(error)
  } finally {
    localStorage.removeItem('token')
    projectStore.clearStore()
    router.push('/login')
  }
}
</script>

<style scoped>
.admin-link {
  color: rgba(255,255,255,0.7) !important;
  transition: all 0.2s ease;
}
.admin-link:hover {
  color: white !important;
  background-color: rgba(255,255,255,0.05);
}
.active-admin {
  background-color: #6366F1 !important;
  color: white !important;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.4);
}
.logout-btn {
  transition: all 0.2s ease;
}
.logout-btn:hover {
  background-color: rgba(239, 68, 68, 0.1);
}
</style>
