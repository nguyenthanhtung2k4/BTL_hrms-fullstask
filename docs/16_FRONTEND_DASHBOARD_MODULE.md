# 📊 Module 5: Dashboard — Thống kê tổng quan (ENHANCED PLAN)

> **Data từ:** Tất cả 3 services
> **File:** `src/modules/dashboard/DashboardView.vue`
> **Cập nhật:** 2026-06-21 — Phân tích chuyên sâu & kế hoạch nâng cấp toàn diện

---

## ✅ Checklist thực hiện

### Đã có (hiện tại)
- [x] Dashboard cards thống kê cơ bản (Admin/Manager)
- [x] Greeting theo giờ
- [x] Bảng đơn nghỉ phép chờ duyệt (top 5)
- [x] Nhân viên theo phòng ban (bar mini)
- [x] Quick actions cho Employee
- [x] PayrollStaff: stat card kỳ lương đang mở

### Cần bổ sung (plan mới)
- [x] **Admin/HR:** Thêm stat cards: Hợp đồng sắp hết hạn, NV mới tháng này, Tỷ lệ nghỉ phép
- [ ] **Admin/HR:** Bảng "Nhân viên mới tháng này" (top 5)
- [x] **Admin/HR:** Bảng "Hợp đồng sắp hết hạn" (trong 30 ngày)
- [x] **Manager:** Stat card tỷ lệ đi làm hôm nay (phòng mình)
- [x] **Manager:** Bảng chấm công hôm nay của phòng
- [x] **Manager:** Quick approve từ Dashboard (không cần vào trang leaves)
- [x] **PayrollStaff:** Stat cards đầy đủ: Tổng quỹ lương, Số phiếu lương, Kỳ cần đóng
- [x] **PayrollStaff:** Bảng kỳ lương đang mở cần xử lý
- [x] **Employee:** Card trạng thái chấm công hôm nay (check-in/out)
- [x] **Employee:** Card phiếu lương gần nhất (net salary)
- [x] **Employee:** Card đơn nghỉ phép của tôi (pending/approved)
- [x] **Employee:** Lịch sử chấm công 7 ngày gần nhất
- [x] **Tất cả:** Service Health Map (3 services + ping status)
- [ ] **Tất cả:** Loading skeleton riêng từng widget

---

## 🔍 Phân tích vấn đề hiện tại

### ❌ Những gì đang thiếu

```
ROLE       | HIỆN TẠI                        | VẤN ĐỀ
-----------|----------------------------------|------------------------------------------
Admin/HR   | 4 cards + 2 bảng đơn giản       | Thiếu: hợp đồng hết hạn, NV mới,
           |                                  | tỷ lệ nghỉ phép, tổng quỹ lương tháng
-----------|----------------------------------|------------------------------------------
Manager    | Dùng chung với Admin             | Không phân tách scope phòng mình
           |                                  | Không có bảng chấm công hôm nay
-----------|----------------------------------|------------------------------------------
PayrollStaff| CHỈ 1 STAT CARD duy nhất!!     | Thiếu hoàn toàn: quỹ lương, phiếu lương,
           |                                  | kỳ cần đóng, top dept chi lương
-----------|----------------------------------|------------------------------------------
Employee   | Chỉ 2 button + text chào         | Không có data gì! Thiếu: checkin status,
           |                                  | phiếu lương gần nhất, đơn nghỉ của mình,
           |                                  | lịch sử 7 ngày
```

---

## 🎯 Plan nâng cấp chi tiết theo Role

---

### 👑 Admin / HR Dashboard

#### Row 1 — KPI Cards (6 cards, 3 cột desktop)
```
┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐
│ 👥 Tổng nhân viên   │  │ ✅ Đang làm việc     │  │ 🏢 Phòng ban        │
│ [số]                │  │ [số] / [tổng]        │  │ [số] phòng          │
│ subtitle: trong hệ  │  │ subtitle: Active      │  │ subtitle: hoạt động │
└─────────────────────┘  └─────────────────────┘  └─────────────────────┘
┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐
│ ⏳ Chờ duyệt nghỉ   │  │ 📋 HĐ sắp hết hạn   │  │ 🆕 NV mới tháng này │
│ [số] đơn pending    │  │ [số] trong 30 ngày   │  │ [số] nhân viên      │
│ color: amber        │  │ color: red/orange    │  │ color: blue         │
└─────────────────────┘  └─────────────────────┘  └─────────────────────┘
```

