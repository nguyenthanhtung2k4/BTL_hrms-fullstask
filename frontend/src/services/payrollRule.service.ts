import { apiClient, extractData } from './apiClient'
import type { PayrollRule, CreatePayrollRuleDto } from '../types/payroll.types'

const BASE = '/api/v1/payroll/payroll-rules'

export const payrollRuleService = {
  getAll: () => apiClient.get<{ data: PayrollRule[] }>(BASE).then(extractData),
  getById: (id: string) => apiClient.get<{ data: PayrollRule }>(`${BASE}/${id}`).then(extractData),
  create: (dto: CreatePayrollRuleDto) => apiClient.post<{ data: PayrollRule }>(BASE, dto).then(extractData),
  update: (id: string, dto: Partial<CreatePayrollRuleDto>) => apiClient.put<{ data: PayrollRule }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
}
