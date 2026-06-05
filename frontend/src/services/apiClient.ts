import axios from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  timeout: 10000,
})

export type ServiceInfo = {
  serviceName: string
  version: string
  database: string
  ownedModules: string[]
  publishedEvents: string[]
  consumedEvents: string[]
}