**API calls:**
```
- GET /api/v1/hr/employees              → count total, active, new this month
- GET /api/v1/hr/departments            → count
- GET /api/v1/attendance/leaves?status=Pending → count
- GET /api/v1/hr/contracts              → filter endDate within 30 days
```

#### Row 2 — Bảng thông tin chi tiết (2 cột)
```
┌──────────────────────────────────┐  ┌──────────────────────────────────┐
│ 📊 Nhân viên theo phòng ban      │  │ 📋 Hợp đồng sắp hết hạn (30 ngày)│
│                                  │  │                                  │
│ Phòng ban | Số NV | % tổng | Bar │  │ Nhân viên | Loại HĐ | Ngày hết  │
│ IT        |   12  |  30%   | ███ │  │ Nguyễn A  | Chính thức | 15/07  │
│ Kế toán   |    8  |  20%   | ██  │  │ Trần B    | Thử việc   | 20/07  │
│ Marketing |    6  |  15%   | █   │  │ ...                              │
│ ...       |  ...  |  ...   | ... │  │ [Xem tất cả hợp đồng →]         │
│ [Xem NV →]                       │  │                                  │
└──────────────────────────────────┘  └──────────────────────────────────┘
```

#### Row 3 — Activity & Alerts
```
┌──────────────────────────────────┐  ┌──────────────────────────────────┐
│ 🆕 Nhân viên mới tháng này       │  │ ⚠️ Đơn nghỉ phép chờ duyệt       │
│                                  │  │                                  │
│ Avatar | Họ tên | Phòng | Ngày   │  │ NV | Loại nghỉ | Ngày | Số ngày │
│ [NV1]  | A.Nguyễn| IT  | 01/06  │  │ A  | Phép năm  | 25/06|  3 ngày │
│ [NV2]  | B.Trần  | KT  | 05/06  │  │ B  | Nghỉ ốm   | 26/06|  1 ngày │
│ ...                              │  │ [Duyệt] [Từ chối] ngay tại đây  │
│ [Xem tất cả →]                   │  │ [Xem tất cả →]                   │
└──────────────────────────────────┘  └──────────────────────────────────┘
```

---

### 👔 Manager Dashboard

#### Row 1 — Stats phòng mình (4 cards)
```
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ 👥 NV phòng  │  │ ✅ Có mặt    │  │ ⏳ Chờ duyệt │  │ 📅 Bảng công │
│ tôi          │  │ hôm nay      │  │ nghỉ phép    │  │ tháng này    │
│ [số]         │  │ [X]/[tổng]   │  │ [số] đơn     │  │ [link]       │
└──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘
```

#### Row 2 — Bảng chi tiết (2 cột)
```
┌──────────────────────────────────┐  ┌──────────────────────────────────┐
│ 🕐 Chấm công hôm nay (phòng tôi) │  │ ⏳ Đơn nghỉ chờ duyệt            │
│                                  │  │                                  │
│ NV | Check-in | Check-out | Giờ  │  │ NV | Loại | Từ → Đến | Ngày     │
│ A  | 08:01    | --:--     | ...  │  │ A  | Phép  | 25-27/06 | 3 ngày  │
│ B  | 08:45    | --:--     | ...  │  │ B  | Ốm    | 26/06    | 1 ngày  │
│ C  | Chưa vào | --:--     | --   │  │ [✅ Duyệt] [❌ Từ chối] mỗi dòng │
└──────────────────────────────────┘  └──────────────────────────────────┘
```

**API calls:**
```
- GET /api/v1/hr/employees?departmentId=xxx    → NV phòng mình
- GET /api/v1/attendance/attendance?date=today  → check-in hôm nay
- GET /api/v1/attendance/leaves?status=Pending  → đơn chờ (filter phòng)
```

---

### 💰 PayrollStaff Dashboard

#### Row 1 — KPI Tài chính (4 cards)
```
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ 📅 Kỳ đang   │  │ 🧾 Số phiếu  │  │ 💵 Tổng quỹ  │  │ ⚠️ Kỳ cần    │
│ mở           │  │ lương tháng  │  │ lương tháng  │  │ đóng         │
│ [số kỳ]      │  │ này          │  │ này          │  │ [số kỳ]      │
│ color: blue  │  │ [số phiếu]   │  │ [X,XXX,XXX đ]│  │ color: red   │
└──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘
```

