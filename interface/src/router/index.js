import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import TaskView from '../views/TaskView.vue'
import DashboardView from '../views/DashboardView.vue'

const routes = [
  {
    path: '/login',
    component: LoginView
  },
  {
    path: '/tasks',
    component: TaskView,
    meta: { requiresAuth: true }
  },
  {
    path: '/',
    component: DashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/login'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')

  if (to.path === '/login') {
    if (token) {
      return next('/tasks')
    }
    return next()
  }

  if (to.meta.requiresAuth && !token) {
    return next('/login')
  }

  return next()
})

export default router