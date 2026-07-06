import { apiClient, extractData, TOKEN_STORAGE_KEY, REFRESH_TOKEN_STORAGE_KEY } from './apiClient'
import type { AuthResponse, LoginRequest, UserInfo, ChangePasswordPayload } from '../types/auth.types'

export const authService = {
  async login(data: LoginRequest): Promise<AuthResponse> {
    const res = await apiClient.post<{ data: AuthResponse }>('/api/v1/hr/auth/login', data)
    return extractData(res)
  },

  async getMe(): Promise<UserInfo> {
    const res = await apiClient.get<{ data: UserInfo }>('/api/v1/hr/auth/me')
    return extractData(res)
  },

  async refresh(refreshToken: string): Promise<AuthResponse> {
    const res = await apiClient.post<{ data: AuthResponse }>('/api/v1/hr/auth/refresh', { refreshToken })
    return extractData(res)
  },

  async revoke(refreshToken: string): Promise<void> {
    await apiClient.post('/api/v1/hr/auth/revoke', { refreshToken })
  },

  async changePassword(payload: ChangePasswordPayload): Promise<void> {
    await apiClient.post('/api/v1/hr/auth/change-password', payload)
  },

  // Helpers truy cập localStorage
  saveTokens(accessToken: string, refreshToken: string) {
    localStorage.setItem(TOKEN_STORAGE_KEY, accessToken)
    localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, refreshToken)
  },

  clearTokens() {
    localStorage.removeItem(TOKEN_STORAGE_KEY)
    localStorage.removeItem(REFRESH_TOKEN_STORAGE_KEY)
  },

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY)
  },
}
