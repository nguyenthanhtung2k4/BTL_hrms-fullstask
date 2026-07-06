import apiClient from './apiClient'

export interface NotificationDto {
  id: string
  employeeId: string | null
  title: string
  content: string
  type: string
  isRead: boolean
  createdAt: string
}

export const notificationService = {
  async getMyNotifications() {
    const response = await apiClient.get<NotificationDto[]>('/api/v1/hr/notifications')
    return response.data
  },

  async markAsRead(id: string) {
    const response = await apiClient.put(`/api/v1/hr/notifications/${id}/read`)
    return response.data
  },

  async markAllAsRead() {
    const response = await apiClient.put('/api/v1/hr/notifications/read-all')
    return response.data
  }
}
