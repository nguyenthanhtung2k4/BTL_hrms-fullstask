import { apiClient } from './apiClient'
import type { PayrollSummaryReport } from '../types/payroll.types'

interface BackendDepartmentSummary {
  departmentId: string
  departmentCode: string
  departmentName: string
  employeeCount: number
  totalBaseSalary: number
  totalWorkedDays: number
  totalAllowance: number
  totalDeduction: number
  totalGrossSalary: number
  totalNetSalary: number
}

interface BackendPayrollSummaryReport {
  payrollPeriodId: string
  periodName: string
  totalEmployees: number
  totalBaseSalary: number
  totalAllowance: number
  totalGrossSalary: number
  totalDeduction: number
  totalNetSalary: number
  departments: BackendDepartmentSummary[]
}

export const reportService = {
  getSummary: async (params?: { payrollPeriodId?: string }): Promise<PayrollSummaryReport[]> => {
    // Map frontend's payrollPeriodId to backend's expected periodId
    const backendParams = params?.payrollPeriodId ? { periodId: params.payrollPeriodId } : {}
    
    const response = await apiClient.get<{ data: BackendPayrollSummaryReport }>('/api/v1/payroll/reports/summary', { params: backendParams })
    const reportData = response.data.data

    if (!reportData || !reportData.departments) {
      return []
    }

    return reportData.departments.map((dept) => ({
      departmentId: dept.departmentId,
      departmentName: dept.departmentName,
      employeeCount: dept.employeeCount,
      totalWorkDays: dept.totalWorkedDays,
      totalAllowances: dept.totalAllowance,
      totalDeductions: dept.totalDeduction,
      totalGross: dept.totalGrossSalary,
      totalNet: dept.totalNetSalary,
    }))
  },
}

