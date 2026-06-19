import { apiClient, extractData } from './apiClient'
import type { Department, CreateDepartmentDto, UpdateDepartmentDto } from '../types/hr.types'

const BASE = '/api/v1/hr/departments'

export const departmentService = {
  getAll: () => apiClient.get<{ data: Department[] }>(BASE).then(extractData),
  getById: (id: string) => apiClient.get<{ data: Department }>(`${BASE}/${id}`).then(extractData),
  create: (dto: CreateDepartmentDto) => apiClient.post<{ data: Department }>(BASE, dto).then(extractData),
  update: (id: string, dto: UpdateDepartmentDto) => apiClient.put<{ data: Department }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
}
