import { apiClient, extractData } from './apiClient'
import type { UserAccount } from '../types/user.types'

const BASE = '/api/v1/hr/users'

export interface CreateUserDto {
  employeeId: string
  email: string
  password: string
  roles: string[]
}

export const userService = {
  getAll: () => apiClient.get<{ data: UserAccount[] }>(BASE).then(extractData),
  getById: (id: string) => apiClient.get<{ data: UserAccount }>(`${BASE}/${id}`).then(extractData),
  getByEmployeeId: (employeeId: string) => apiClient.get<{ data: UserAccount }>(`${BASE}/employee/${employeeId}`).then(extractData),
  create: (dto: CreateUserDto) => apiClient.post<{ data: UserAccount }>(BASE, dto).then(extractData),
  updateRoles: (id: string, roles: string[]) => apiClient.put<{ data: UserAccount }>(`${BASE}/${id}/roles`, { roles }).then(extractData),
  resetPassword: (id: string, newPassword: string) => apiClient.put(`${BASE}/${id}/password`, { newPassword }),
  changeStatus: (id: string, isActive: boolean) => apiClient.put(`${BASE}/${id}/status`, { isActive }),
}
