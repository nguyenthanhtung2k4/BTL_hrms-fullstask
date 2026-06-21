import { apiClient, extractData } from './apiClient'
import type { AttendanceRecord } from '../types/attendance.types'

const BASE = '/api/v1/attendance/records'

export const attendanceService = {
  getAll: (params?: object) => apiClient.get<{ data: AttendanceRecord[] }>(BASE, { params }).then(extractData),

  getMyToday: () =>
    apiClient.get<{ data: AttendanceRecord[] }>(`${BASE}/me`).then((r) => r.data.data),

  getMyRecords: (params?: { fromDate?: string; toDate?: string }) =>
    apiClient.get<{ data: AttendanceRecord[] }>(`${BASE}/me`, { params }).then((r) => r.data.data),

  checkIn: (shiftCode?: string) =>
    apiClient.post(`${BASE}/check-in`, { shiftCode: shiftCode || undefined }),

  checkOut: () =>
    apiClient.post(`${BASE}/check-out`),
}
