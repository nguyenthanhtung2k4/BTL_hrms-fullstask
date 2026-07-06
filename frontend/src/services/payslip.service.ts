import { apiClient, extractData } from './apiClient'
import type { Payslip } from '../types/payroll.types'

const BASE = '/api/v1/payroll/payslips'

export const payslipService = {
  getAll: (params?: object) => apiClient.get<{ data: Payslip[] }>(BASE, { params }).then(extractData),
  getById: (id: string) => apiClient.get<{ data: Payslip }>(`${BASE}/${id}`).then(extractData),
  getMyPayslips: () => apiClient.get<{ data: Payslip[] }>(`${BASE}/me`).then(extractData),
  update: (id: string, data: { workedDays: number; paidLeaveDays: number }) => apiClient.put<{ data: Payslip }>(`${BASE}/${id}`, data).then(extractData),
}
