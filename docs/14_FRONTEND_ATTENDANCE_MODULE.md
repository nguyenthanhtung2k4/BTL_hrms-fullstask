# 📅 Module 3: Attendance — Chấm công & Nghỉ phép

> **Service:** Attendance (`/api/v1/attendance/`)  
> **Files:** `src/modules/attendance/`

---

## Checklist thực hiện

### Ca làm việc (Shifts)
- [x] `shift.service.ts`
- [x] `ShiftListView.vue`
- [x] `ShiftFormModal.vue`
- [x] `useShifts.ts`

### Lịch làm việc (Work Schedules)
- [x] `workSchedule.service.ts`
- [x] `WorkScheduleListView.vue`
- [x] `WorkScheduleFormModal.vue`
- [x] `useWorkSchedules.ts`

### Chấm công (Attendance Records)
- [x] `attendance.service.ts`
- [x] `MyAttendanceView.vue` — Employee: check-in/out cá nhân
- [x] `AttendanceListView.vue` — Admin/HR/Manager: xem toàn bộ
- [x] `useAttendance.ts`

### Nghỉ phép (Leave Requests)
- [x] `leave.service.ts`
- [x] `LeaveListView.vue`
- [x] `LeaveFormModal.vue` — Employee tạo đơn
- [x] `LeaveApproveModal.vue` — Manager/HR duyệt/từ chối
- [x] `useLeaves.ts`

### Bảng công (Timesheets)
- [x] `timesheet.service.ts`
- [x] `TimesheetView.vue`
- [x] `useTimesheets.ts`

---

## 1. Shifts — Ca làm việc

### Màn hình `ShiftListView.vue`
```
[PageHeader: "Ca làm việc"] [Button "+ Thêm ca" — Admin/HR]
[Table]
  Cột: Tên ca | Giờ bắt đầu | Giờ kết thúc | Giờ nghỉ | Tổng giờ làm | Trạng thái | Hành động
  Hành động: [Sửa] [Xóa] — Admin/HR
```

### Modal `ShiftFormModal.vue`
```
Fields:
  Tên ca*        → Input
  Mã ca          → Input
  Giờ bắt đầu*  → TimePicker (HH:mm)
  Giờ kết thúc* → TimePicker (HH:mm)
  Giờ nghỉ      → Number (phút)
  Kích hoạt     → Toggle
```

---

## 2. Work Schedules — Lịch làm việc

### Màn hình `WorkScheduleListView.vue`
```
[PageHeader: "Lịch làm việc"]
[Filter: Nhân viên | Tháng | Ca làm]
[Table]
  Cột: Nhân viên | Ca làm | Ngày bắt đầu | Ngày kết thúc | Trạng thái | Hành động
[Button "+ Phân lịch" — Admin/HR/Manager]
```

### Modal `WorkScheduleFormModal.vue`
```
Fields:
  Nhân viên*     → Select (load từ API)
  Ca làm việc*   → Select (load shifts)
  Ngày bắt đầu* → DatePicker
  Ngày kết thúc* → DatePicker
  Loại lặp       → Select: Không / Hàng tuần / Hàng tháng
```

---

## 3. Attendance — Chấm công

### Màn hình `MyAttendanceView.vue` (dành cho Employee)
```
[Card: Hôm nay - [Ngày/Thứ]]
  Ca làm hôm nay: [Tên ca] - [Giờ bắt đầu] → [Giờ kết thúc]
  
  Trạng thái hiện tại:
    Chưa check-in → Badge xám "Chưa vào"
    Đã check-in   → Badge xanh "Đang làm việc" + giờ check-in
    Đã check-out  → Badge xanh đậm "Đã hoàn thành" + số giờ làm

  [Button "CHECK-IN" — to lớn, xanh lá] (ẩn nếu đã check-in)
  [Button "CHECK-OUT" — to lớn, xanh dương] (ẩn nếu chưa check-in hoặc đã checkout)

[Card: Lịch sử chấm công tháng này]
  Table: Ngày | Ca | Giờ vào | Giờ ra | Phút làm | Ghi chú
```

**Logic nút bấm:**
```
Nếu chưa check-in hôm nay     → Hiện [CHECK-IN], ẩn [CHECK-OUT]
Nếu đã check-in chưa checkout → Ẩn [CHECK-IN], Hiện [CHECK-OUT]
Nếu đã cả 2                   → Ẩn cả 2, hiện "Hoàn thành hôm nay ✓"
Nếu không có ca làm hôm nay   → "Hôm nay không có lịch làm việc"
```

### Màn hình `AttendanceListView.vue` (Admin/HR/Manager)
```
[PageHeader: "Bảng chấm công"]
[Filter: Nhân viên | Phòng ban | Tháng | Năm]
[Button "Tính bảng công" — Admin/HR] → gọi POST /timesheets/calculate
[Table]
  Cột: Nhân viên | Phòng ban | Ngày | Ca | Giờ vào | Giờ ra | Phút làm | Ghi chú
[Export CSV button — Admin/HR]
```

---

## 4. Leave Requests — Nghỉ phép

### Màn hình `LeaveListView.vue`

