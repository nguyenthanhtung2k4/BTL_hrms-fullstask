# 🎨 FRONTEND MASTER PLAN — HRMS Microservices
> **Đề tài 03: Hệ thống Quản lý Nhân sự & Chấm công**  
> Stack: Vue 3 + TypeScript + TailwindCSS v4 + Pinia + Axios  
> Mục tiêu: Xây dựng giao diện đầy đủ kết nối với 3 backend services qua API Gateway

---
addc
## 📋 Tổng quan tài liệu

| Tài liệu | Mô tả |
|---|---|
| `10_FRONTEND_MASTER_PLAN.md` | File này — tổng quan toàn bộ |
| `11_FRONTEND_FOLDER_STRUCTURE.md` | Cấu trúc thư mục chi tiết |
| `12_FRONTEND_AUTH_MODULE.md` | Module Auth — Login, Me, Route Guard |
| `13_FRONTEND_HR_MODULE.md` | Module HR — Departments, Positions, Employees, Contracts |
| `14_FRONTEND_ATTENDANCE_MODULE.md` | Module Attendance — Shifts, WorkSchedules, CheckIn/Out, Leaves, Timesheets |
| `15_FRONTEND_PAYROLL_MODULE.md` | Module Payroll — Periods, Rules, Allowances, Payslips, Reports |
| `16_FRONTEND_DASHBOARD_MODULE.md` | Dashboard — Statistics, Charts |

---

## 🔐 Phân quyền theo Role (từ đề tài)

### Role matrix — Menu hiển thị theo role

| Menu / Chức năng | Admin | HR | Manager | Employee | PayrollStaff |
|---|---|---|---|---|---|
| **Dashboard** | ✅ Toàn bộ | ✅ Toàn bộ | ✅ Phạm vi | ✅ Cá nhân | ✅ Lương |
| **HR › Phòng ban** | ✅ CRUD | ✅ CRUD | 👁 Xem | ❌ | ❌ |
| **HR › Chức vụ** | ✅ CRUD | ✅ CRUD | 👁 Xem | ❌ | ❌ |
| **HR › Nhân viên** | ✅ CRUD | ✅ CRUD | 👁 Phạm vi | 👁 Cá nhân | 👁 Cần thiết |
| **HR › Hợp đồng** | ✅ CRUD | ✅ CRUD | ❌ | ❌ | ❌ |
| **Attendance › Ca làm** | ✅ CRUD | ✅ CRUD | 👁 Xem | 👁 Xem | ❌ |
| **Attendance › Lịch làm** | ✅ CRUD | ✅ CRUD | ✅ Phạm vi | 👁 Của mình | ❌ |
| **Attendance › Chấm công** | ✅ | ✅ | ✅ | ✅ Check-in/out | ❌ |
| **Attendance › Nghỉ phép** | ✅ Duyệt | ✅ Duyệt | ✅ Duyệt | ✅ Tạo/xem | ❌ |
| **Attendance › Bảng công** | ✅ | ✅ | ✅ Phòng mình | 👁 Của mình | ❌ |
| **Payroll › Kỳ lương** | ✅ CRUD | ❌ | ❌ | ❌ | ✅ CRUD |
| **Payroll › Quy tắc lương** | ✅ CRUD | ❌ | ❌ | ❌ | ✅ CRUD |
| **Payroll › Phụ cấp/Khấu trừ** | ✅ | ❌ | ❌ | ❌ | ✅ |
| **Payroll › Phiếu lương** | ✅ Tất cả | ❌ | ❌ | 👁 Của mình | ✅ Tất cả |
| **Payroll › Báo cáo** | ✅ | ✅ | ✅ Phòng mình | ❌ | ✅ |

### Sidebar menu theo Role

