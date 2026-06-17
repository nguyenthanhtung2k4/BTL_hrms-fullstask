# 💰 Module 4: Payroll & Report — Tính lương & Báo cáo

> **Service:** Payroll (`/api/v1/payroll/`)  
> **Files:** `src/modules/payroll/`  
> **Roles:** Admin, PayrollStaff (quản lý) | Employee (xem phiếu lương của mình) | HR/Manager (xem báo cáo)

---

## Checklist thực hiện

### Kỳ lương (Payroll Periods)
- [ ] `payrollPeriod.service.ts`
- [ ] `PeriodListView.vue`
- [ ] `PeriodFormModal.vue`
- [ ] `PeriodDetailView.vue` — Xem phiếu lương trong kỳ + tính lương + đóng kỳ
- [ ] `usePayrollPeriods.ts`

### Quy tắc lương (Payroll Rules)
- [ ] `payrollRule.service.ts`
- [ ] `RuleListView.vue`
- [ ] `RuleFormModal.vue`
- [ ] `usePayrollRules.ts`

### Phụ cấp (Allowances)
- [ ] `allowance.service.ts`
- [ ] `AllowanceListView.vue`
- [ ] `AllowanceFormModal.vue`
- [ ] `useAllowances.ts`

### Khấu trừ (Deductions)
- [ ] `deduction.service.ts`
- [ ] `DeductionListView.vue`
- [ ] `DeductionFormModal.vue`
- [ ] `useDeductions.ts`

### Phiếu lương (Payslips)
- [ ] `payslip.service.ts`
- [ ] `PayslipListView.vue` — Admin/PayrollStaff xem tất cả
- [ ] `MyPayslipView.vue` — Employee xem của mình
- [ ] `PayslipDetailView.vue` — Chi tiết phiếu lương
- [ ] `usePayslips.ts`

### Báo cáo (Reports)
- [ ] `report.service.ts`
- [ ] `ReportView.vue` — Báo cáo tổng hợp + biểu đồ
- [ ] `useReports.ts`

---

## 1. Payroll Periods — Kỳ lương

### Màn hình `PeriodListView.vue`
```
[PageHeader: "Kỳ lương"] [Button "+ Tạo kỳ lương" — Admin/PayrollStaff]
[Filter: Năm | Trạng thái]
[Table]
  Cột: Tên kỳ | Từ ngày | Đến ngày | Trạng thái | Số phiếu lương | Tổng lương | Hành động
  Trạng thái badge:
    Draft      → blue   "Nháp"
    Calculated → yellow "Đã tính"
    Closed     → green  "Đã đóng" (khóa — không sửa được)
  Hành động:
    Draft:      [Xem] [Sửa] [Tính lương →] [Xóa]
    Calculated: [Xem] [Tính lại] [Đóng kỳ 🔒]
    Closed:     [Xem] — chỉ xem, không sửa
```

### Modal `PeriodFormModal.vue`
```
Fields:
  Tên kỳ lương*  → Input (vd: "Lương tháng 06/2026")
  Từ ngày*       → DatePicker
  Đến ngày*      → DatePicker
  Quy tắc lương* → Select (load từ API)
```

### Màn hình `PeriodDetailView.vue`

```
[Back] [Tên kỳ lương] [Badge trạng thái]
Thông tin: Từ ngày | Đến ngày | Quy tắc áp dụng

[Action Bar — chỉ khi Draft/Calculated]
  [Button "Tính lương cho kỳ này" — PayrollStaff/Admin]
    → Loading spinner
    → Sau khi xong: refresh bảng phiếu lương
  [Button "Đóng kỳ lương 🔒" — chỉ khi Calculated]
    → Confirm dialog: "Sau khi đóng không thể chỉnh sửa. Tiếp tục?"
    → Chuyển trạng thái → Closed

[Table: Phiếu lương trong kỳ này]
  Cột: Nhân viên | Phòng ban | Lương cơ bản | Phụ cấp | Khấu trừ | Lương Gross | Lương Net | Thao tác
  [Xem chi tiết]
```

