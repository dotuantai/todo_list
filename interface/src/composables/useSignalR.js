import { HubConnectionBuilder } from '@microsoft/signalr'
import { useProjectStore } from '../stores/projectStore.js'

let signalRConnection = null

export function useSignalR() {
  const projectStore = useProjectStore()

  const initSignalR = async () => {
    if (signalRConnection) return
    const token = localStorage.getItem('token')
    if (!token) return

    const baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7087/api'
    const hubUrl = baseUrl.replace('/api', '/hubs/notifications')

    signalRConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build()

    signalRConnection.on('ReceiveNotification', (n) => {
      window.dispatchEvent(new CustomEvent('notification-received', { detail: n }))
    })

    signalRConnection.on('TaskCreated', (task) => {
      window.dispatchEvent(new CustomEvent('task-created', { detail: task }))
    })

    signalRConnection.on('TaskUpdated', (task) => {
      window.dispatchEvent(new CustomEvent('task-updated', { detail: task }))
    })

    signalRConnection.on('TaskDeleted', (taskId) => {
      window.dispatchEvent(new CustomEvent('task-deleted', { detail: taskId }))
    })

    signalRConnection.onreconnected(async (connectionId) => {
      console.log(`SignalR reconnected: ${connectionId}`)
      if (projectStore.currentUserId) {
        await signalRConnection.invoke('RegisterUser', projectStore.currentUserId)
      }
      if (projectStore.currentProjectId) {
        await signalRConnection.invoke('JoinProject', projectStore.currentProjectId)
      }
    })

    try {
      await signalRConnection.start()
      console.log('SignalR connected.')
      if (projectStore.currentUserId) {
        await signalRConnection.invoke('RegisterUser', projectStore.currentUserId)
      }
      if (projectStore.currentProjectId) {
        await signalRConnection.invoke('JoinProject', projectStore.currentProjectId)
      }
    } catch (err) {
      console.error('SignalR connection failed', err)
    }
  }

  const stopSignalR = async () => {
    if (signalRConnection) {
      try {
        await signalRConnection.stop()
      } catch (e) {}
      signalRConnection = null
    }
  }

  const joinProject = async (projectId) => {
    if (signalRConnection && signalRConnection.state === 'Connected') {
      try {
        await signalRConnection.invoke('JoinProject', projectId)
        console.log(`Joined SignalR project group: ${projectId}`)
      } catch (err) {
        console.error('Failed to join SignalR project group', err)
      }
    }
  }

  const leaveProject = async (projectId) => {
    if (signalRConnection && signalRConnection.state === 'Connected') {
      try {
        await signalRConnection.invoke('LeaveProject', projectId)
        console.log(`Left SignalR project group: ${projectId}`)
      } catch (err) {
        console.error('Failed to leave SignalR project group', err)
      }
    }
  }

  return {
    initSignalR,
    stopSignalR,
    joinProject,
    leaveProject,
    get connection() {
      return signalRConnection
    }
  }
}
