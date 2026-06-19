import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

export const apiClient = axios.create({
  baseURL: BASE_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// ─── Request interceptor: tự động đính JWT token ─────────────────────────
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('hrms_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

// ─── Response interceptor: tự động logout nếu 401 ───────────────────────
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('hrms_token')
      // Redirect về login nếu không phải đang ở trang login
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)

// ─── Helper để trích xuất data từ ApiResponse<T> ─────────────────────────
export function extractData<T>(response: { data: { data: T } }): T {
  return response.data.data
}

export default apiClient
