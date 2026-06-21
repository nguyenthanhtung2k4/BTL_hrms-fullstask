export interface UserAccount {
  id: string
  employeeId: string | null
  email: string
  isActive: boolean
  roles: string[]
  lastLoginAt: string | null
}