```
Admin / HR:
├── Dashboard
├── Nhân sự (HR)
│   ├── Phòng ban
│   ├── Chức vụ
│   ├── Nhân viên
│   └── Hợp đồng
├── Chấm công
│   ├── Ca làm việc
│   ├── Lịch làm việc
│   ├── Chấm công
│   ├── Nghỉ phép
│   └── Bảng công
└── Lương & Báo cáo
    ├── Kỳ lương
    ├── Quy tắc lương
    ├── Phụ cấp / Khấu trừ
    ├── Phiếu lương
    └── Báo cáo

Manager:
├── Dashboard (phạm vi)
├── Nhân sự (chỉ xem nhân viên phòng mình)
├── Chấm công
│   ├── Ca làm việc (xem)
│   ├── Lịch làm việc (phạm vi)
│   ├── Chấm công (xem)
│   ├── Nghỉ phép (Duyệt/Từ chối)
│   └── Bảng công (phòng mình)
└── Báo cáo (phòng mình)

Employee:
├── Dashboard (cá nhân)
├── Hồ sơ cá nhân
├── Chấm công
│   ├── Check-in / Check-out
│   ├── Nghỉ phép (Tạo đơn / xem đơn của mình)
│   └── Bảng công cá nhân
└── Phiếu lương của tôi

PayrollStaff:
├── Dashboard (lương)
├── Kỳ lương
├── Quy tắc lương
├── Phụ cấp / Khấu trừ
├── Phiếu lương (tất cả)
└── Báo cáo lương
```

---

## 📁 Cấu trúc thư mục Frontend

```
frontend/src/
├── assets/
│   └── logo.svg
│
├── components/                    # Shared UI components
│   ├── ui/
│   │   ├── AppButton.vue          # Button với variants (primary/secondary/danger/ghost)
│   │   ├── AppInput.vue           # Input + label + error state
│   │   ├── AppSelect.vue          # Select dropdown
│   │   ├── AppModal.vue           # Modal/Dialog
│   │   ├── AppTable.vue           # Table + loading + empty + pagination
│   │   ├── AppBadge.vue           # Status badges (Active/Inactive/Pending...)
│   │   ├── AppPagination.vue      # Pagination controls
│   │   ├── AppToast.vue           # Toast notifications
│   │   └── AppConfirm.vue         # Confirm dialog
│   └── layout/
│       ├── PageHeader.vue         # Tiêu đề trang + breadcrumb
│       └── StatCard.vue           # Card thống kê Dashboard
│
├── layouts/
│   ├── MainLayout.vue             # Sidebar + Topbar layout (đã có, cần nâng cấp)
│   └── AuthLayout.vue             # Layout trang login
│
├── modules/
│   ├── auth/
│   │   ├── LoginView.vue          # Trang đăng nhập thật (gọi API)
│   │   └── composables/
│   │       └── useAuth.ts
│   │
│   ├── dashboard/
│   │   └── DashboardView.vue      # Dashboard thống kê theo role
│   │
│   ├── hr/
│   │   ├── departments/
│   │   │   ├── DepartmentListView.vue
│   │   │   ├── DepartmentFormModal.vue
│   │   │   └── composables/useDepartments.ts
│   │   ├── positions/
│   │   │   ├── PositionListView.vue
│   │   │   ├── PositionFormModal.vue
│   │   │   └── composables/usePositions.ts
│   │   ├── employees/
│   │   │   ├── EmployeeListView.vue
│   │   │   ├── EmployeeDetailView.vue
│   │   │   ├── EmployeeFormModal.vue
│   │   │   ├── EmployeeStatusModal.vue
│   │   │   └── composables/useEmployees.ts
│   │   └── contracts/
│   │       ├── ContractListView.vue
│   │       ├── ContractFormModal.vue
│   │       └── composables/useContracts.ts
│   │
│   ├── attendance/
│   │   ├── shifts/
│   │   │   ├── ShiftListView.vue
│   │   │   ├── ShiftFormModal.vue
│   │   │   └── composables/useShifts.ts
│   │   ├── work-schedules/
│   │   │   ├── WorkScheduleListView.vue
│   │   │   ├── WorkScheduleFormModal.vue
│   │   │   └── composables/useWorkSchedules.ts
│   │   ├── attendance/
│   │   │   ├── AttendanceView.vue         # Check-in/out + lịch sử
│   │   │   ├── AttendanceListView.vue     # Admin xem toàn bộ
│   │   │   └── composables/useAttendance.ts
│   │   ├── leaves/
│   │   │   ├── LeaveListView.vue
│   │   │   ├── LeaveFormModal.vue
│   │   │   ├── LeaveApproveModal.vue
│   │   │   └── composables/useLeaves.ts
│   │   └── timesheets/
│   │       ├── TimesheetView.vue
│   │       └── composables/useTimesheets.ts
│   │
│   └── payroll/
│       ├── periods/
│       │   ├── PeriodListView.vue
│       │   ├── PeriodFormModal.vue
│       │   ├── PeriodDetailView.vue       # Xem payslips trong kỳ
│       │   └── composables/usePayrollPeriods.ts
│       ├── rules/
│       │   ├── RuleListView.vue
│       │   ├── RuleFormModal.vue
│       │   └── composables/usePayrollRules.ts
│       ├── allowances/
│       │   ├── AllowanceListView.vue
│       │   ├── AllowanceFormModal.vue
│       │   └── composables/useAllowances.ts
│       ├── deductions/
│       │   ├── DeductionListView.vue
│       │   ├── DeductionFormModal.vue
│       │   └── composables/useDeductions.ts
│       ├── payslips/
│       │   ├── PayslipListView.vue        # Admin/PayrollStaff xem tất cả
│       │   ├── MyPayslipView.vue          # Employee xem của mình
│       │   ├── PayslipDetailView.vue
│       │   └── composables/usePayslips.ts
│       └── reports/
│           ├── ReportView.vue
│           └── composables/useReports.ts
│
├── router/
│   └── index.ts                   # Routes với meta: requiresAuth, roles
│
├── services/                      # API service layer
│   ├── apiClient.ts               # Axios instance + JWT interceptor
│   ├── auth.service.ts
│   ├── department.service.ts
│   ├── position.service.ts
│   ├── employee.service.ts
│   ├── contract.service.ts
│   ├── shift.service.ts
│   ├── workSchedule.service.ts
│   ├── attendance.service.ts
│   ├── leave.service.ts
│   ├── timesheet.service.ts
│   ├── payrollPeriod.service.ts
│   ├── payrollRule.service.ts
│   ├── allowance.service.ts
│   ├── deduction.service.ts
│   ├── payslip.service.ts
│   └── report.service.ts
│
├── stores/                        # Pinia stores
│   ├── auth.ts                    # User session, JWT token, roles
│   ├── toast.ts                   # Global toast notifications
│   └── ui.ts                      # Loading states, sidebar collapsed
│
└── types/                         # TypeScript types/interfaces
    ├── auth.types.ts
    ├── hr.types.ts
    ├── attendance.types.ts
    └── payroll.types.ts
```

