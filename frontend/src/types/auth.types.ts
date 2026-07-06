// ─── Auth Types ───────────────────────────────────────────────────────────────

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
  avatarUrl?: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  refreshTokenExpiry: string
  user: UserInfo
}

export interface ChangePasswordPayload {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

export type UserRole = 'Admin' | 'HR' | 'Manager' | 'Employee' | 'PayrollStaff'
