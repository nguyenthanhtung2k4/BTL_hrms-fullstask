import { apiClient, extractData } from './apiClient'
import type { Contract, CreateContractDto, UpdateContractDto } from '../types/hr.types'

const BASE = '/api/v1/hr/contracts'

export const contractService = {
  getAll: () => apiClient.get<{ data: Contract[] }>(BASE).then(extractData),
  getById: (id: string) => apiClient.get<{ data: Contract }>(`${BASE}/${id}`).then(extractData),
  create: (dto: CreateContractDto) => apiClient.post<{ data: Contract }>(BASE, dto).then(extractData),
  update: (id: string, dto: UpdateContractDto) => apiClient.put<{ data: Contract }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
}
