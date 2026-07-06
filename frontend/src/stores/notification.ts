import { defineStore } from 'pinia'
import { notificationService, type NotificationDto } from '../services/notification.service'

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [] as NotificationDto[],
    loading: false,
    pollIntervalId: null as any
  }),

  getters: {
    unreadCount: (state) => state.notifications.filter(n => !n.isRead).length
  },

  actions: {
    async fetchNotifications() {
      this.loading = true
      try {
        const data = await notificationService.getMyNotifications()
        this.notifications = data
      } catch (error) {
        console.error('Failed to fetch notifications:', error)
      } finally {
        this.loading = false
      }
    },

    async markAsRead(id: string) {
      try {
        await notificationService.markAsRead(id)
        const notif = this.notifications.find(n => n.id === id)
        if (notif) {
          notif.isRead = true
        }
      } catch (error) {
        console.error('Failed to mark notification as read:', error)
      }
    },

    async markAllAsRead() {
      try {
        await notificationService.markAllAsRead()
        this.notifications.forEach(n => {
          n.isRead = true
        })
      } catch (error) {
        console.error('Failed to mark all notifications as read:', error)
      }
    },

    startPolling(intervalMs = 30000) {
      if (this.pollIntervalId) return
      this.fetchNotifications()
      this.pollIntervalId = setInterval(() => {
        this.fetchNotifications()
      }, intervalMs)
    },

    stopPolling() {
      if (this.pollIntervalId) {
        clearInterval(this.pollIntervalId)
        this.pollIntervalId = null
      }
    }
  }
})
