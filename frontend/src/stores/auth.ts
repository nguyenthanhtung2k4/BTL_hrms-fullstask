import { defineStore } from 'pinia'

export type UserRole = 'Admin' | 'HR' | 'Manager' | 'Employee' | 'PayrollStaff'

type UserSession = {
  fullName: string
  email: string
  roles: UserRole[]
}

const demoUser: UserSession = {
  fullName: 'Admin Demo',
  email: 'admin@hrms.local',
  roles: ['Admin', 'HR', 'PayrollStaff'],
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
  },
  actions: {
    loginDemo() {
      this.user = demoUser
      this.token = 'demo-token'
    },
    logout() {
      this.user = null
      this.token = ''
    },
  },
})

