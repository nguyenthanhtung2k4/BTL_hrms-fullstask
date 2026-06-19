import { apiClient, extractData } from './apiClient'
import type { Shift, CreateShiftDto, UpdateShiftDto } from '../types/attendance.types'

const BASE = '/api/v1/attendance/shifts'

export const shiftService = {
  getAll: () => apiClient.get<{ data: Shift[] }>(BASE).then(extractData),
  getById: (id: string) => apiClient.get<{ data: Shift }>(`${BASE}/${id}`).then(extractData),
  create: (dto: CreateShiftDto) => apiClient.post<{ data: Shift }>(BASE, dto).then(extractData),
  update: (id: string, dto: UpdateShiftDto) => apiClient.put<{ data: Shift }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
}
