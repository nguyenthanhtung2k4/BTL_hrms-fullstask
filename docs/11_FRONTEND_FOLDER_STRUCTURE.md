# 📁 Cấu trúc thư mục Frontend — Chi tiết

> File này mô tả từng file cần tạo, vai trò của nó và thứ tự ưu tiên.

---

## Thứ tự tạo file (theo dependency)

### ✅ Phase 1 — Foundation (HOÀN THÀNH)
```
- [x] src/types/auth.types.ts
- [x] src/types/hr.types.ts
- [x] src/types/attendance.types.ts
- [x] src/types/payroll.types.ts
- [x] src/services/apiClient.ts         ← JWT interceptor
- [x] src/stores/auth.ts                ← Pinia auth store thật
- [x] src/stores/toast.ts               ← Global toast
- [x] src/stores/ui.ts                  ← Loading/sidebar state
- [x] src/services/auth.service.ts
```

### ✅ Phase 2 — Shared UI Components (HOÀN THÀNH)
```
- [x] src/components/ui/AppButton.vue
- [x] src/components/ui/AppInput.vue
- [x] src/components/ui/AppSelect.vue
- [x] src/components/ui/AppModal.vue
- [x] src/components/ui/AppTable.vue
- [x] src/components/ui/AppBadge.vue
- [x] src/components/ui/AppPagination.vue
- [x] src/components/ui/AppToast.vue
- [x] src/components/ui/AppConfirm.vue
- [x] src/components/layout/PageHeader.vue
- [x] src/components/layout/StatCard.vue
```

### ✅ Phase 3 — Auth + Layout + Router (HOÀN THÀNH)
```
- [x] src/modules/auth/LoginView.vue    ← Login thật
- [x] src/layouts/MainLayout.vue        ← Upgrade: role-based sidebar
- [x] src/router/index.ts               ← Full routes + role guard
- [x] src/App.vue                       ← Gọi auth.fetchMe() khi mount
```

### ✅ Phase 4 — HR Module (HOÀN THÀNH)
```
- [x] src/services/department.service.ts
- [x] src/services/position.service.ts
- [x] src/services/employee.service.ts
- [x] src/services/contract.service.ts
- [x] src/modules/hr/departments/DepartmentListView.vue
- [x] src/modules/hr/departments/DepartmentFormModal.vue   ← inline trong ListView
- [x] src/modules/hr/positions/PositionListView.vue
- [x] src/modules/hr/positions/PositionFormModal.vue       ← inline trong ListView
- [x] src/modules/hr/employees/EmployeeListView.vue
- [x] src/modules/hr/employees/EmployeeDetailView.vue
- [x] src/modules/hr/employees/EmployeeFormModal.vue       ← inline trong ListView
- [x] src/modules/hr/employees/EmployeeStatusModal.vue     ← inline trong ListView
- [x] src/modules/hr/contracts/ContractListView.vue
- [x] src/modules/hr/contracts/ContractFormModal.vue       ← inline trong ListView
```

### ✅ Phase 5 — Attendance Module (HOÀN THÀNH)
```
- [x] src/services/shift.service.ts
- [x] src/services/workSchedule.service.ts
- [x] src/services/attendance.service.ts
- [x] src/services/leave.service.ts
- [x] src/services/timesheet.service.ts
- [x] src/modules/attendance/shifts/ShiftListView.vue
- [x] src/modules/attendance/shifts/ShiftFormModal.vue     ← inline trong ListView
- [x] src/modules/attendance/work-schedules/WorkScheduleListView.vue
- [x] src/modules/attendance/work-schedules/WorkScheduleFormModal.vue ← inline
- [x] src/modules/attendance/attendance/MyAttendanceView.vue
- [x] src/modules/attendance/attendance/AttendanceListView.vue
- [x] src/modules/attendance/leaves/LeaveListView.vue
- [x] src/modules/attendance/leaves/LeaveFormModal.vue     ← inline trong ListView
- [x] src/modules/attendance/leaves/LeaveApproveModal.vue  ← inline (AppConfirm)
- [x] src/modules/attendance/timesheets/TimesheetView.vue
```

### ✅ Phase 6 — Payroll Module (HOÀN THÀNH)
```
- [x] src/services/payrollPeriod.service.ts
- [x] src/services/payrollRule.service.ts
- [x] src/services/allowance.service.ts
- [x] src/services/deduction.service.ts
- [x] src/services/payslip.service.ts
- [x] src/services/report.service.ts
- [x] src/modules/payroll/periods/PeriodListView.vue
- [x] src/modules/payroll/periods/PeriodFormModal.vue      ← inline trong ListView
- [x] src/modules/payroll/periods/PeriodDetailView.vue
- [x] src/modules/payroll/rules/RuleListView.vue
- [x] src/modules/payroll/rules/RuleFormModal.vue          ← inline trong ListView
- [x] src/modules/payroll/allowances/AllowanceListView.vue
- [x] src/modules/payroll/allowances/AllowanceFormModal.vue ← inline
- [x] src/modules/payroll/deductions/DeductionListView.vue
- [x] src/modules/payroll/deductions/DeductionFormModal.vue ← inline
- [x] src/modules/payroll/payslips/PayslipListView.vue
- [x] src/modules/payroll/payslips/MyPayslipView.vue
- [x] src/modules/payroll/payslips/PayslipDetailView.vue
- [x] src/modules/payroll/reports/ReportView.vue
```

### ✅ Phase 7 — Dashboard nâng cấp (HOÀN THÀNH)
```
- [x] src/modules/dashboard/DashboardView.vue  ← Nâng cấp với API thật, stat cards, role-based content
```

---

> 🎉 **TẤT CẢ 7 PHASES ĐÃ HOÀN THÀNH** — Frontend kết nối đầy đủ với 3 backend services qua API Gateway

## Naming conventions

| Loại | Convention | Ví dụ |
|---|---|---|
| Views/Pages | `PascalCase + View` | `EmployeeListView.vue` |
| Modals | `PascalCase + Modal` | `EmployeeFormModal.vue` |
| Composables | `use + PascalCase` | `useEmployees.ts` |
| Services | `camelCase + .service` | `employee.service.ts` |
| Stores | `camelCase` | `auth.ts` |
| Types | `camelCase + .types` | `hr.types.ts` |
