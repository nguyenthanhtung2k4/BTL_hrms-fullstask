import { apiClient, extractData } from './apiClient'
import type { Timesheet } from '../types/attendance.types'

const BASE = '/api/v1/attendance/timesheets'

export const timesheetService = {
  getAll: (params?: object) => apiClient.get<{ data: Timesheet[] }>(BASE, { params }).then(extractData),
  calculate: (month: number, year: number) =>
    apiClient.post(`${BASE}/recalculate?year=${year}&month=${month}`),
}
