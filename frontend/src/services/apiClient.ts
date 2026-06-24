/**
 * apiClient.ts — Axios instance với:
 * 1. JWT Bearer auto-attach
 * 2. Refresh Token auto-retry khi nhận 401
 * 3. Logout tự động nếu refresh thất bại
 */
import axios, { type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5005'

// ─── Keys lưu trữ ────────────────────────────────────────────────────────────
const TOKEN_KEY         = 'hrms_token'
const REFRESH_TOKEN_KEY = 'hrms_refresh_token'

// ─── Axios instance chính ────────────────────────────────────────────────────
export const apiClient: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
})

// ─── Request interceptor: đính JWT token tự động ─────────────────────────────
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem(TOKEN_KEY)
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

// ─── Response interceptor: tự động refresh token khi nhận 401 ────────────────
let isRefreshing = false
let failedQueue: { resolve: (v: any) => void; reject: (e: any) => void }[] = []

function processQueue(error: any, token: string | null = null) {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error)
    else resolve(token)
  })
  failedQueue = []
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // Nếu 401 và chưa retry, thử refresh token
    if (error.response?.status === 401 && !originalRequest._retry) {
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)

      // Không có refresh token → đăng xuất ngay
      if (!refreshToken) {
        handleLogout()
        return Promise.reject(error)
      }

      if (isRefreshing) {
        // Queue các request đang chờ
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`
          return apiClient(originalRequest)
        }).catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const response = await axios.post(
          `${BASE_URL}/api/v1/hr/auth/refresh`,
          { refreshToken },
          { headers: { 'Content-Type': 'application/json' } }
        )

        const newAccessToken: string = response.data.data.accessToken
        const newRefreshToken: string = response.data.data.refreshToken

        localStorage.setItem(TOKEN_KEY, newAccessToken)
        localStorage.setItem(REFRESH_TOKEN_KEY, newRefreshToken)
        apiClient.defaults.headers.common.Authorization = `Bearer ${newAccessToken}`

        processQueue(null, newAccessToken)
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`
        return apiClient(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError, null)
        handleLogout()
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

function handleLogout() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  if (!window.location.pathname.includes('/login')) {
    window.location.href = '/login'
  }
}

// ─── Helper: trích xuất data từ ApiResponse<T> ───────────────────────────────
export function extractData<T>(response: { data: { data: T } }): T {
  return response.data.data
}

// ─── Helper: trích xuất chi tiết lỗi từ ApiResponse ──────────────────────────
export function extractError(err: any, fallback: string = 'Thao tác thất bại'): string {
  const apiData = err?.response?.data
  if (apiData?.errors && Array.isArray(apiData.errors) && apiData.errors.length > 0) {
    return apiData.errors.join(', ')
  }
  return apiData?.message || fallback
}

// ─── Export helpers ───────────────────────────────────────────────────────────
export const TOKEN_STORAGE_KEY         = TOKEN_KEY
export const REFRESH_TOKEN_STORAGE_KEY = REFRESH_TOKEN_KEY

export default apiClient