**Hiển thị theo role:**
```
Employee:
  Chỉ thấy đơn của mình
  Có nút "+ Tạo đơn nghỉ phép"
  Có nút [Hủy] với đơn Pending của mình

Manager/HR/Admin:
  Thấy toàn bộ đơn (Manager: phòng mình)
  Filter: Nhân viên | Phòng ban | Trạng thái | Từ ngày → Đến ngày
  Nút [Duyệt] [Từ chối] với đơn Pending
```

**Table:**
```
Cột: Nhân viên | Loại nghỉ | Từ ngày | Đến ngày | Số ngày | Lý do | Trạng thái | Hành động
Trạng thái badge:
  Pending  → amber/yellow
  Approved → green
  Rejected → red
  Cancelled → gray
```

### Modal `LeaveFormModal.vue` (Employee tạo đơn)
```
Fields:
  Loại nghỉ*    → Select: load từ /leaves/types
                   Vd: Nghỉ phép năm / Nghỉ ốm / Nghỉ không lương...
  Từ ngày*      → DatePicker
  Đến ngày*     → DatePicker (>= Từ ngày)
  Lý do*        → Textarea
  
Hiển thị: Tổng số ngày xin nghỉ (tự tính)
[Hủy] [Gửi đơn]
```

### Modal `LeaveApproveModal.vue` (Manager/HR duyệt)
```
Thông tin đơn:
  Nhân viên: [Tên]
  Loại nghỉ: [Loại]
  Thời gian: [Từ ngày] → [Đến ngày] ([X] ngày)
  Lý do: [...]

[Button "Duyệt" — green]  [Button "Từ chối" — red]  [Hủy]
Confirm dialog trước khi duyệt/từ chối
```

---

## 5. Timesheets — Bảng công

### Màn hình `TimesheetView.vue`
```
[PageHeader: "Bảng công tháng"]
[Filter: Tháng | Năm | Nhân viên — Admin/HR/Manager]

Bảng tổng hợp:
  Cột: Nhân viên | Ngày làm | Ngày nghỉ phép có lương | Ngày vắng | Tổng giờ OT | Trạng thái

[Button "Tính bảng công tháng [MM/YYYY]" — Admin/HR]
  → Gọi POST /timesheets/calculate
  → Loading spinner + disable nút khi đang tính
  → Toast success khi xong
```

---

## 6. API Services

```typescript
// src/services/attendance.service.ts
export const attendanceService = {
  checkIn: (employeeId: string) =>
    apiClient.post('/api/v1/attendance/attendance/check-in', { employeeId }),
  
  checkOut: (employeeId: string) =>
    apiClient.post('/api/v1/attendance/attendance/check-out', { employeeId }),
  
  getMyToday: (employeeId: string) =>
    apiClient.get(`/api/v1/attendance/attendance/my-today?employeeId=${employeeId}`),
  
  getAll: (params) =>
    apiClient.get('/api/v1/attendance/attendance', { params }),
}

// src/services/leave.service.ts
export const leaveService = {
  getTypes: () => apiClient.get('/api/v1/attendance/leaves/types'),
  getAll: (params) => apiClient.get('/api/v1/attendance/leaves', { params }),
  getMyLeaves: (employeeId) => apiClient.get(`/api/v1/attendance/leaves?employeeId=${employeeId}`),
  create: (employeeId, data) => apiClient.post(`/api/v1/attendance/leaves?employeeId=${employeeId}`, data),
  approve: (id, approvedById) => apiClient.post(`/api/v1/attendance/leaves/${id}/approve`, { approvedByEmployeeId: approvedById }),
  reject: (id, approvedById) => apiClient.post(`/api/v1/attendance/leaves/${id}/reject`, { approvedByEmployeeId: approvedById }),
  cancel: (id, employeeId) => apiClient.post(`/api/v1/attendance/leaves/${id}/cancel`, { employeeId }),
}
```

---

## 7. TypeScript Types

```typescript
// src/types/attendance.types.ts

export interface Shift {
  id: string
  code: string
  name: string
  startTime: string   // "08:00"
  endTime: string     // "17:00"
  breakMinutes: number
  isActive: boolean
}

export interface LeaveRequest {
  id: string
  employeeId: string
  employeeName: string
  leaveTypeId: string
  leaveTypeName: string
  isPaid: boolean
  fromDate: string
  toDate: string
  totalDays: number
  reason: string
  status: 'Pending' | 'Approved' | 'Rejected' | 'Cancelled'
  approvedByEmployeeId?: string
  approvedByName?: string
  approvedAt?: string
}

export interface AttendanceRecord {
  id: string
  employeeId: string
  employeeName: string
  workScheduleId: string
  shiftName: string
  checkInTime?: string
  checkOutTime?: string
  totalMinutesWorked: number
  notes?: string
  date: string
}

export interface Timesheet {
  id: string
  employeeId: string
  employeeName: string
  month: number
  year: number
  totalWorkDays: number
  totalPaidLeaveDays: number
  totalUnpaidLeaveDays: number
  totalAbsentDays: number
  totalOvertimeMinutes: number
}
```

