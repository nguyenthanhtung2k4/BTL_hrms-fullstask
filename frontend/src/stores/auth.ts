import { defineStore } from 'pinia'
import { authService } from '../services/auth.service'
import { TOKEN_STORAGE_KEY } from '../services/apiClient'
import type { UserInfo, ChangePasswordPayload } from '../types/auth.types'

// Backend có thể trả roles dưới dạng string hoặc string[] → normalize
function normalizeRoles(roles: string | string[] | undefined): string[] {
  if (!roles) return []
  if (Array.isArray(roles)) return roles
  return roles.split(',').map((r) => r.trim()).filter(Boolean)
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as UserInfo | null,
    token: localStorage.getItem(TOKEN_STORAGE_KEY) ?? '',
    loading: false,
    initialized: false,
  }),

  getters: {
    isAuthenticated: (state) => !!state.user,
    displayName: (state) => state.user?.fullName ?? '',
    roles: (state) => state.user?.roles ?? [],
    employeeId: (state) => state.user?.employeeId ?? null,
    userId: (state) => state.user?.id ?? null,

    // Role helpers — giữ nguyên quyền theo đúng spec dự án
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
    // ─── Login ────────────────────────────────────────────────────────────────
    async login(email: string, password: string) {
      this.loading = true
      try {
        const result = await authService.login({ email, password })
        this.token = result.accessToken
        this.user = { ...result.user, roles: normalizeRoles((result.user as any).roles) }
        authService.saveTokens(result.accessToken, result.refreshToken)
      } finally {
        this.loading = false
      }
    },

    // ─── Fetch me (gọi khi app khởi động) ────────────────────────────────────
    async fetchMe() {
      if (!this.token) {
        this.initialized = true
        return
      }
      try {
        const me = await authService.getMe()
        this.user = { ...me, roles: normalizeRoles((me as any).roles) }
      } catch {
        // Token không hợp lệ → thử refresh trước khi logout
        const refreshed = await this.tryRefresh()
        if (!refreshed) this.logout()
      } finally {
        this.initialized = true
      }
    },

    // ─── Thử refresh token ────────────────────────────────────────────────────
    async tryRefresh(): Promise<boolean> {
      const refreshToken = authService.getRefreshToken()
      if (!refreshToken) return false
      try {
        const result = await authService.refresh(refreshToken)
        this.token = result.accessToken
        this.user = { ...result.user, roles: normalizeRoles((result.user as any).roles) }
        authService.saveTokens(result.accessToken, result.refreshToken)
        return true
      } catch {
        return false
      }
    },

    // ─── Đổi mật khẩu ────────────────────────────────────────────────────────
    async changePassword(payload: ChangePasswordPayload): Promise<void> {
      await authService.changePassword(payload)
      // Thu hồi refresh token cũ → buộc đăng nhập lại
      const rt = authService.getRefreshToken()
      if (rt) {
        try { await authService.revoke(rt) } catch { /* ignore */ }
      }
      this.logout()
    },

    // ─── Logout ───────────────────────────────────────────────────────────────
    async logout() {
      // Thu hồi refresh token ở backend trước
      const rt = authService.getRefreshToken()
      if (rt) {
        try { await authService.revoke(rt) } catch { /* ignore if failed */ }
      }
      this.user = null
      this.token = ''
      this.initialized = false
      authService.clearTokens()
    },
  },
})
