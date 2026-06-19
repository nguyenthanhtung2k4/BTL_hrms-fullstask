import { apiClient, extractData } from './apiClient'
import type { AuthResponse, LoginRequest, UserInfo } from '../types/auth.types'

export const authService = {
  async login(data: LoginRequest): Promise<AuthResponse> {
    const res = await apiClient.post<{ data: AuthResponse }>('/api/v1/hr/auth/login', data)
    return extractData(res)
  },

  async getMe(): Promise<UserInfo> {
    const res = await apiClient.get<{ data: UserInfo }>('/api/v1/hr/auth/me')
    return extractData(res)
  },
}
