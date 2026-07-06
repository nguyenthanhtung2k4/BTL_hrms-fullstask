import { apiClient, extractData } from './apiClient'
import type { WorkSchedule, CreateWorkScheduleDto, UpdateWorkScheduleDto } from '../types/attendance.types'

const BASE = '/api/v1/attendance/work-schedules'

export const workScheduleService = {
  getAll: (params?: object) => apiClient.get<{ data: WorkSchedule[] }>(BASE, { params }).then(extractData),
  getById: (id: string) => apiClient.get<{ data: WorkSchedule }>(`${BASE}/${id}`).then(extractData),
  create: (dto: CreateWorkScheduleDto) => apiClient.post<{ data: WorkSchedule }>(BASE, dto).then(extractData),
  update: (id: string, dto: UpdateWorkScheduleDto) => apiClient.put<{ data: WorkSchedule }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
}
