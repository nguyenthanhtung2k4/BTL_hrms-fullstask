import { apiClient, extractData } from './apiClient'
import type { LeaveRequest, LeaveType, CreateLeaveRequestDto } from '../types/attendance.types'

const BASE = '/api/v1/attendance/leaves'

export const leaveService = {
  getAll: (params?: object) => apiClient.get<{ data: LeaveRequest[] }>(BASE, { params }).then(extractData),
  getTypes: () => apiClient.get<{ data: LeaveType[] }>(`${BASE}/types`).then(extractData),
  create: (employeeId: string, dto: CreateLeaveRequestDto) =>
    apiClient.post<{ data: LeaveRequest }>(BASE, dto, { params: { employeeId } }).then(extractData),
  approve: (id: string, approvedByEmployeeId: string) =>
    apiClient.post(`${BASE}/${id}/approve`, { approvedByEmployeeId }),
  reject: (id: string, approvedByEmployeeId: string) =>
    apiClient.post(`${BASE}/${id}/reject`, { approvedByEmployeeId }),
  cancel: (id: string, employeeId: string) =>
    apiClient.post(`${BASE}/${id}/cancel`, { employeeId }),
}
