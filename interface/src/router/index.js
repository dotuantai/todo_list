import { createRouter, createWebHistory } from 'vue-router'
import { useProjectStore } from '../stores/projectStore.js'
const LoginView = () => import('../views/LoginView.vue')
const RegisterView = () => import('../views/RegisterView.vue')
const ProjectsView = () => import('../views/ProjectsView.vue')
const DashboardView = () => import('../views/DashboardView.vue')
const TaskView = () => import('../views/TaskView.vue')
const SettingsView = () => import('../views/SettingsView.vue')
const MembersView = () => import('../views/MembersView.vue')
const ChangePasswordView = () => import('../views/ChangePasswordView.vue')

const routes = [
  {
    path: '/login',
    name: 'login',
    component: LoginView
  },
  {
    path: '/register',
    name: 'register',
    component: RegisterView
  },
  {
    path: '/change-password',
    name: 'change-password',
    component: ChangePasswordView,
    meta: { requiresAuth: true }
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

  if (to.path === '/login' || to.path === '/register') {
    if (isAuthenticated) {
      return '/projects'
    }
    return true
  }

  if (to.meta.requiresAuth && !isAuthenticated) {
    store.clearStore()
    return '/login'
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