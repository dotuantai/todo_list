
import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import TaskView from '../views/TaskView.vue'
import DashboardView from '../views/DashboardView.vue'

const routes = [
  {
    path: '/',
    component: LoginView
  },
  {
    path: '/tasks',
    component: TaskView
  },
   {
    path: '/dashboard',
    component: DashboardView
  }
]

export default createRouter({
  history: createWebHistory(),
  routes
})