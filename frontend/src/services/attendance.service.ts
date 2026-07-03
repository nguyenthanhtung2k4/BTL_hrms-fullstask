import { apiClient, extractData } from './apiClient'
import type { AttendanceRecord, AttendanceAdjustment, CreateAdjustmentDto } from '../types/attendance.types'

const BASE = '/api/v1/attendance/records'
const ADJ_BASE = '/api/v1/attendance/adjustments'

export const attendanceService = {
  getAll: (params?: object) => apiClient.get<{ data: AttendanceRecord[] }>(BASE, { params }).then(extractData),

  getMyToday: () =>
    apiClient.get<{ data: AttendanceRecord[] }>(`${BASE}/me`).then((r) => r.data.data),

  getMyRecords: (params?: { fromDate?: string; toDate?: string }) =>
    apiClient.get<{ data: AttendanceRecord[] }>(`${BASE}/me`, { params }).then((r) => r.data.data),

  checkIn: (shiftCode?: string, reason?: string) =>
    apiClient.post(`${BASE}/check-in`, { shiftCode: shiftCode || undefined, reason: reason || undefined }),

  checkOut: (reason?: string) =>
    apiClient.post(`${BASE}/check-out`, { reason: reason || undefined }),

  // Adjustments
  getAdjustments: (params?: { employeeId?: string; status?: string }) =>
    apiClient.get<{ data: AttendanceAdjustment[] }>(ADJ_BASE, { params }).then(extractData),

  getMyAdjustments: () =>
    apiClient.get<{ data: AttendanceAdjustment[] }>(`${ADJ_BASE}/me`).then((r) => r.data.data),

  createAdjustment: (dto: CreateAdjustmentDto) =>
    apiClient.post<{ data: AttendanceAdjustment }>(`${ADJ_BASE}/me`, dto).then((r) => r.data.data),

  approveAdjustment: (id: string) =>
    apiClient.put<{ data: AttendanceAdjustment }>(`${ADJ_BASE}/${id}/approve`).then((r) => r.data.data),

  rejectAdjustment: (id: string) =>
    apiClient.put<{ data: AttendanceAdjustment }>(`${ADJ_BASE}/${id}/reject`).then((r) => r.data.data),
}