---

## 🚦 Trạng thái & Màu sắc chuẩn (Badge)

```
Employee Status:
  Active    → green  badge
  Inactive  → gray   badge
  OnLeave   → yellow badge
  Resigned  → red    badge

Contract Status:
  Active    → green
  Expired   → orange
  Terminated → red

Leave Status:
  Pending   → yellow/amber
  Approved  → green
  Rejected  → red
  Cancelled → gray

Payroll Period Status:
  Draft     → blue
  Calculated → yellow
  Closed    → green (khóa, không sửa được)

Attendance:
  CheckedIn  → green
  CheckedOut → blue
  Late       → orange (đến muộn)
  Absent     → red
```

---

## 📅 Thứ tự thực hiện (Task Order)

> ✅ **TẤT CẢ BƯỚC ĐÃ HOÀN THÀNH** — Login test thành công tại `http://localhost:5173`

```
[x] Bước 1: Shared Foundation ✅
    [x] apiClient.ts — JWT interceptor thật
    [x] auth.ts store — gọi API thật (+ fix normalize roles string→array)
    [x] toast.ts store
    [x] AppButton, AppInput, AppTable, AppModal, AppBadge, AppToast, AppConfirm
    [x] Router với role guard

[x] Bước 2: Auth Module ✅
    [x] LoginView thật (gọi POST /api/v1/hr/auth/login)
    [x] Lưu JWT vào localStorage + store
    [x] GET /auth/me khi khởi động app
    [x] Route guard theo role

[x] Bước 3: HR Module ✅
    [x] Departments — CRUD
    [x] Positions — CRUD
    [x] Employees — CRUD + đổi trạng thái
    [x] Contracts — CRUD

[x] Bước 4: Attendance Module ✅
    [x] Shifts — CRUD
    [x] WorkSchedules — CRUD
    [x] Attendance — Check-in/out + lịch sử
    [x] Leave Requests — Tạo/Duyệt/Từ chối/Hủy
    [x] Timesheets — Xem bảng công tháng + Tính bảng công

[x] Bước 5: Payroll Module ✅
    [x] Payroll Periods — CRUD + calculate + close
    [x] Payroll Rules — CRUD
    [x] Allowances & Deductions — CRUD
    [x] Payslips — Xem + GET /me + detail
    [x] Reports — Dashboard summary theo phòng ban

[x] Bước 6: Dashboard ✅
    [x] Thống kê tổng số nhân viên, kỳ lương, nghỉ phép pending
    [x] Stat cards theo role (Admin/Manager/PayrollStaff/Employee)
    [x] Dept breakdown chart + Pending leaves list
    [x] Quick actions cho Employee
```

