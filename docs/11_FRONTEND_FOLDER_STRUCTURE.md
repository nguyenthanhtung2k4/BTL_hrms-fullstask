# 📁 Cấu trúc thư mục Frontend — Chi tiết

> File này mô tả từng file cần tạo, vai trò của nó và thứ tự ưu tiên.

---

## Thứ tự tạo file (theo dependency)

### 🔴 Phase 1 — Foundation (Tạo trước tiên)
```
- [ ] src/types/auth.types.ts
- [ ] src/types/hr.types.ts
- [ ] src/types/attendance.types.ts
- [ ] src/types/payroll.types.ts
- [ ] src/services/apiClient.ts         ← JWT interceptor
- [ ] src/stores/auth.ts                ← Pinia auth store thật
- [ ] src/stores/toast.ts               ← Global toast
- [ ] src/stores/ui.ts                  ← Loading/sidebar state
- [ ] src/services/auth.service.ts
```

### 🔴 Phase 2 — Shared UI Components
```
- [ ] src/components/ui/AppButton.vue
- [ ] src/components/ui/AppInput.vue
- [ ] src/components/ui/AppSelect.vue
- [ ] src/components/ui/AppModal.vue
- [ ] src/components/ui/AppTable.vue
- [ ] src/components/ui/AppBadge.vue
- [ ] src/components/ui/AppPagination.vue
- [ ] src/components/ui/AppToast.vue
- [ ] src/components/ui/AppConfirm.vue
- [ ] src/components/layout/PageHeader.vue
- [ ] src/components/layout/StatCard.vue
```

### 🔴 Phase 3 — Auth + Layout + Router
```
- [ ] src/modules/auth/LoginView.vue    ← Login thật
- [ ] src/layouts/MainLayout.vue        ← Upgrade: role-based sidebar
- [ ] src/router/index.ts               ← Full routes + role guard
- [ ] src/App.vue                       ← Gọi auth.fetchMe() khi mount
```

### 🟡 Phase 4 — HR Module
```
- [ ] src/services/department.service.ts
- [ ] src/services/position.service.ts
- [ ] src/services/employee.service.ts
- [ ] src/services/contract.service.ts
- [ ] src/modules/hr/departments/DepartmentListView.vue
- [ ] src/modules/hr/departments/DepartmentFormModal.vue
- [ ] src/modules/hr/positions/PositionListView.vue
- [ ] src/modules/hr/positions/PositionFormModal.vue
- [ ] src/modules/hr/employees/EmployeeListView.vue
- [ ] src/modules/hr/employees/EmployeeDetailView.vue
- [ ] src/modules/hr/employees/EmployeeFormModal.vue
- [ ] src/modules/hr/employees/EmployeeStatusModal.vue
- [ ] src/modules/hr/contracts/ContractListView.vue
- [ ] src/modules/hr/contracts/ContractFormModal.vue
```

### 🟡 Phase 5 — Attendance Module
```
- [ ] src/services/shift.service.ts
- [ ] src/services/workSchedule.service.ts
- [ ] src/services/attendance.service.ts
- [ ] src/services/leave.service.ts
- [ ] src/services/timesheet.service.ts
- [ ] src/modules/attendance/shifts/ShiftListView.vue
- [ ] src/modules/attendance/shifts/ShiftFormModal.vue
- [ ] src/modules/attendance/work-schedules/WorkScheduleListView.vue
- [ ] src/modules/attendance/work-schedules/WorkScheduleFormModal.vue
- [ ] src/modules/attendance/attendance/MyAttendanceView.vue
- [ ] src/modules/attendance/attendance/AttendanceListView.vue
- [ ] src/modules/attendance/leaves/LeaveListView.vue
- [ ] src/modules/attendance/leaves/LeaveFormModal.vue
- [ ] src/modules/attendance/leaves/LeaveApproveModal.vue
- [ ] src/modules/attendance/timesheets/TimesheetView.vue
```

### 🟡 Phase 6 — Payroll Module
```
- [ ] src/services/payrollPeriod.service.ts
- [ ] src/services/payrollRule.service.ts
- [ ] src/services/allowance.service.ts
- [ ] src/services/deduction.service.ts
- [ ] src/services/payslip.service.ts
- [ ] src/services/report.service.ts
- [ ] src/modules/payroll/periods/PeriodListView.vue
- [ ] src/modules/payroll/periods/PeriodFormModal.vue
- [ ] src/modules/payroll/periods/PeriodDetailView.vue
- [ ] src/modules/payroll/rules/RuleListView.vue
- [ ] src/modules/payroll/rules/RuleFormModal.vue
- [ ] src/modules/payroll/allowances/AllowanceListView.vue
- [ ] src/modules/payroll/allowances/AllowanceFormModal.vue
- [ ] src/modules/payroll/deductions/DeductionListView.vue
- [ ] src/modules/payroll/deductions/DeductionFormModal.vue
- [ ] src/modules/payroll/payslips/PayslipListView.vue
- [ ] src/modules/payroll/payslips/MyPayslipView.vue
- [ ] src/modules/payroll/payslips/PayslipDetailView.vue
- [ ] src/modules/payroll/reports/ReportView.vue
```

### 🟢 Phase 7 — Dashboard nâng cấp
```
- [ ] src/modules/dashboard/DashboardView.vue  ← Nâng cấp với API thật
```

---

## Naming conventions

| Loại | Convention | Ví dụ |
|---|---|---|
| Views/Pages | `PascalCase + View` | `EmployeeListView.vue` |
| Modals | `PascalCase + Modal` | `EmployeeFormModal.vue` |
| Composables | `use + PascalCase` | `useEmployees.ts` |
| Services | `camelCase + .service` | `employee.service.ts` |
| Stores | `camelCase` | `auth.ts` |
| Types | `camelCase + .types` | `hr.types.ts` |
