import { defineStore } from 'pinia'
import { authService } from '../services/auth.service'
import type { UserInfo } from '../types/auth.types'

// Backend có thể trả roles dưới dạng string hoặc string[]
// Normalize về string[] để đồng nhất
function normalizeRoles(roles: string | string[] | undefined): string[] {
  if (!roles) return []
  if (Array.isArray(roles)) return roles
  // "Admin" → ["Admin"] | "Admin,HR" → ["Admin","HR"]
  return roles.split(',').map((r) => r.trim()).filter(Boolean)
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as UserInfo | null,
    token: localStorage.getItem('hrms_token') ?? '',
    loading: false,
    initialized: false,
  }),

  getters: {
    isAuthenticated: (state) => !!state.user,
    displayName: (state) => state.user?.fullName ?? '',
    roles: (state) => state.user?.roles ?? [],
    employeeId: (state) => state.user?.employeeId ?? null,
    userId: (state) => state.user?.id ?? null,

    // Role helpers
    isAdmin: (state) => state.user?.roles.includes('Admin') ?? false,
    isHR: (state) =>
      state.user?.roles.some((r) => ['Admin', 'HR'].includes(r)) ?? false,
    isManager: (state) =>
      state.user?.roles.some((r) => ['Admin', 'HR', 'Manager'].includes(r)) ?? false,
    isEmployee: (state) => state.user?.roles.includes('Employee') ?? false,
    isPayrollStaff: (state) =>
      state.user?.roles.some((r) => ['Admin', 'PayrollStaff'].includes(r)) ?? false,

    hasRole: (state) => (role: string) => state.user?.roles.includes(role) ?? false,
    hasAnyRole: (state) => (roles: string[]) =>
      state.user?.roles.some((r) => roles.includes(r)) ?? false,
  },

  actions: {
    async login(email: string, password: string) {
      this.loading = true
      try {
        const result = await authService.login({ email, password })
        this.token = result.accessToken
        // Normalize roles: backend có thể trả string hoặc string[]
        this.user = {
          ...result.user,
          roles: normalizeRoles((result.user as any).roles),
        }
        localStorage.setItem('hrms_token', result.accessToken)
      } finally {
        this.loading = false
      }
    },

    async fetchMe() {
      if (!this.token) {
        this.initialized = true
        return
      }
      try {
        const me = await authService.getMe()
        this.user = { ...me, roles: normalizeRoles((me as any).roles) }
      } catch {
        // Token hết hạn hoặc không hợp lệ → logout
        this.logout()
      } finally {
        this.initialized = true
      }
    },

    logout() {
      this.user = null
      this.token = ''
      this.initialized = false
      localStorage.removeItem('hrms_token')
    },
  },
})