---

## 🔗 API Endpoints Map

### Auth
| Method | Endpoint | Dùng ở đâu |
|---|---|---|
| POST | `/api/v1/hr/auth/login` | LoginView |
| GET | `/api/v1/hr/auth/me` | App khởi động, refresh |

### HR Core (`/api/v1/hr/`)
| Method | Endpoint | Dùng ở đâu |
|---|---|---|
| GET/POST | `/departments` | DepartmentListView |
| GET/PUT/DELETE | `/departments/{id}` | DepartmentFormModal |
| GET/POST | `/positions` | PositionListView |
| GET/PUT/DELETE | `/positions/{id}` | PositionFormModal |
| GET/POST | `/employees` | EmployeeListView |
| GET/PUT/DELETE | `/employees/{id}` | EmployeeDetailView |
| PUT | `/employees/{id}/status` | EmployeeStatusModal |
| GET/POST | `/contracts` | ContractListView |
| GET/PUT/DELETE | `/contracts/{id}` | ContractFormModal |

### Attendance (`/api/v1/attendance/`)
| Method | Endpoint | Dùng ở đâu |
|---|---|---|
| GET/POST | `/shifts` | ShiftListView |
| GET/PUT/DELETE | `/shifts/{id}` | ShiftFormModal |
| GET/POST | `/work-schedules` | WorkScheduleListView |
| POST | `/attendance/check-in` | AttendanceView |
| POST | `/attendance/check-out` | AttendanceView |
| GET | `/attendance` | AttendanceListView |
| GET | `/attendance/my-today` | AttendanceView (Employee) |
| GET/POST | `/leaves` | LeaveListView |
| POST | `/leaves/{id}/approve` | LeaveApproveModal |
| POST | `/leaves/{id}/reject` | LeaveApproveModal |
| GET | `/timesheets` | TimesheetView |
| POST | `/timesheets/calculate` | TimesheetView (admin) |

### Payroll (`/api/v1/payroll/`)
| Method | Endpoint | Dùng ở đâu |
|---|---|---|
| GET/POST | `/payroll-periods` | PeriodListView |
| POST | `/payroll-periods/{id}/calculate` | PeriodDetailView |
| POST | `/payroll-periods/{id}/close` | PeriodDetailView |
| GET/POST | `/payroll-rules` | RuleListView |
| GET/POST | `/allowances` | AllowanceListView |
| GET/POST | `/deductions` | DeductionListView |
| GET | `/payslips` | PayslipListView |
| GET | `/payslips/me` | MyPayslipView |
| GET | `/reports/summary` | ReportView |

---

## ⚙️ Chi tiết kỹ thuật quan trọng

### JWT Interceptor (apiClient.ts)
- Tự động đính kèm `Authorization: Bearer {token}` vào mọi request
- Nếu 401 → tự động logout và redirect về `/login`

### Role Guard (router/index.ts)
```
route.meta.roles = ['Admin', 'HR']
→ Nếu user không có role đó → redirect 403 hoặc về dashboard
```

### Form Validation
- Tất cả form đều validate ở client trước khi gửi
- Hiển thị lỗi từ API response (ProblemDetails) dưới field tương ứng

### Loading States
- Skeleton loading cho tables
- Spinner cho form submit buttons
- Toast success/error sau mỗi action

---

> Xem chi tiết từng module trong các file `11_` → `16_` trong thư mục `docs/`
