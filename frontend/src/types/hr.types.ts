// HR Core Types

export interface Department {
  id: string
  code: string
  name: string
  description?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export interface CreateDepartmentDto {
  code: string
  name: string
  description?: string
  isActive?: boolean
}

export interface UpdateDepartmentDto {
  name: string
  description?: string
  isActive: boolean
}

// ---

export interface Position {
  id: string
  code: string
  name: string
  description?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export interface CreatePositionDto {
  code: string
  name: string
  description?: string
  isActive?: boolean
}

export interface UpdatePositionDto {
  name: string
  description?: string
  isActive: boolean
}

// ---

export type EmployeeStatus = 'Active' | 'Inactive' | 'OnLeave' | 'Resigned'

export interface Employee {
  id: string
  employeeCode: string
  fullName: string
  email: string
  phone?: string
  gender?: string
  dateOfBirth?: string
  hireDate: string
  departmentId: string
  departmentName: string
  positionId: string
  positionName: string
  managerEmployeeId?: string
  managerName?: string
  status: EmployeeStatus
  createdAt: string
  updatedAt?: string
}

export interface CreateEmployeeDto {
  employeeCode: string
  fullName: string
  email: string
  phone?: string
  gender?: string
  dateOfBirth?: string
  hireDate: string
  departmentId: string
  positionId: string
  managerEmployeeId?: string
}

export interface UpdateEmployeeDto {
  fullName: string
  phone?: string
  gender?: string
  dateOfBirth?: string
  hireDate: string
  departmentId: string
  positionId: string
  managerEmployeeId?: string
  status: EmployeeStatus
}

export interface ChangeStatusDto {
  newStatus: EmployeeStatus
  reason?: string
  changedByUserId: string
}

// ---

export type ContractStatus = 'Active' | 'Expired' | 'Terminated'
export type ContractType = 'Chính thức' | 'Thử việc' | 'Part-time'

export interface Contract {
  id: string
  contractNumber: string
  employeeId: string
  employeeName: string
  contractType: ContractType
  startDate: string
  endDate?: string
  baseSalary: number
  status: ContractStatus
  createdAt: string
  updatedAt?: string
}

export interface CreateContractDto {
  contractNumber: string
  employeeId: string
  contractType: ContractType
  startDate: string
  endDate?: string
  baseSalary: number
}

export interface UpdateContractDto {
  contractType: ContractType
  startDate: string
  endDate?: string
  baseSalary: number
  status: ContractStatus
}
