import { apiClient, extractData } from './apiClient'
import type { Employee, CreateEmployeeDto, UpdateEmployeeDto, ChangeStatusDto } from '../types/hr.types'

const BASE = '/api/v1/hr/employees'

export const employeeService = {
  getAll: () => apiClient.get<{ data: Employee[] }>(BASE).then(extractData),
  getById: (id: string) => apiClient.get<{ data: Employee }>(`${BASE}/${id}`).then(extractData),
  create: (dto: CreateEmployeeDto) => apiClient.post<{ data: Employee }>(BASE, dto).then(extractData),
  update: (id: string, dto: UpdateEmployeeDto) => apiClient.put<{ data: Employee }>(`${BASE}/${id}`, dto).then(extractData),
  changeStatus: (id: string, dto: ChangeStatusDto) => apiClient.put(`${BASE}/${id}/status`, dto),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
}
