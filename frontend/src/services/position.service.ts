import { apiClient, extractData } from './apiClient'
import type { Position, CreatePositionDto, UpdatePositionDto } from '../types/hr.types'

const BASE = '/api/v1/hr/positions'

export const positionService = {
  getAll: () => apiClient.get<{ data: Position[] }>(BASE).then(extractData),
  getById: (id: string) => apiClient.get<{ data: Position }>(`${BASE}/${id}`).then(extractData),
  create: (dto: CreatePositionDto) => apiClient.post<{ data: Position }>(BASE, dto).then(extractData),
  update: (id: string, dto: UpdatePositionDto) => apiClient.put<{ data: Position }>(`${BASE}/${id}`, dto).then(extractData),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
}
