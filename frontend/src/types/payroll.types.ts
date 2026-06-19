// Payroll Types

export type PeriodStatus = 'Draft' | 'Calculated' | 'Closed'

export interface PayrollPeriod {
  id: string
  code: string
  name: string
  fromDate: string
  toDate: string
  status: PeriodStatus
  payrollRuleId: string
  payrollRuleName?: string
  createdAt: string
}

export interface CreatePayrollPeriodDto {
  code: string
  name: string
  fromDate: string
  toDate: string
  payrollRuleId: string
}

export interface UpdatePayrollPeriodDto {
  name: string
  fromDate: string
  toDate: string
  payrollRuleId: string
}

// ---

export interface PayrollRule {
  id: string
  code: string
  name: string
  workDayHours: number
  paidLeaveCountsAsWork: boolean
  overtimeRate: number
  isActive: boolean
  createdAt: string
}

export interface CreatePayrollRuleDto {
  code: string
  name: string
  workDayHours: number
  paidLeaveCountsAsWork: boolean
  overtimeRate: number
  isActive?: boolean
}

// ---

export interface AllowanceType {
  id: string
  code: string
  name: string
  isActive: boolean
}

export interface EmployeeAllowance {
  id: string
  payrollPeriodId: string
  periodName?: string
  employeeId: string
  employeeName?: string
  allowanceTypeId: string
  allowanceTypeName?: string
  amount: number
  notes?: string
  createdAt: string
}

export interface CreateAllowanceDto {
  payrollPeriodId: string
  employeeId: string
  allowanceTypeId: string
  amount: number
  notes?: string
}

// ---

export interface DeductionType {
  id: string
  code: string
  name: string
  isActive: boolean
}

export interface EmployeeDeduction {
  id: string
  payrollPeriodId: string
  periodName?: string
  employeeId: string
  employeeName?: string
  deductionTypeId: string
  deductionTypeName?: string
  amount: number
  notes?: string
  createdAt: string
}

export interface CreateDeductionDto {
  payrollPeriodId: string
  employeeId: string
  deductionTypeId: string
  amount: number
  notes?: string
}

// ---

export interface PayslipItem {
  id: string
  type: 'Earning' | 'Deduction'
  name: string
  amount: number
}

export interface Payslip {
  id: string
  payrollPeriodId: string
  periodName?: string
  employeeId: string
  employeeName?: string
  departmentName?: string
  positionName?: string
  baseSalary: number
  actualWorkDays: number
  standardWorkDays: number
  salaryByWork: number
  totalAllowances: number
  totalDeductions: number
  grossSalary: number
  netSalary: number
  items?: PayslipItem[]
  createdAt: string
}

// ---

export interface PayrollSummaryReport {
  departmentId: string
  departmentName: string
  employeeCount: number
  totalWorkDays: number
  totalAllowances: number
  totalDeductions: number
  totalGross: number
  totalNet: number
}

// --- Generic API Response wrapper
export interface ApiResponse<T> {
  success: boolean
  message: string
  data: T
  errors?: string[]
}
