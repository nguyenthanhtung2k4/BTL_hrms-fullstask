import { apiClient, extractData } from './apiClient'
import type { AttendanceRecord } from '../types/attendance.types'

const BASE = '/api/v1/attendance/attendance'

export const attendanceService = {
  getAll: (params?: object) => apiClient.get<{ data: AttendanceRecord[] }>(BASE, { params }).then(extractData),

  getMyToday: (employeeId: string) =>
    apiClient.get<{ data: AttendanceRecord | null }>(`${BASE}/my-today`, { params: { employeeId } }).then((r) => r.data.data),

  checkIn: (employeeId: string) =>
    apiClient.post(`${BASE}/check-in`, { employeeId }),

  checkOut: (employeeId: string) =>
    apiClient.post(`${BASE}/check-out`, { employeeId }),
}