#### Row 2 — Bảng xử lý lương (2 cột)
```
┌──────────────────────────────────┐  ┌──────────────────────────────────┐
│ 📋 Kỳ lương cần xử lý            │  │ 📊 Chi lương theo phòng ban       │
│                                  │  │                                  │
│ Tên kỳ | Từ→Đến | TT | Hành động│  │ Phòng ban | Số NV | Tổng Net     │
│ T6/26  | 1-30/6 | Draft | [Tính] │  │ IT        |   12  | 180,000,000 │
│ T5/26  | 1-31/5 | Calc  | [Đóng] │  │ Kế toán   |    8  | 120,000,000 │
│ ...    | ...    | ...   | ...    │  │ ...       |  ...  | ...          │
│ [+ Tạo kỳ mới]                   │  │ Tổng: [XXX,XXX,XXX đ]           │
└──────────────────────────────────┘  └──────────────────────────────────┘
```

**API calls:**
```
- GET /api/v1/payroll/payroll-periods        → danh sách kỳ lương
- GET /api/v1/payroll/payslips?periodId=xxx  → count + sum netSalary
- GET /api/v1/payroll/reports/summary        → chi lương theo phòng ban
```

---

### 👤 Employee Dashboard

> **Hiện tại:** Chỉ có 2 button + text chào — **THIẾU HOÀN TOÀN DATA!**

#### Layout mới — 3 vùng chính
```
┌─────────────────────────────────────────────────────────────────┐
│ 👋 Chào buổi sáng, Nguyễn Văn A!  Thứ 7, 21/06/2026            │
│    Phòng ban: IT  |  Chức vụ: Developer  |  Mã NV: EMP-001      │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────┐  ┌──────────────────────────────────┐
│ 🕐 Chấm công hôm nay     │  │ 💰 Phiếu lương gần nhất          │
│                          │  │                                  │
│ Ca: Ca sáng 08:00-17:00  │  │ Kỳ: Tháng 05/2026               │
│ Check-in:  08:05         │  │ Ngày công: 22 ngày               │
│ Check-out: --:--         │  │ Gross: 18,000,000 đ              │
│                          │  │ Net:   15,200,000 đ              │
│ [Button CHECK-OUT to lớn]│  │ [Xem chi tiết phiếu lương →]     │
└──────────────────────────┘  └──────────────────────────────────┘

┌──────────────────────────┐  ┌──────────────────────────────────┐
│ 📋 Đơn nghỉ phép của tôi │  │ 📅 Chấm công 7 ngày gần nhất     │
│                          │  │                                  │
│ [Pending] 2 đơn chờ duyệt│  │ Ngày  | Vào   | Ra    | Giờ làm │
│ [Approved] 1 đơn đã duyệt│  │ T6 21 | 08:01 | 17:05 | 8.0h   │
│                          │  │ T5 20 | 08:15 | 17:00 | 7.8h   │
│ [+ Tạo đơn mới]          │  │ T4 19 | --    | --    | Nghỉ   │
│ [Xem tất cả đơn của tôi] │  │ T3 18 | 07:55 | 17:10 | 8.2h   │
└──────────────────────────┘  └──────────────────────────────────┘
```

**API calls:**
```
- GET /api/v1/attendance/attendance/my-today?employeeId=xxx → trạng thái hôm nay
- GET /api/v1/payroll/payslips/me                           → phiếu lương gần nhất
- GET /api/v1/attendance/leaves?employeeId=xxx              → đơn nghỉ của tôi
- GET /api/v1/attendance/attendance?employeeId=xxx&limit=7  → 7 ngày gần nhất
```

---

## 🏥 Service Health Map (tất cả roles)

```
┌──────────────────────────────────────────────────────────────────────┐
│ 🏥 Trạng thái hệ thống                                  Refresh 30s  │
├──────────────────────────────────────────────────────────────────────┤
│ Service        │ Database           │ Port │ Status     │ Response    │
│ HR Core        │ HRMS_HrCoreDb      │ 5001 │ 🟢 Online  │ 45ms        │
│ Attendance     │ HRMS_AttendanceDb  │ 5002 │ 🟢 Online  │ 62ms        │
│ Payroll        │ HRMS_PayrollDb     │ 5003 │ 🟡 Slow    │ 850ms       │
│ API Gateway    │ —                  │ 5000 │ 🟢 Online  │ 12ms        │
└──────────────────────────────────────────────────────────────────────┘
```

