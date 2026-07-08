<template>
  <div v-if="showAppShell" class="d-flex vh-100 overflow-hidden">
    <!-- Sidebar -->
    <nav class="sidebar d-flex flex-column flex-shrink-0">

      <div class="px-4 py-3 border-bottom">
        <div class="d-flex align-items-center gap-3">
          <div class="logo-box d-flex align-items-center justify-content-center fw-bold text-white fs-5">TF</div>
          <div>
            <h1 class="mb-0 fs-6 fw-bold text-dark lh-1">TaskFlow Pro</h1>
            <p class="small text-muted mb-0 mt-1">Enterprise Plan</p>
          </div>
        </div>
      </div>

      <div class="px-3 py-3">
        <button class="btn btn-primary w-100 py-2 fw-semibold d-flex align-items-center justify-content-center gap-2" @click="openCreateTaskModal">
          <i class="bi bi-plus-lg"></i> Create Task
        </button>
      </div>

      <div class="flex-grow-1 px-2 overflow-auto">
        <ul class="nav flex-column gap-1">
          <li class="nav-item">
            <router-link to="/" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3">
              <i class="bi bi-grid-1x2-fill fs-6"></i>
              <span class="small fw-medium">Dashboard</span>
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/tasks" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3">
              <i class="bi bi-list-task fs-6"></i>
              <span class="small fw-medium">My Tasks</span>
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/board" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3">
              <i class="bi bi-kanban fs-6"></i>
              <span class="small fw-medium">Task Board</span>
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/reports" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3">
              <i class="bi bi-bar-chart fs-6"></i>
              <span class="small fw-medium">Reports</span>
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/settings" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3">
              <i class="bi bi-gear fs-6"></i>
              <span class="small fw-medium">Settings</span>
            </router-link>
          </li>
        </ul>
      </div>

      <div class="px-2 py-3 border-top">
        <ul class="nav flex-column gap-1">
          <li class="nav-item">
            <router-link to="/help" class="nav-link sidebar-link d-flex align-items-center gap-2 px-3 py-2 rounded-3">
              <i class="bi bi-question-circle fs-6"></i>
              <span class="small fw-medium">Help</span>
            </router-link>
          </li>
          <li class="nav-item">
            <button
              @click="handleLogout"
              class="btn btn-link nav-link sidebar-link-danger d-flex align-items-center gap-2 px-3 py-2 rounded-3 w-100 text-start text-decoration-none">
              <i class="bi bi-box-arrow-right fs-6"></i>
              <span class="small fw-medium">Logout</span>
            </button>
          </li>
        </ul>
      </div>

    </nav>

    <!-- Main area -->
    <div class="flex-grow-1 d-flex flex-column overflow-hidden">

      <header class="top-navbar px-4 d-flex align-items-center justify-content-between border-bottom bg-white flex-shrink-0">

        <div style="max-width:380px; width:100%">
          <div class="input-group input-group-sm">
            <span class="input-group-text bg-white border-end-0 text-muted">
              <i class="bi bi-search"></i>
            </span>
            <input type="text" class="form-control border-start-0 ps-0" placeholder="Search tasks..." />
          </div>
        </div>

        <div class="d-flex align-items-center gap-3">
          <button class="icon-btn">
            <i class="bi bi-bell"></i>
            <span class="notif-dot"></span>
          </button>
          <button class="icon-btn"><i class="bi bi-clock-history"></i></button>
          <div class="vr opacity-25 mx-1" style="height:28px"></div>
          <div class="d-flex align-items-center gap-2">
            <img src="https://i.pravatar.cc/150?img=12" class="avatar" alt="avatar" />
            <div class="d-none d-md-block">
              <div class="fw-semibold small lh-1">Alex Rivera</div>
              <div class="text-muted" style="font-size:11px; margin-top:2px">Project Manager</div>
            </div>
          </div>
        </div>

      </header>

      <main class="flex-grow-1 overflow-auto bg-body-secondary">
        <router-view />
      </main>

    </div>

  </div>


  <router-view v-else />

  <TaskModal ref="createTaskModal" />
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import TaskModal from './components/TaskModal.vue'
import { logout } from './services/authService.js'

const router = useRouter()
const route = useRoute()
const createTaskModal = ref(null)

const openCreateTaskModal = () => {
  createTaskModal.value?.openModal()
}

const authRoutes = ['/login', '/register']
const showAppShell = computed(() => !authRoutes.includes(route.path))

const handleLogout = async () => {
  try {
    await logout()
  } catch (error) {
    console.error(error)
  } finally {
    localStorage.removeItem('token')
    router.push('/login')      
  }
}
</script>

<style scoped>
.sidebar {
  width: 260px;
  min-height: 100vh;
  background: #ffffff;
  border-right: 1px solid #e2e8f0;
}

.logo-box {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  flex-shrink: 0;
}

.top-navbar {
  height: 60px;
}

.sidebar-link {
  color: #64748b;
  font-size: 1rem;
  transition: background 0.15s, color 0.15s;
  text-decoration: none;
}
.sidebar-link:hover {
  background: #f1f5f9;
  color: #0f172a;
}
.router-link-active,
.router-link-exact-active {
  background: linear-gradient(135deg, #4f46e5, #6366f1) !important;
  color: #fff !important;
  box-shadow: 0 4px 14px rgba(79, 70, 229, 0.25);
}

.sidebar-link-danger {
  color: #ef4444;
}
.sidebar-link-danger:hover {
  background: #fff1f2;
  color: #dc2626;
}

.btn-primary {
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  border: none;
  font-size: 0.85rem;
  border-radius: 10px;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.25);
  transition: opacity 0.15s;
}
.btn-primary:hover { opacity: 0.9; background: linear-gradient(135deg, #4338ca, #4f46e5); border: none; }

.input-group-text { border-color: #e2e8f0; }
.form-control { border-color: #e2e8f0; font-size: 0.85rem; }
.form-control:focus { box-shadow: none; border-color: #6366f1; }
.input-group:focus-within .input-group-text { border-color: #6366f1; }

.icon-btn {
  position: relative;
  border: none;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  width: 36px; height: 36px;
  border-radius: 9px;
  font-size: 16px;
  color: #64748b;
  display: flex; align-items: center; justify-content: center;
  transition: background 0.15s, color 0.15s;
  cursor: pointer;
}
.icon-btn:hover { background: #f1f5f9; color: #4f46e5; }

.notif-dot {
  position: absolute;
  top: 6px; right: 7px;
  width: 7px; height: 7px;
  border-radius: 50%;
  background: #ef4444;
  border: 1.5px solid #fff;
}

.avatar {
  width: 36px; height: 36px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #e2e8f0;
}
</style>