---

## 2. Payroll Rules — Quy tắc lương

### Màn hình `RuleListView.vue`
```
[PageHeader: "Quy tắc tính lương"] [Button "+ Thêm quy tắc" — Admin/PayrollStaff]
[Table]
  Cột: Mã | Tên | Giờ/ngày công chuẩn | Tính OT | Hệ số OT | Phép có lương tính vào công | Kích hoạt | Hành động
```

### Modal `RuleFormModal.vue`
```
Fields:
  Mã*                      → Input
  Tên*                     → Input
  Số giờ công chuẩn/ngày*  → Number (vd: 8)
  Tính phép có lương vào công → Toggle (default: true)
  Hệ số lương OT*          → Number (vd: 1.5)
  Kích hoạt                → Toggle
```

---

## 3. Allowances & Deductions

### Màn hình `AllowanceListView.vue`
```
[PageHeader: "Phụ cấp nhân viên"]
[Filter: Kỳ lương | Nhân viên | Loại phụ cấp]
[Button "+ Thêm phụ cấp" — Admin/PayrollStaff]
[Table]
  Cột: Nhân viên | Kỳ lương | Loại phụ cấp | Số tiền | Mô tả | Hành động
```

### Modal `AllowanceFormModal.vue`
```
Fields:
  Kỳ lương*     → Select (chỉ load kỳ chưa Closed)
  Nhân viên*    → Select
  Loại phụ cấp* → Select (load /allowances/types):
                   Phụ cấp đi lại / Ăn trưa / Điện thoại...
  Số tiền*      → InputNumber (VNĐ)
  Mô tả         → Input
```

*(Deductions tương tự Allowances — các loại: BHXH, BHYT, BHTN, Thuế TNCN, Khấu trừ đi muộn)*

**Lưu ý UX quan trọng:**
> Nếu kỳ lương đã `Closed` → disable nút thêm/sửa/xóa phụ cấp/khấu trừ  
> Hiện tooltip: "Kỳ lương đã đóng, không thể chỉnh sửa"

---

## 4. Payslips — Phiếu lương

### Màn hình `PayslipListView.vue` (Admin/PayrollStaff)
```
[PageHeader: "Phiếu lương"]
[Filter: Kỳ lương | Nhân viên | Phòng ban]
[Table]
  Cột: Nhân viên | Kỳ lương | Ngày công | Lương CB | +Phụ cấp | -Khấu trừ | Gross | Net | Xem
```

### Màn hình `MyPayslipView.vue` (Employee)
```
[PageHeader: "Phiếu lương của tôi"]
[Filter: Tháng | Năm]
[Cards — mỗi card là 1 kỳ lương]
  Card: Tháng XX/YYYY
    Lương Net: [số tiền to, xanh]
    Ngày công: X / 22 ngày
    [Xem chi tiết →]
```

### Màn hình `PayslipDetailView.vue`
```
[Tiêu đề: PHIẾU LƯƠNG — [Tháng/Năm]]
[Thông tin nhân viên: Tên | Mã | Phòng ban | Chức vụ]

[Bảng chi tiết]
  A. THU NHẬP
     Lương cơ bản:          X,XXX,XXX đ
     Số ngày công thực tế:  XX/22 ngày
     Lương theo công:       X,XXX,XXX đ
     + Phụ cấp đi lại:       XXX,XXX đ
     + Phụ cấp ăn trưa:      XXX,XXX đ
     TỔNG THU NHẬP (Gross): X,XXX,XXX đ

  B. KHẤU TRỪ
     - BHXH (8%):            XXX,XXX đ
     - BHYT (1.5%):           XX,XXX đ
     - BHTN (1%):             XX,XXX đ
     - Thuế TNCN:            XXX,XXX đ
     TỔNG KHẤU TRỪ:         XXX,XXX đ

  C. LƯƠNG THỰC LĨNH (Net): X,XXX,XXX đ  [to, đậm, xanh]

[Button "In phiếu lương" / "Xuất PDF" — optional]
```

