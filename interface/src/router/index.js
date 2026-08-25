import { createRouter, createWebHistory } from 'vue-router'
import { useProjectStore } from '../stores/projectStore.js'
const LoginView = () => import('../views/LoginView.vue')
const ProjectsView = () => import('../views/ProjectsView.vue')
const DashboardView = () => import('../views/DashboardView.vue')
const TaskView = () => import('../views/TaskView.vue')
const CalendarView = () => import('../views/CalendarView.vue')
const GanttView = () => import('../views/GanttView.vue')
const SettingsView = () => import('../views/SettingsView.vue')
const MembersView = () => import('../views/MembersView.vue')
const ChangePasswordView = () => import('../views/ChangePasswordView.vue')
const ForceChangePasswordView = () => import('../views/ForceChangePasswordView.vue')
const ForgotPasswordView = () => import('../views/ForgotPasswordView.vue')

// Admin Views
const AdminLayout = () => import('../layouts/AdminLayout.vue')
const AdminDashboardView = () => import('../views/admin/AdminDashboardView.vue')
const AdminProjectsView = () => import('../views/admin/AdminProjectsView.vue')
const AdminUsersView = () => import('../views/admin/AdminUsersView.vue')

const routes = [
  {
    path: '/login',
    name: 'login',
    component: LoginView
  },
  {
    path: '/change-password',
    name: 'change-password',
    component: ChangePasswordView,
    meta: { requiresAuth: true }
  },
  {
    path: '/force-change-password',
    name: 'force-change-password',
    component: ForceChangePasswordView,
    meta: { requiresAuth: true }
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: ForgotPasswordView
  },
  {
    path: '/projects',
    name: 'projects',
    component: ProjectsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/projects/:projectId/dashboard',
    name: 'project-dashboard',
    component: DashboardView,
    meta: { requiresAuth: true, requiresProject: true }
  },
  {
    path: '/projects/:projectId/tasks',
    name: 'project-tasks',
    component: TaskView,
    meta: { requiresAuth: true, requiresProject: true }
  },
  {
    path: '/projects/:projectId/calendar',
    name: 'project-calendar',
    component: CalendarView,
    meta: { requiresAuth: true, requiresProject: true }
  },
  {
    path: '/projects/:projectId/gantt',
    name: 'project-gantt',
    component: GanttView,
    meta: { requiresAuth: true, requiresProject: true }
  },
  {
    path: '/projects/:projectId/settings',
    name: 'project-settings',
    component: SettingsView,
    meta: { requiresAuth: true, requiresProject: true }
  },
  {
    path: '/projects/:projectId/members',
    name: 'project-members',
    component: MembersView,
    meta: { requiresAuth: true, requiresProject: true }
  },
  // --- ADMIN ROUTES ---
  {
    path: '/admin',
    component: AdminLayout,
    meta: { requiresAuth: true, requiresAdmin: true },
    children: [
      {
        path: '',
        redirect: '/admin/dashboard'
      },
      {
        path: 'dashboard',
        name: 'admin-dashboard',
        component: AdminDashboardView
      },
      {
        path: 'projects',
        name: 'admin-projects',
        component: AdminProjectsView
      },
      {
        path: 'users',
        name: 'admin-users',
        component: AdminUsersView
      }
    ]
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/projects'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from) => {
  const store = useProjectStore()
  
  if (!store.currentUserEmail) {
    store.decodeToken()
  }

  const token = localStorage.getItem('token')
  const isAuthenticated = !!token && token !== 'null' && token !== 'undefined' && token.split('.').length === 3
  const requiresPasswordChange = localStorage.getItem('requiresPasswordChange') === 'true'

  if (to.path === '/login') {
    if (isAuthenticated) {
      if (requiresPasswordChange) return '/force-change-password'
      if (store.appRole === 'Admin') return '/admin/dashboard'
      return '/projects'
    }
    return true
  }

  if (to.meta.requiresAuth && !isAuthenticated) {
    store.clearStore()
    return '/login'
  }

  if (isAuthenticated && requiresPasswordChange && to.path !== '/force-change-password') {
    return '/force-change-password'
  }

  if (isAuthenticated && !requiresPasswordChange && to.path === '/force-change-password') {
    if (store.appRole === 'Admin') return '/admin/projects'
    return '/projects'
  }

  if (to.meta.requiresAdmin && store.appRole !== 'Admin') {
    return '/projects'
  }

  if (to.meta.requiresProject) {
    const projectId = to.params.projectId
    
    if (store.projects.length === 0) {
      await store.fetchProjects()
    }
    
    const exists = store.projects.some(p => p.Id === projectId)
    if (!exists) {
      return '/projects'
    }
    
    store.setCurrentProjectId(projectId)
  }

  return true
})

export default router