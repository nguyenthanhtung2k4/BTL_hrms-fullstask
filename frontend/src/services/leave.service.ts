import { apiClient, extractData } from './apiClient'
import type { LeaveRequest, LeaveType, CreateLeaveRequestDto } from '../types/attendance.types'

const BASE = '/api/v1/attendance/leave-requests'

export const leaveService = {
  getAll: (params?: object) => apiClient.get<{ data: LeaveRequest[] }>(BASE, { params }).then(extractData),
  getTypes: () => apiClient.get<{ data: LeaveType[] }>(`${BASE}/types`).then(extractData),
  getMyRequests: () => apiClient.get<{ data: LeaveRequest[] }>(`${BASE}/me`).then(extractData),
  create: (dto: CreateLeaveRequestDto) =>
    apiClient.post<{ data: LeaveRequest }>(`${BASE}/me`, dto).then(extractData),
  approve: (id: string) =>
    apiClient.put(`${BASE}/${id}/approve`),
  reject: (id: string) =>
    apiClient.put(`${BASE}/${id}/reject`),
  cancel: (id: string) =>
    apiClient.put(`${BASE}/${id}/cancel`),
}
