import { defineStore } from 'pinia'

export type UserRole = 'Admin' | 'HR' | 'Manager' | 'Employee' | 'PayrollStaff'

export type UserSession = {
  employeeId: string
  fullName: string
  email: string
  roles: UserRole[]
  selectedRole: UserRole
}

const demoUser: UserSession = {
  employeeId: 'emp-admin',
  fullName: 'Admin Demo',
  email: 'admin@hrms.local',
  roles: ['Admin', 'HR', 'Manager', 'Employee', 'PayrollStaff'],
  selectedRole: 'Admin',
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as UserSession | null,
    token: '',
  }),
  getters: {
    isAuthenticated: (state) => Boolean(state.user),
    displayName: (state) => state.user?.fullName ?? '',
    roles: (state) => state.user?.roles ?? [],
    activeRole: (state) => state.user?.selectedRole ?? 'Employee',
  },
  actions: {
    loginDemo() {
      this.user = { ...demoUser }
      this.token = 'demo-token'
    },
    loginAs(role: UserRole) {
      this.token = 'demo-token'
      this.user = {
        employeeId: '',
        fullName: '',
        email: '',
        roles: ['Admin', 'HR', 'Manager', 'Employee', 'PayrollStaff'],
        selectedRole: role
      }
      this.switchRole(role)
    },
    switchRole(role: UserRole) {
      if (this.user) {
        this.user.selectedRole = role
        if (role === 'Employee') {
          this.user.fullName = 'Nguyễn Văn A (Nhân viên)'
          this.user.email = 'employee@hrms.local'
          this.user.employeeId = 'emp-001'
        } else if (role === 'Manager') {
          this.user.fullName = 'Trần Thị B (Trưởng phòng)'
          this.user.email = 'manager@hrms.local'
          this.user.employeeId = 'emp-002'
        } else if (role === 'HR') {
          this.user.fullName = 'Lê Văn C (Nhân sự)'
          this.user.email = 'hr@hrms.local'
          this.user.employeeId = 'emp-003'
        } else if (role === 'PayrollStaff') {
          this.user.fullName = 'Phạm Minh D (Kế toán)'
          this.user.email = 'payroll@hrms.local'
          this.user.employeeId = 'emp-004'
        } else {
          this.user.fullName = 'Admin Demo'
          this.user.email = 'admin@hrms.local'
          this.user.employeeId = 'emp-admin'
        }
      }
    },
    logout() {
      this.user = null
      this.token = ''
    },
  },
})


