# 🔐 Module 1: Auth — Xác thực & Phân quyền

> **Service:** HR Core (`/api/v1/hr/auth/`)  
> **Files:** `src/modules/auth/`, `src/stores/auth.ts`, `src/services/auth.service.ts`

---

## Checklist thực hiện

- [x] **auth.service.ts** — API calls thật
- [x] **auth.ts store** — Pinia store với JWT thật
- [x] **LoginView.vue** — Form login gọi API
- [x] **apiClient.ts** — JWT interceptor
- [x] **router/index.ts** — Route guard theo role
- [x] **App.vue** — Tự động gọi `/auth/me` khi load app

---

## 1. `src/services/auth.service.ts`

```typescript
import { apiClient } from './apiClient'

export interface LoginRequest {
  email: string
  password: string
}

export interface UserInfo {
  id: string
  email: string
  fullName: string
  employeeId: string | null
  roles: string[]
}

export interface AuthResponse {
  accessToken: string
  user: UserInfo
}

export const authService = {
  async login(data: LoginRequest) {
    const res = await apiClient.post<{ data: AuthResponse }>('/api/v1/hr/auth/login', data)
    return res.data.data
  },

  async getMe() {
    const res = await apiClient.get<{ data: UserInfo }>('/api/v1/hr/auth/me')
    return res.data.data
  }
}
```

---

## 2. `src/services/apiClient.ts` — JWT Interceptor

```typescript
import axios from 'axios'
import { useAuthStore } from '../stores/auth'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  timeout: 10000,
})

// Tự động đính kèm JWT token vào mọi request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('hrms_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Tự động logout nếu 401
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('hrms_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)
```

---

## 3. `src/stores/auth.ts` — Pinia Store thật

```typescript
import { defineStore } from 'pinia'
import { authService, type UserInfo } from '../services/auth.service'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as UserInfo | null,
    token: localStorage.getItem('hrms_token') ?? '',
    loading: false,
  }),
  getters: {
    isAuthenticated: (state) => !!state.user,
    displayName: (state) => state.user?.fullName ?? '',
    roles: (state) => state.user?.roles ?? [],
    hasRole: (state) => (role: string) => state.user?.roles.includes(role) ?? false,
    isAdmin: (state) => state.user?.roles.includes('Admin') ?? false,
    isHR: (state) => state.user?.roles.some(r => ['Admin', 'HR'].includes(r)) ?? false,
    isManager: (state) => state.user?.roles.includes('Manager') ?? false,
    isEmployee: (state) => state.user?.roles.includes('Employee') ?? false,
    isPayrollStaff: (state) => state.user?.roles.some(r => ['Admin', 'PayrollStaff'].includes(r)) ?? false,
  },
  actions: {
    async login(email: string, password: string) {
      this.loading = true
      try {
        const result = await authService.login({ email, password })
        this.token = result.accessToken
        this.user = result.user
        localStorage.setItem('hrms_token', result.accessToken)
      } finally {
        this.loading = false
      }
    },

    async fetchMe() {
      if (!this.token) return
      try {
        this.user = await authService.getMe()
      } catch {
        this.logout()
      }
    },

    logout() {
      this.user = null
      this.token = ''
      localStorage.removeItem('hrms_token')
    }
  }
})
```

---

## 4. `src/modules/auth/LoginView.vue`

**UI cần có:**
- Logo HRMS + tên hệ thống
- Input Email (type=email, required)
- Input Password (type=password, required)
- Button "Đăng nhập" (loading state khi đang gọi API)
- Hiển thị lỗi nếu sai email/password
- Sau login thành công → redirect theo role:
  - Admin/HR → `/`
  - Employee → `/attendance/me`
  - PayrollStaff → `/payroll`

**States:**
```
Default    → Form trống, button enabled
Loading    → Button disabled + spinner, inputs disabled
Error      → Border đỏ + error message bên dưới
Success    → Redirect (không cần hiện gì)
```

---

## 5. `src/router/index.ts` — Route Guard hoàn chỉnh

```typescript
// Meta cho mỗi route
{ meta: { requiresAuth: true, roles: ['Admin', 'HR'] } }

// Guard
router.beforeEach(async (to) => {
  const auth = useAuthStore()

  // Nếu có token nhưng chưa load user → gọi /me
  if (auth.token && !auth.user) {
    await auth.fetchMe()
  }

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login' }
  }

  const requiredRoles = to.meta.roles as string[] | undefined
  if (requiredRoles && !requiredRoles.some(r => auth.roles.includes(r))) {
    return { name: 'forbidden' } // hoặc về dashboard
  }
})
```

---

## 6. Route map đầy đủ

```
/login                          → LoginView [public]
/                               → DashboardView [requiresAuth]
/hr/departments                 → DepartmentListView [Admin, HR]
/hr/positions                   → PositionListView [Admin, HR]
/hr/employees                   → EmployeeListView [Admin, HR, Manager]
/hr/employees/:id               → EmployeeDetailView [Admin, HR, Manager]
/hr/contracts                   → ContractListView [Admin, HR]
/profile                        → ProfileView [All]
/attendance/shifts              → ShiftListView [Admin, HR]
/attendance/work-schedules      → WorkScheduleListView [Admin, HR, Manager]
/attendance/records             → AttendanceListView [Admin, HR, Manager]
/attendance/checkin             → MyAttendanceView [Employee, All]
/attendance/leaves              → LeaveListView [All]
/attendance/timesheets          → TimesheetView [Admin, HR, Manager, Employee]
/payroll/periods                → PeriodListView [Admin, PayrollStaff]
/payroll/rules                  → RuleListView [Admin, PayrollStaff]
/payroll/allowances             → AllowanceListView [Admin, PayrollStaff]
/payroll/deductions             → DeductionListView [Admin, PayrollStaff]
/payroll/payslips               → PayslipListView [Admin, PayrollStaff]
/payroll/my-payslip             → MyPayslipView [Employee]
/payroll/reports                → ReportView [Admin, HR, PayrollStaff, Manager]
/forbidden                      → ForbiddenView
```

