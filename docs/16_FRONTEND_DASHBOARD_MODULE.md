


# 📊 Module 5: Dashboard — Thống kê tổng quan

> **Data từ:** Tất cả 3 services  
> **File:** `src/modules/dashboard/DashboardView.vue`

---

## Checklist thực hiện

- [x] Dashboard cards thống kê (gọi API thật)
- [x] Bảng service map (giữ lại, update với real-time health)
- [x] Quick actions theo role
- [x] Pending items (đơn nghỉ phép chờ duyệt, kỳ lương chưa tính...)

---

## Dashboard theo Role

### Admin / HR — Xem toàn bộ
```
[Row 1 — Stats Cards]
  [Tổng nhân viên]     → GET /api/v1/hr/employees (count)
  [Nhân viên Active]   → filter status=Active
  [Đơn nghỉ phép chờ] → GET /api/v1/attendance/leaves?status=Pending (count)
  [Kỳ lương đang mở]  → GET /api/v1/payroll/payroll-periods?status=Draft (count)

[Row 2 — Quick Actions]
  [+ Thêm nhân viên]   → link /hr/employees
  [+ Tạo ca làm việc]  → link /attendance/shifts
  [+ Tạo kỳ lương]     → link /payroll/periods

[Row 3 — Recent Activity]
  Bảng "Đơn nghỉ phép chờ duyệt" (top 5)
  Cột: Nhân viên | Loại nghỉ | Ngày | Số ngày | [Duyệt nhanh]
```

### Manager — Phạm vi phòng mình
```
[Row 1 — Stats]
  [Nhân viên phòng tôi]   → filter theo departmentId
  [Đơn nghỉ phép chờ]    → filter phòng mình + status=Pending
  [Bảng công tháng này]   → link timesheets

[Row 2 — Pending Actions]
  Bảng "Đơn nghỉ phép cần duyệt"
  [Duyệt] [Từ chối] ngay trên dashboard
```

### Employee — Cá nhân
```
[Card: Chào [Tên nhân viên]]
[Card: Trạng thái hôm nay]
  Ca làm: [Tên ca] [Giờ]
  Check-in: [Giờ vào] hoặc [Chưa check-in]
  [Button: CHECK-IN / CHECK-OUT — to lớn]

[Card: Phiếu lương gần nhất]
  Tháng XX/YYYY — Net: X,XXX,XXX đ
  [Xem chi tiết →]

[Card: Đơn nghỉ phép của tôi]
  [X] đơn đang chờ duyệt
  [+ Tạo đơn mới]
```

### PayrollStaff — Lương
```
[Row 1 — Stats]
  [Kỳ lương mở]
  [Kỳ lương đã tính — cần đóng]
  [Tổng phiếu lương tháng này]
  [Tổng quỹ lương]

[Row 2 — Actions]
  [Tính lương kỳ hiện tại]
  [Xem báo cáo tổng hợp]
```

---

## Service Map (giữ lại, cải thiện)
```
Bảng Service Map:
  Service | Database | Port | Health Status
  HR Core | HRMS_HrCoreDb | 5001 | 🟢 Online
  Attendance | HRMS_AttendanceDb | 5002 | 🟢 Online
  Payroll | HRMS_PayrollReportDb | 5003 | 🟢 Online

Health check: gọi /health của từng service mỗi 30s
```

---

## Notes kỹ thuật
- Dùng `Promise.allSettled()` để gọi nhiều API song song
- Nếu một API lỗi → chỉ ảnh hưởng card đó, card khác vẫn hiện
- Loading skeleton cho từng card riêng biệt

