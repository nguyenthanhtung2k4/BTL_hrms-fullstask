import { apiClient, extractData } from './apiClient'
import type { PayrollPeriod, CreatePayrollPeriodDto, UpdatePayrollPeriodDto } from '../types/payroll.types'

const BASE = '/api/v1/payroll/payroll-periods'

export const payrollPeriodService = {
  getAll: (params?: object) => apiClient.get<{ data: PayrollPeriod[] }>(BASE, { params }).then(extractData),
  getById: (id: string) => apiClient.get<{ data: PayrollPeriod }>(`${BASE}/${id}`).then(extractData),
  create: (dto: CreatePayrollPeriodDto) => apiClient.post<{ data: PayrollPeriod }>(BASE, dto).then(extractData),
  update: (id: string, dto: UpdatePayrollPeriodDto) => apiClient.put<{ data: PayrollPeriod }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
  calculate: (id: string) => apiClient.post(`${BASE}/${id}/calculate`),
  close: (id: string) => apiClient.post(`${BASE}/${id}/close`),
}
