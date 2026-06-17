import { apiClient, extractData } from './apiClient'
import type { PayrollSummaryReport } from '../types/payroll.types'

export const reportService = {
  getSummary: (params?: object) =>
    apiClient.get<{ data: PayrollSummaryReport[] }>('/api/v1/payroll/reports/summary', { params }).then(extractData),
}
