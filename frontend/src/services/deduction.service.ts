import { apiClient, extractData } from './apiClient'
import type { EmployeeDeduction, DeductionType, CreateDeductionDto } from '../types/payroll.types'

const BASE = '/api/v1/payroll/deductions'

export const deductionService = {
  getAll: (params?: object) => apiClient.get<{ data: EmployeeDeduction[] }>(BASE, { params }).then(extractData),
  getTypes: () => apiClient.get<{ data: DeductionType[] }>(`${BASE}/types`).then(extractData),
  create: (dto: CreateDeductionDto) => apiClient.post<{ data: EmployeeDeduction }>(BASE, dto).then(extractData),
  update: (id: string, dto: Partial<CreateDeductionDto>) => apiClient.put<{ data: EmployeeDeduction }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
}
