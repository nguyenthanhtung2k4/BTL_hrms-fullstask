// Auth Types
export interface LoginRequest {
  email: string
  password: string
}

export interface UserInfo {
  id: string
  email: string
  fullName: string
  employeeId: string | null
  roles: string[]
}

export interface AuthResponse {
  accessToken: string
  user: UserInfo
}

export type UserRole = 'Admin' | 'HR' | 'Manager' | 'Employee' | 'PayrollStaff'
