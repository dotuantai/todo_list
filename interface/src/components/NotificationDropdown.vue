<template>
  <div class="position-relative notification-wrapper">
    <button 
      @click="toggleNotificationDropdown" 
      class="btn btn-light p-0 border rounded-3 d-flex align-items-center justify-content-center" 
      style="width: 36px; height: 36px; color: #64748b; position: relative;"
      title="Notifications"
    >
      <i class="bi" :class="unreadCount > 0 ? 'bi-bell-fill text-primary' : 'bi-bell'"></i>
      <span v-if="unreadCount > 0" class="position-absolute bg-danger border border-white rounded-circle" style="width: 8px; height: 8px; top: 7px; right: 7px;"></span>
    </button>

    <!-- Dropdown panel -->
    <div 
      v-if="showNotifications" 
      class="card shadow-lg border position-absolute mt-2 p-0 notification-dropdown" 
      style="width: 320px; right: 0; z-index: 1050; border-radius: 12px; overflow: hidden;"
    >
      <div class="card-header bg-white border-bottom py-2.5 px-3 d-flex align-items-center justify-content-between">
        <span class="fw-bold text-body small text-uppercase tracking-wider mb-0" style="font-size: 11px;">Notifications</span>
        <button 
          v-if="unreadCount > 0" 
          @click="handleMarkAllAsRead" 
          class="btn btn-link p-0 text-decoration-none small text-primary fw-semibold" 
          style="font-size: 11px;"
        >
          Mark all as read
        </button>
      </div>
      <div class="list-group list-group-flush overflow-auto" style="max-height: 350px;">
        <div v-if="notifications.length === 0" class="text-center py-4 text-muted small fst-italic">
          <i class="bi bi-bell-slash d-block fs-4 opacity-50 mb-1"></i>
          No notifications
        </div>
        <button 
          v-else
          v-for="n in notifications" 
          :key="n.Id" 
          @click="handleNotificationClick(n)"
          class="list-group-item list-group-item-action text-start p-3 border-bottom d-flex align-items-start gap-2"
          :style="!n.IsRead ? { backgroundColor: 'rgba(99, 102, 241, 0.03)' } : {}"
        >
          <div class="flex-grow-1 min-w-0">
            <div class="d-flex align-items-center justify-content-between gap-2 mb-1">
              <span class="fw-bold text-body text-truncate" style="font-size: 12.5px;">{{ n.Title }}</span>
              <span v-if="!n.IsRead" class="badge rounded-circle p-1 bg-primary" style="width: 6px; height: 6px;" title="Unread"></span>
            </div>
            <p class="text-secondary mb-1 small lh-sm" style="font-size: 12px;">{{ n.Message }}</p>
            <span class="text-muted" style="font-size: 9.5px; font-family: monospace;">{{ formatTime(n.CreatedAt) }}</span>
          </div>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { getNotifications, markAsRead, markAllAsRead } from '../services/notificationService.js'
import { useProjectStore } from '../stores/projectStore.js'

const projectStore = useProjectStore()
const notifications = ref([])
const showNotifications = ref(false)
const unreadCount = computed(() => notifications.value.filter(n => !n.IsRead).length)

const loadNotifications = async () => {
  if (!projectStore.isAuthenticated) return
  try {
    const res = await getNotifications()
    notifications.value = res?.data || []
  } catch (e) {
    console.error('Failed to load notifications:', e)
  }
}

const toggleNotificationDropdown = async () => {
  showNotifications.value = !showNotifications.value
  if (showNotifications.value) {
    await loadNotifications()
  }
}

const handleNotificationClick = async (n) => {
  if (n.IsRead) return
  try {
    await markAsRead(n.Id)
    n.IsRead = true
  } catch (e) {
    console.error('Failed to mark notification as read:', e)
  }
}

const handleMarkAllAsRead = async () => {
  try {
    await markAllAsRead()
    notifications.value.forEach(n => n.IsRead = true)
  } catch (e) {
    console.error('Failed to mark all as read:', e)
  }
}

const closeNotificationDropdownOnOutside = (e) => {
  if (!e.target.closest('.notification-wrapper')) {
    showNotifications.value = false
  }
}

const formatTime = (iso) => {
  if (!iso) return ''
  const dateObj = new Date(iso)
  return dateObj.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) + ' ' + dateObj.toLocaleDateString('en-US', { day: '2-digit', month: '2-digit' })
}

const onNotificationReceived = (e) => {
  if (e.detail) {
    notifications.value.unshift(e.detail)
  }
}

watch(() => projectStore.isAuthenticated, (newVal) => {
  if (newVal) {
    loadNotifications()
  } else {
    notifications.value = []
    showNotifications.value = false
  }
})

onMounted(() => {
  window.addEventListener('click', closeNotificationDropdownOnOutside)
  window.addEventListener('notification-received', onNotificationReceived)
  if (projectStore.isAuthenticated) {
    loadNotifications()
  }
})

onUnmounted(() => {
  window.removeEventListener('click', closeNotificationDropdownOnOutside)
  window.removeEventListener('notification-received', onNotificationReceived)
})
</script>

<style scoped>
.notification-dropdown {
  background: var(--bs-card-bg);
  border-color: var(--bs-border-color) !important;
  box-shadow: var(--shadow-lg) !important;
}
.notification-dropdown .list-group-item {
  transition: background-color 0.2s ease;
  border-bottom: 1px solid var(--bs-border-color) !important;
}
.notification-dropdown .list-group-item:hover {
  background-color: var(--bs-secondary-bg) !important;
}
</style>
