import { apiClient, extractData } from './apiClient'
import type { EmployeeAllowance, AllowanceType, CreateAllowanceDto } from '../types/payroll.types'

const BASE = '/api/v1/payroll/allowances'

export const allowanceService = {
  getAll: (params?: object) => apiClient.get<{ data: EmployeeAllowance[] }>(BASE, { params }).then(extractData),
  getTypes: () => apiClient.get<{ data: AllowanceType[] }>(`${BASE}/types`).then(extractData),
  create: (dto: CreateAllowanceDto) => apiClient.post<{ data: EmployeeAllowance }>(BASE, dto).then(extractData),
  update: (id: string, dto: Partial<CreateAllowanceDto>) => apiClient.put<{ data: EmployeeAllowance }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
  createType: (name: string) => apiClient.post<{ data: AllowanceType }>(`${BASE}/types`, { name }).then(extractData),
}