---

## 5. Reports — Báo cáo

### Màn hình `ReportView.vue`

**Layout 3 section:**

#### Section 1: Tổng quan (cards thống kê)
```
[Card] Tổng nhân viên: 50
[Card] Nhân viên active: 45
[Card] Tổng lương tháng này: 1,250,000,000 đ
[Card] Đơn nghỉ phép đang chờ: 3
```

#### Section 2: Báo cáo theo phòng ban
```
[Filter: Kỳ lương | Phòng ban]
[Table]
  Cột: Phòng ban | Số NV | Tổng ngày công | Tổng phụ cấp | Tổng khấu trừ | Tổng Gross | Tổng Net
```

#### Section 3: Biểu đồ (nếu có thư viện chart)
```
- Bar chart: So sánh lương theo phòng ban
- Pie chart: Phân bổ nhân sự theo phòng ban
```

**Filter panel:**
```
- Dropdown Kỳ lương (load từ API — chỉ Closed)
- Dropdown Phòng ban (load từ API)
- Button [Xem báo cáo]
```

---

## 6. API Services

```typescript
// src/services/payrollPeriod.service.ts
export const payrollPeriodService = {
  getAll: (params?) => apiClient.get('/api/v1/payroll/payroll-periods', { params }),
  getById: (id) => apiClient.get(`/api/v1/payroll/payroll-periods/${id}`),
  create: (data) => apiClient.post('/api/v1/payroll/payroll-periods', data),
  update: (id, data) => apiClient.put(`/api/v1/payroll/payroll-periods/${id}`, data),
  delete: (id) => apiClient.delete(`/api/v1/payroll/payroll-periods/${id}`),
  calculate: (id) => apiClient.post(`/api/v1/payroll/payroll-periods/${id}/calculate`),
  close: (id) => apiClient.post(`/api/v1/payroll/payroll-periods/${id}/close`),
}

// src/services/payslip.service.ts
export const payslipService = {
  getAll: (params?) => apiClient.get('/api/v1/payroll/payslips', { params }),
  getById: (id) => apiClient.get(`/api/v1/payroll/payslips/${id}`),
  getMyPayslips: () => apiClient.get('/api/v1/payroll/payslips/me'),
}

// src/services/report.service.ts
export const reportService = {
  getSummary: (params?) => apiClient.get('/api/v1/payroll/reports/summary', { params }),
}
```

---

## 7. TypeScript Types

```typescript
// src/types/payroll.types.ts

export type PeriodStatus = 'Draft' | 'Calculated' | 'Closed'

export interface PayrollPeriod {
  id: string
  name: string
  fromDate: string
  toDate: string
  status: PeriodStatus
  payrollRuleId: string
  payrollRuleName: string
}

export interface PayrollRule {
  id: string
  code: string
  name: string
  workDayHours: number
  paidLeaveCountsAsWork: boolean
  overtimeRate: number
  isActive: boolean
}

export interface EmployeeAllowance {
  id: string
  payrollPeriodId: string
  employeeId: string
  employeeName: string
  allowanceTypeId: string
  allowanceTypeName: string
  amount: number
  notes?: string
}

export interface Payslip {
  id: string
  payrollPeriodId: string
  periodName: string
  employeeId: string
  employeeName: string
  departmentName: string
  baseSalary: number
  actualWorkDays: number
  standardWorkDays: number
  salaryByWork: number
  totalAllowances: number
  totalDeductions: number
  grossSalary: number
  netSalary: number
  items: PayslipItem[]
}

export interface PayslipItem {
  id: string
  type: 'Earning' | 'Deduction'
  name: string
  amount: number
}

export interface PayrollSummaryReport {
  departmentId: string
  departmentName: string
  employeeCount: number
  totalWorkDays: number
  totalAllowances: number
  totalDeductions: number
  totalGross: number
  totalNet: number
}
```