**API calls:**
```
- GET /api/v1/hr/health          → HR service ping
- GET /api/v1/attendance/health  → Attendance service ping
- GET /api/v1/payroll/health     → Payroll service ping
→ Gọi mỗi 30s với Promise.allSettled() (lỗi 1 service không ảnh hưởng các service khác)
```

---

## ⚙️ Kỹ thuật triển khai

### Cấu trúc code đề xuất

```
DashboardView.vue (orchestrator — phân theo role)
├── AdminDashboard.vue    (Admin + HR)
├── ManagerDashboard.vue  (Manager)
├── PayrollDashboard.vue  (PayrollStaff)
├── EmployeeDashboard.vue (Employee)
└── ServiceHealthMap.vue  (Tất cả roles — ở cuối trang)
```

### Loading Strategy
```typescript
// Dùng Promise.allSettled để load song song
// Nếu 1 API lỗi → widget đó hiện "Không thể tải" riêng
// Widget khác vẫn hiện bình thường

const results = await Promise.allSettled([
  loadEmployees(),
  loadContracts(),
  loadLeaves(),
  loadPeriods(),
])
```

### Widget loading skeleton
```
- Mỗi card/bảng có loading skeleton riêng
- Không block toàn trang
- Sau khi load xong → hiện data, không giật/nhảy
```

### Quick Actions (inline từ Dashboard)
```typescript
// Manager duyệt nghỉ phép ngay trên Dashboard
async function approveLeave(id: string) {
  await leaveService.approve(id, auth.employeeId)
  // Refresh chỉ widget leaves, không reload toàn trang
  await loadPendingLeaves()
}
```

---

## 📐 Dữ liệu cần tính toán (client-side)

| Metric | Cách tính | Từ API nào |
|---|---|---|
| NV mới tháng này | filter `joinDate` trong tháng hiện tại | `/employees` |
| HĐ sắp hết hạn | filter `endDate` ≤ today + 30d | `/contracts` |
| Tỷ lệ có mặt hôm nay | check-in today / tổng NV phòng | `/attendance` + `/employees` |
| Tổng quỹ lương | sum(netSalary) của kỳ gần nhất | `/payslips?periodId=xxx` |
| Response time service | `Date.now()` trước/sau khi gọi /health | `/health` endpoints |

---

## 🎨 UI/UX Guidelines

```
- Stat cards: gradient nhẹ, icon SVG, số lớn, subtitle nhỏ
- Bảng: max 5-7 dòng, link "Xem tất cả →"
- Badge trạng thái: dùng AppBadge đã có
- Không dùng biểu đồ phức tạp (chart.js/echarts) → dùng CSS bar đơn giản
- Color scheme:
    emerald  → NV, active, positive
    blue     → info, periods, payroll
    amber    → pending, cần xử lý
    red      → urgent, hết hạn, lỗi
    violet   → phòng ban, cơ cấu tổ chức
- Responsive: mobile-first, grid cols 1 → 2 → 4
```

---

## 📋 Thứ tự triển khai (ưu tiên)

```
[P1 - Quan trọng nhất]
1. Employee Dashboard — hiện đang GẦN NHƯ TRỐNG
   → Thêm: check-in card, payslip card, leave card, history 7 ngày

2. PayrollStaff Dashboard — chỉ có 1 stat card
   → Thêm: tổng quỹ lương, số phiếu, bảng kỳ lương cần xử lý

[P2 - Quan trọng]
3. Admin/HR — bổ sung: HĐ sắp hết hạn, NV mới, quick approve
4. Manager — tách scope phòng mình, bảng chấm công hôm nay

[P3 - Nice to have]
5. Service Health Map — real-time ping 3 services
6. Tách thành các component con riêng biệt
```

---

> 🎯 **Mục tiêu:** Dashboard phải là **"trung tâm điều hành"** — mỗi role mở ra là thấy ngay những gì cần xử lý hôm nay, không cần click vào từng trang riêng lẻ.
