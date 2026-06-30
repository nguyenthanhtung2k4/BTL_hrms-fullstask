import { defineStore } from 'pinia'
import { notificationService, type NotificationDto } from '../services/notification.service'

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [] as NotificationDto[],
    loading: false,
    pollIntervalId: null as any
  }),

  getters: {
    // ĐÃ FIX: Kiểm tra chắc chắn nó là Mảng (Array) thì mới đếm, nếu không thì trả về 0
    unreadCount: (state) => {
      if (!Array.isArray(state.notifications)) return 0;
      return state.notifications.filter(n => !n.isRead).length;
    }
  },

  actions: {
    async fetchNotifications() {
      this.loading = true
      try {
        const data = await notificationService.getMyNotifications()
        // ĐÃ FIX: Chỉ gán data nếu nó thực sự là một mảng
        this.notifications = Array.isArray(data) ? data : []
      } catch (error) {
        console.error('Failed to fetch notifications:', error)
        this.notifications = [] // Reset về mảng rỗng nếu lỗi API
      } finally {
        this.loading = false
      }
    },

    async markAsRead(id: string) {
      try {
        await notificationService.markAsRead(id)
        if (Array.isArray(this.notifications)) {
          const notif = this.notifications.find(n => n.id === id)
          if (notif) {
            notif.isRead = true
          }
        }
      } catch (error) {
        console.error('Failed to mark notification as read:', error)
      }
    },

    async markAllAsRead() {
      try {
        await notificationService.markAllAsRead()
        if (Array.isArray(this.notifications)) {
          this.notifications.forEach(n => {
            n.isRead = true
          })
        }
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