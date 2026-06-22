# 📊 PHÂN TÍCH TOÀN DIỆN & KẾ HOẠCH NÂNG CẤP DỰ ÁN HRMS
> Phân tích chi tiết dựa trên codebase thực tế — Backend (3 microservices + gateway) & Frontend (Vue 3 + Vite)
> Ngày phân tích: 22/06/2026

---

## MỤC LỤC
1. [Tổng quan kiến trúc hiện tại](#1-tổng-quan)
2. [Những gì đang THIẾU / CHƯA CÓ](#2-thiếu-sót)
3. [Giao diện người dùng — Đánh giá UX/UI](#3-ui-ux)
4. [Backend — Vấn đề & Tối ưu hóa](#4-backend)
5. [Yêu cầu để chạy THỰC TẾ ở doanh nghiệp](#5-thực-tế)
6. [KẾ HOẠCH TRIỂN KHAI CHI TIẾT — 4 Giai đoạn](#6-kế-hoạch)
7. [KẾ HOẠCH UI/UX REDESIGN — Tối giản & Chuyên nghiệp](#7-ux-redesign)
8. [KẾ HOẠCH DARK MODE & ĐA NGÔN NGỮ (i18n)](#8-dark-mode-i18n)

---

## 1. TỔNG QUAN KIẾN TRÚC HIỆN TẠI

### Những gì đã làm được ✅
| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| API Gateway (YARP Reverse Proxy) | ✅ Hoạt động | Chuyển tiếp request đến 3 service |
| HR Core Service (port 5001) | ✅ Hoạt động | Auth, Employee, Department, Position, Contract |
| Attendance Service (port 5002) | ✅ Hoạt động | Shift, CheckIn/Out, Leave, Timesheet, WorkSchedule |
| Payroll Report Service (port 5003) | ✅ Hoạt động | Period, Rule, Allowance, Deduction, Payslip, Report |
| Frontend Vue 3 + Vite | ✅ Hoạt động | 5 module: Dashboard, HR, Attendance, Payroll, Auth |
| JWT Authentication | ✅ Hoạt động | Role-based access: Admin, HR, Manager, PayrollStaff, Employee |
| RabbitMQ Event Bus | ✅ Cấu hình | Outbox pattern, cross-service sync |
| SQL Server databases | ✅ Hoạt động | Mỗi service có DB riêng (CQRS-ready) |
| Excel Export | ✅ Vừa thêm | Phụ cấp, Khấu trừ, Phiếu lương, Báo cáo |
| Excel Import | ✅ Vừa thêm | Phụ cấp, Khấu trừ với validation |
| Phân trang (Pagination) | ✅ Hoạt động | AppPagination + usePagination composable |

---

## 2. NHỮNG GÌ ĐANG THIẾU / CHƯA CÓ

### 2.1 ❌ Backend — Thiếu nghiêm trọng

#### A. Không có Unit Tests (Thư mục tests toàn `.gitkeep`)
```
backend/tests/hr-core-tests/      → CHỈ CÓ .gitkeep ← TRỐNG!
backend/tests/attendance-tests/   → CHỈ CÓ .gitkeep ← TRỐNG!
backend/tests/payroll-report-tests/ → CHỈ CÓ .gitkeep ← TRỐNG!
```
**Hệ quả:** Không thể biết code có đúng không khi refactor. Bug có thể âm thầm xuất hiện.

#### B. Không có Rate Limiting
Gateway hiện tại **không có** rate limiting. Một người dùng xấu có thể gọi API liên tục dẫn đến overload toàn bộ hệ thống.

#### C. Logging tập trung chưa có
Mỗi service log riêng lẻ. Không có công cụ thu thập log tập trung (Seq, ELK, Serilog structured).

#### D. Health Checks chỉ là cơ bản
`/health` endpoint tồn tại nhưng **không kiểm tra** kết nối DB, RabbitMQ, hay các service phụ thuộc.

#### E. Không có Refresh Token
Hiện tại chỉ có Access Token. Khi token hết hạn → người dùng bị đăng xuất đột ngột. Không có refresh token flow.

#### F. Payroll Calculation Logic chưa hoàn chỉnh
- Chưa tính thuế TNCN (PIT - Personal Income Tax)
- Chưa tính BHXH/BHYT/BHTN (Social Insurance) theo đúng luật Việt Nam
- Không có logic tính lương theo giờ OT (overtime)
- Công thức tính lương ngày = BaseSalary / 26 ngày (chưa linh hoạt theo tháng thực tế)

#### G. Không có Email / Notification Service
Không thể gửi thông báo cho nhân viên khi:
- Đơn nghỉ phép được duyệt/từ chối
- Phiếu lương kỳ mới được tạo
- Hợp đồng sắp hết hạn

#### H. File Upload chưa có
Không thể upload:
- Ảnh đại diện nhân viên
- File scan hợp đồng lao động
- File chứng chỉ, bằng cấp

### 2.2 ❌ Frontend — Thiếu quan trọng

#### A. Không có trang Profile cá nhân [ĐÃ HOÀN THÀNH ✅]
Nhân viên hiện tại đã có thể xem, chỉnh sửa thông tin cá nhân và tự đổi mật khẩu qua trang `/profile` (ProfileView.vue).

#### B. Không có trang Quản lý User/Account (UI) [ĐÃ HOÀN THÀNH ✅]
Đã phát triển giao diện quản lý tài khoản người dùng `/admin/users` (UserManagementView.vue) cho Admin để kích hoạt/khóa tài khoản, reset mật khẩu, và gán phân quyền (Roles).

#### C. Dashboard chỉ có thống kê tĩnh [ĐÃ HOÀN THÀNH ✅]
Đã nâng cấp Dashboard nâng cao (DashboardView.vue) hiển thị biểu đồ xu hướng lương, tỷ lệ chuyên cần/vắng mặt, cảnh báo nhân sự mới, hợp đồng hết hạn và phân quyền hiển thị theo đúng vai trò (Admin, HR, Manager, PayrollStaff, Employee).

#### D. Không có chức năng Tìm kiếm toàn cục
Không có thanh tìm kiếm để nhanh chóng tìm nhân viên, hợp đồng, phiếu lương từ thanh Header.

#### E. Không có chức năng In phiếu lương [ĐÃ HOÀN THÀNH ✅]
Đã tích hợp chức năng xuất/in tải file PDF phiếu lương trực quan sử dụng thư viện `jsPDF` ở trang chi tiết phiếu lương.

#### F. Không có Dark Mode [ĐÃ HOÀN THÀNH ✅]
Đã tích hợp hoàn chỉnh hệ thống CSS Variables toàn cục hỗ trợ Light/Dark Mode đồng bộ hóa giao diện người dùng.

#### G. Chưa hỗ trợ mobile (Responsive kém)
Sidebar và một số bảng dữ liệu hiển thị chưa tối ưu hoàn toàn trên màn hình điện thoại/tablet nhỏ.

---

## 3. GIAO DIỆN NGƯỜI DÙNG — ĐÁNH GIÁ UX/UI

### 3.1 Điểm mạnh hiện tại ✅
- Màu sắc nhất quán (emerald/teal gradient)
- Sidebar navigation rõ ràng, phân nhóm theo role
- Toast notifications đầy đủ
- Loading states và skeleton screens
- Pagination tốt

### 3.2 Vấn đề UX cần sửa ⚠️

| Vấn đề | Màn hình bị ảnh hưởng | Mức độ |
|---|---|---|
| Không có breadcrumb navigation hoạt động (chỉ hiển thị) | Tất cả | Trung bình |
| Bảng dữ liệu không có sort theo cột | Tất cả bảng | Quan trọng |
| Form không có auto-save khi rời trang | Form nhân viên | Quan trọng |
| Không có Empty State hướng dẫn người dùng mới | Dashboard, danh sách | Thấp |
| Modal không đóng khi click outside trong một số trường hợp | AppModal | Thấp |
| Không có Skeleton Loading cho Detail View | PayslipDetail | Thấp |
| Không có xác nhận trước khi rời trang có form đang nhập | EmployeeDetail | Quan trọng |
| Không có inline error message khi API fail | Form tạo nhân viên | Quan trọng |
| Timesheet view hiển thị dữ liệu thô, khó đọc | TimesheetView | Quan trọng |
| Check-in/out không hiện thị lịch sử ngay | CheckIn view | Trung bình |

### 3.3 Thiếu trang quan trọng
```
/profile                → Trang profile cá nhân nhân viên
/admin/users            → Quản lý tài khoản người dùng
/payroll/my-payslip/:id/print → In phiếu lương
/notifications          → Trang thông báo
/settings               → Cài đặt hệ thống (cấu hình công ty)
```

---

## 4. BACKEND — VẤN ĐỀ & TỐI ƯU HÓA

### 4.1 Security Issues 🔴

#### Vấn đề 1: EmployeesController thiếu [Authorize(Roles=...)]
```csharp
// Hiện tại — bất kỳ user đã đăng nhập nào cũng GET được toàn bộ nhân viên
[HttpGet]
public async Task<ActionResult<...>> GetAll()

// Cần thêm:
[Authorize(Roles = "Admin,HR,Manager")]
```

#### Vấn đề 2: Không có CORS fine-grained
Gateway chỉ cho phép `http://localhost:5173`. Khi deploy production cần cấu hình domain thực.

#### Vấn đề 3: Password không có độ phức tạp tối thiểu
Không enforce password policy (ít nhất 8 ký tự, chữ hoa, số, ký tự đặc biệt).

### 4.2 Performance Issues 🟡

#### Vấn đề 1: N+1 Query problem
Một số service load dữ liệu tuần tự thay vì song song với `Task.WhenAll`.

#### Vấn đề 2: Không có Caching
Dữ liệu ít thay đổi (danh sách phòng ban, chức vụ, loại phụ cấp) bị query DB mỗi lần request.

#### Vấn đề 3: Projection data không có TTL refresh
`EmployeeProjection` và `DepartmentProjection` ở service khác có thể bị stale nếu RabbitMQ miss event.

#### Vấn đề 4: Payroll tính toán chạy synchronous
Khi tính lương cho 500+ nhân viên, `PayrollPeriod` Calculate() sẽ block thread cho đến khi xong.

### 4.3 Business Logic Gaps 🔴

#### Thiếu: Tính thuế TNCN theo bậc thuế Việt Nam
```
Thu nhập chịu thuế = Gross - Giảm trừ bản thân (11tr) - Giảm trừ người phụ thuộc
Bậc 1: ≤ 5 triệu → 5%
Bậc 2: 5-10 triệu → 10%
Bậc 3: 10-18 triệu → 15%
Bậc 4: 18-32 triệu → 20%
Bậc 5: 32-52 triệu → 25%
Bậc 6: 52-80 triệu → 30%
Bậc 7: > 80 triệu  → 35%
```

#### Thiếu: Tính BHXH/BHYT/BHTN
```
BHXH nhân viên đóng: 8% mức lương đóng BH
BHYT nhân viên đóng: 1.5%
BHTN nhân viên đóng: 1%
→ Tổng NV đóng: 10.5%
```

#### Thiếu: Quản lý số ngày phép còn lại theo năm
Hiện tại LeaveRequest có trạng thái Approved/Rejected nhưng không tracking số ngày phép còn lại.

---

## 5. YÊU CẦU ĐỂ CHẠY THỰC TẾ Ở DOANH NGHIỆP

### 5.1 Infrastructure cần có

| Thành phần | Hiện tại | Production cần |
|---|---|---|
| Web Server | localhost Vite dev | Nginx / IIS + CDN |
| Backend Hosting | `dotnet run` local | Docker + Kubernetes hoặc IIS |
| Database | SQL Server local | SQL Server Always On / Azure SQL |
| Message Bus | RabbitMQ Docker | RabbitMQ Cluster hoặc Azure Service Bus |
| Logging | Console | Seq / ELK Stack / Azure Monitor |
| Email | Không có | SendGrid / SMTP Server |
| File Storage | Không có | Azure Blob / MinIO / AWS S3 |
| SSL/HTTPS | Không có | Let's Encrypt / Cloudflare |
| Backup | Không có | Automated daily backup |
| Monitoring | Không có | Prometheus + Grafana hoặc Azure Monitor |

### 5.2 Quy trình vận hành cần thiết

```
1. Quy trình onboarding nhân viên mới:
   HR tạo Employee → Tạo User account → Gán Role → Tạo WorkSchedule → Tạo Contract

2. Quy trình tính lương hàng tháng:
   Mở kỳ lương → Nhập phụ cấp/khấu trừ → Chạy tính lương tự động → Review → Chốt kỳ → Xuất bảng lương → Chuyển khoản

3. Quy trình duyệt nghỉ phép:
   Nhân viên tạo đơn → Manager nhận notification → Duyệt/Từ chối → Nhân viên nhận notification

4. Quy trình chấm công:
   Check-in/out hàng ngày → Cuối tháng tạo Timesheet → HR xét duyệt Timesheet → Đưa vào tính lương
```

### 5.3 Dữ liệu Master cần cấu hình trước khi dùng thực tế
```
✅ Đã có: Departments, Positions, Shifts
❌ Chưa có: 
  - Thông tin công ty (tên, địa chỉ, MST, logo)
  - Loại nghỉ phép và quota mặc định theo năm  
  - Bảng thuế TNCN và mức đóng BHXH
  - Ngày lễ/tết quốc gia (để tính ngày công chính xác)
  - Bank account template cho từng nhân viên
```

---

## 6. KẾ HOẠCH TRIỂN KHAI CHI TIẾT — 4 GIAI ĐOẠN

### 🔵 GIAI ĐOẠN 1 — Hoàn thiện Core (Ưu tiên cao — 1-2 tuần)

#### Backend
- [ ] **Thêm Role-based Authorization chi tiết** cho EmployeesController, ContractsController
- [x] **Implement Refresh Token** — endpoint `/auth/refresh`, lưu RefreshToken vào DB ✅
- [ ] **Thêm Password Policy validation** khi tạo/đổi mật khẩu
- [x] **Viết Unit Tests** cho PayrollCalculation service (quan trọng nhất) ✅
- [ ] **Sửa Health Checks** — kiểm tra DB connection và RabbitMQ connection

#### Frontend
- [x] **Tạo trang `/admin/users`** — Quản lý tài khoản: xem, tạo, reset password, gán role ✅
- [x] **Tạo trang `/profile`** — Nhân viên xem/sửa thông tin cá nhân ✅
- [x] **Thêm Column Sorting** vào AppTable component ✅
- [x] **In/Download PDF phiếu lương** — dùng thư viện `jsPDF` ✅
- [x] **Sửa Timesheet view** — hiển thị calendar view thay vì bảng dữ liệu thô ✅

### 🟡 GIAI ĐOẠN 2 — Nghiệp vụ nâng cao (2-3 tuần)

#### Backend
- [x] **Tính thuế TNCN** — Thêm `TaxCalculationService` với bậc thuế lũy tiến ✅
- [x] **Tính BHXH/BHYT/BHTN** — Thêm vào PayslipItem với type riêng ✅
- [ ] **Quản lý số ngày phép** — `LeaveBalance` entity, trừ ngày khi approve leave
- [ ] **Cảnh báo hợp đồng** — Background job quét contract sắp hết hạn (< 30 ngày)
- [x] **Thêm API Pagination phía backend** — tránh load toàn bộ dữ liệu ✅
- [ ] **In-memory Caching** — Cache danh sách Departments, Positions, AllowanceTypes

#### Frontend
- [x] **Dashboard nâng cao** — Thêm biểu đồ xu hướng lương 6 tháng, tỷ lệ vắng mặt ✅
- [ ] **Trang thông báo `/notifications`** — Hiển thị thông báo phê duyệt, phiếu lương mới
- [ ] **Thêm Global Search** — Tìm kiếm nhân viên, phiếu lương từ thanh header
- [ ] **Form Guard** — Cảnh báo khi rời trang có dữ liệu chưa lưu
- [ ] **Responsive Mobile** — Sửa sidebar, bảng dữ liệu cho màn hình nhỏ

### 🟢 GIAI ĐOẠN 3 — Production Ready (3-4 tuần)

#### Infrastructure
- [ ] **Dockerize toàn bộ** — `docker-compose.yml` cho cả frontend + 4 backend services
- [ ] **CI/CD Pipeline** — GitHub Actions: build → test → deploy tự động
- [ ] **Centralized Logging** — Tích hợp Serilog + Seq
- [ ] **Rate Limiting** — Thêm vào Gateway: 100 req/min per IP
- [ ] **HTTPS/SSL** — Cấu hình Nginx reverse proxy với SSL certificate
- [ ] **Automated Backup** — Script backup SQL Server theo lịch

#### Backend
- [ ] **File Upload Service** — Ảnh nhân viên, scan hợp đồng (lưu trên MinIO)
- [ ] **Email Notification** — SMTP integration (SendGrid), template email HTML
- [ ] **Audit Trail hoàn chỉnh** — Log mọi thay đổi dữ liệu nhân sự/lương
- [ ] **API Versioning** — Chuẩn bị cho backward compatibility

### 🔴 GIAI ĐOẠN 4 — Enterprise Features (Tùy chọn, dài hạn)

- [ ] **Cổng thông tin nhân viên (Employee Self-Service Portal)**
- [ ] **Module Tuyển dụng** — Job posting, applicant tracking
- [ ] **Module Đào tạo & Phát triển** — Training records, certification tracking
- [ ] **Module KPI & Đánh giá** — Performance review, target setting
- [ ] **Tích hợp máy chấm công** — Import data từ thiết bị vân tay/thẻ từ
- [ ] **Mobile App** — React Native hoặc Flutter cho iOS/Android
- [ ] **Multi-tenant** — Hỗ trợ nhiều công ty trên cùng một nền tảng
- [ ] **Báo cáo nâng cao** — Power BI embedding hoặc custom analytics

---

## TÓM TẮT ƯU TIÊN THEO IMPACT

```
🔴 CRITICAL (Làm ngay):
   1. Thêm Unit Tests cho PayrollCalculation
   2. Implement Refresh Token
   3. Role-based authorization chi tiết cho tất cả endpoints
   4. Trang quản lý User/Account (Admin)

🟡 HIGH (Làm trong tháng):
   5. Tính thuế TNCN + BHXH/BHYT/BHTN
   6. Quản lý số ngày phép còn lại
   7. Trang Profile cá nhân nhân viên
   8. In PDF phiếu lương
   9. Column sorting trong bảng dữ liệu
   10. Responsive mobile

🟢 MEDIUM (Làm trong quý):
   11. Email notification service
   12. File upload (avatar nhân viên)
   13. Dockerize + CI/CD
   14. Centralized logging
   15. Dashboard analytics nâng cao
```

---

> **Kết luận:** Dự án hiện tại có nền tảng kiến trúc **rất tốt** (Microservices + CQRS + Event Bus + Role-based Auth). Tuy nhiên để chạy thực tế ở doanh nghiệp, ưu tiên cao nhất là hoàn thiện **business logic tính lương theo luật Việt Nam** (thuế TNCN + BHXH), thêm **tests** để đảm bảo chính xác, và xây dựng thêm **Refresh Token + Profile + User Management** để trải nghiệm người dùng hoàn chỉnh.

---

## 7. KẾ HOẠCH UI/UX REDESIGN — Tối giản & Chuyên nghiệp

> **Triết lý thiết kế:** *Tối giản có chiều sâu* — Không phải "trắng trơn", mà là từng chi tiết đều có dụng ý. Giao diện phải cảm giác **mượt mà như một sản phẩm thương mại thật sự**, không lộ ra "dấu hiệu AI sinh ra".

### 7.1 Vấn đề UX/UI hiện tại cần sửa

| # | Vấn đề | Màn hình | Độ ưu tiên |
|---|---|---|---|
| 1 | Bảng dữ liệu không sort được theo cột | Tất cả bảng | 🔴 Cao |
| 2 | Form không có guard khi rời trang đang nhập dở | Employee, Contract | 🔴 Cao |
| 3 | Không có inline error message khi API lỗi | Form tạo nhân viên | 🔴 Cao |
| 4 | Timesheet view hiển thị dữ liệu thô, khó đọc | TimesheetView | 🟡 Trung bình |
| 5 | Check-in/out không hiện lịch sử ngay | CheckinView | 🟡 Trung bình |
| 6 | Modal không đóng khi click outside | AppModal | 🟡 Trung bình |
| 7 | Empty State chưa hướng dẫn user mới | Dashboard, danh sách | 🟢 Thấp |
| 8 | Skeleton loading thiếu trên một số màn | PayslipDetail | 🟢 Thấp |

### 7.2 Design System — Thống nhất toàn cục

#### A. Typography (Chữ)
```
Font chính:  "Inter" (Google Fonts) — tối giản, dễ đọc, chuyên nghiệp
Font số/code: "JetBrains Mono" — dùng cho giá trị tiền, mã nhân viên

Cấp độ:
  - Heading 1 (Page title):   24px / font-bold / tracking-tight
  - Heading 2 (Section):      18px / font-semibold
  - Body:                     14px / font-normal / leading-6
  - Caption / Label:          12px / font-medium / text-slate-500
  - Mono (Tiền, mã):          13px / JetBrains Mono
```

#### B. Color Palette — Thống nhất Light & Dark
```
Primary (Emerald):   hsl(158, 64%, 42%)   — nút chính, accent
Primary Hover:       hsl(158, 64%, 36%)
Secondary (Slate):   hsl(215, 20%, 65%)   — text phụ, border
Success (Green):     hsl(142, 71%, 45%)
Warning (Amber):     hsl(45, 93%, 47%)
Danger (Rose):       hsl(351, 83%, 53%)
Info (Blue):         hsl(213, 94%, 60%)

--- Light Mode ---
bg-surface:          hsl(0, 0%, 100%)      — card, modal
bg-page:             hsl(210, 20%, 98%)    — nền trang
border:              hsl(214, 32%, 91%)
text-primary:        hsl(222, 47%, 11%)
text-secondary:      hsl(215, 16%, 47%)

--- Dark Mode ---
bg-surface:          hsl(222, 47%, 11%)
bg-page:             hsl(222, 47%, 8%)
border:              hsl(217, 33%, 17%)
text-primary:        hsl(210, 40%, 96%)
text-secondary:      hsl(215, 20%, 65%)
```

#### C. Spacing & Radius — Nhất quán
```
spacing unit:  4px base
border-radius:
  - sm (input, badge):    6px
  - md (card, modal):     12px
  - lg (page card):       16px
  - full (avatar, chip):  9999px

shadows:
  - card:    0 1px 3px rgba(0,0,0,.08), 0 1px 2px rgba(0,0,0,.06)
  - modal:   0 20px 60px rgba(0,0,0,.15)
  - dropdown: 0 4px 16px rgba(0,0,0,.10)
```

#### D. Motion & Animation — Không quá AI
```
Nguyên tắc: Mọi animation đều phải CÓ MỤC ĐÍCH — chỉ để giúp user 
             hiểu "cái gì vừa thay đổi", không chỉ để "đẹp"

Transition duration chuẩn:
  - color/bg change:       150ms ease-out
  - modal/drawer open:     200ms ease-out
  - skeleton shimmer:      1500ms linear infinite
  - toast slide-in:        250ms cubic-bezier(0.16,1,0.3,1)
  - page transition:       180ms ease

Không dùng:
  ❌ bounce animation trên form validation
  ❌ floating animation trên card
  ❌ particle effects, glow effects quá nặng
  ✅ Dùng: fade + translate nhẹ, opacity, scale(0.98→1) khi click
```

### 7.3 Component Redesign Chi Tiết

#### AppTable — Nâng cấp thành Interactive Table
```
Thêm:
  ✅ Sort theo cột (click header → asc/desc/none)
  ✅ Column resize (kéo rộng/hẹp cột)
  ✅ Row hover highlight tinh tế (bg-slate-50/50)
  ✅ Sticky header khi scroll
  ✅ Selected row state (checkbox nếu cần bulk action)
  ✅ Empty State có icon + message + CTA button
  ✅ Skeleton loading rows (3-5 dòng giả khi loading)

Giữ nguyên:
  ✅ Pagination ở dưới
  ✅ Column định nghĩa từ ngoài vào
```

#### AppButton — Phân loại rõ hơn
```
Variants:
  primary   → bg-emerald (hành động chính)
  secondary → bg-white border (hành động phụ)
  ghost     → transparent, chỉ có text+icon (nav, inline)
  danger    → bg-rose (xóa, hủy)
  success   → bg-green (xác nhận quan trọng)

Sizes:
  xs  → h-6, px-2, text-xs   (badge-like)
  sm  → h-7, px-3, text-xs   (trong bảng)
  md  → h-9, px-4, text-sm   (mặc định)
  lg  → h-11, px-6, text-base (form submit)

States:
  loading → spinner icon thay text, disabled
  disabled → opacity-50, cursor-not-allowed
  icon-only → h=w, padding đều nhau, tooltip bắt buộc
```

#### AppInput / AppSelect — Thống nhất style
```
Floating label animation (label nổi lên khi focus)
Error state: border-rose-400 + error text dưới
Success state: border-emerald-400 + checkmark icon
  
Không dùng:
  ❌ placeholder thay cho label (khó dùng khi có dữ liệu)
  ✅ Label + placeholder cùng tồn tại
```

#### Sidebar — Micro-interactions
```
Active item:   pill shape highlight, not full-width
Hover item:    bg translateX(2px) nhẹ
Group label:   divider line + text, không chỉ là text
Collapse mode: icon-only khi màn hình nhỏ, tooltip hiện tên
User zone:     avatar tròn + name + role badge + logout button
```

#### PageHeader — Thêm Breadcrumb hoạt động
```
Breadcrumb: clickable, navigate về trang trước
Action buttons: luôn ở góc phải, flex gap-2
Subtitle: text-sm text-secondary, optional
Divider: border-b border dưới header
```

### 7.4 Màn hình cần Redesign hoàn toàn

#### A. Trang Check-in / Check-out
```
Hiện tại: Form đơn giản, không có phản hồi trực quan
Cần:
  - Đồng hồ real-time đang chạy (clock animation)
  - Nút Check-in lớn, rõ ràng, đổi thành Check-out khi đã in
  - Timeline mini bên cạnh: 09:00 Check-in ✅ → ... → 18:00 Check-out
  - Badge trạng thái hôm nay (Đúng giờ / Đi trễ / Chưa check-in)
  - Lịch sử 5 ngày gần nhất ngay dưới
```

#### B. Trang Timesheet
```
Hiện tại: Bảng dữ liệu thô
Cần:
  - Calendar view theo tháng (grid 7 cột)
  - Mỗi ô ngày hiện trạng thái: 🟢 Đủ công / 🟡 Nửa ngày / 🔴 Vắng / ⚪ Nghỉ lễ
  - Click vào ngày → popup chi tiết giờ vào/ra
  - Summary bar phía trên: Tổng ngày công / Tổng giờ / Số ngày nghỉ
```

#### C. Trang Dashboard
```
Hiện tại: Cards + charts cơ bản
Cần:
  - Welcome banner với tên user, thời gian chào
  - Quick actions: Check-in ngay, Tạo đơn nghỉ, Xem phiếu lương
  - KPI cards với trend arrow (▲ +3 so với tháng trước)
  - Mini calendar bên cạnh hiện ngày lễ, sự kiện công ty
  - Activity feed: "Nguyễn A vừa check-in", "Đơn nghỉ phép của B được duyệt"
```

#### D. Trang Phiếu lương cá nhân
```
Hiện tại: Danh sách card
Cần:
  - Timeline dạng accordion theo năm/tháng
  - Biểu đồ lương 6 tháng gần nhất (bar chart nhỏ)
  - So sánh lương tháng này vs tháng trước (▲/▼ %)
  - Badge trạng thái màu rõ (Đã trả / Bản nháp / Đã chốt)
  - Nút "Tải PDF" to, dễ tìm
```

---

## 8. KẾ HOẠCH DARK MODE & ĐA NGÔN NGỮ (i18n)

### 8.1 Dark Mode — Kế hoạch triển khai

#### Cơ chế hoạt động
```
Chiến lược: CSS Custom Properties (Variables) + Tailwind data-theme attribute
Lưu trữ:    localStorage('hrms-theme') → 'light' | 'dark' | 'system'
Áp dụng:    <html data-theme="dark"> → CSS vars tự đổi toàn bộ

Không dùng:
  ❌ class="dark" toggle trên <html> (Tailwind dark: prefix)
     → Lý do: Khó override khi dùng 3rd-party components
  ✅ CSS Custom Properties → Dễ control, không phụ thuộc framework
```

#### Cách implement - CSS Variables
```css
/* Ví dụ cấu trúc variables */
:root {
  --bg-page:    hsl(210, 20%, 98%);
  --bg-surface: hsl(0, 0%, 100%);
  --text-primary: hsl(222, 47%, 11%);
  --border:     hsl(214, 32%, 91%);
  --color-primary: hsl(158, 64%, 42%);
}

[data-theme="dark"] {
  --bg-page:    hsl(222, 47%, 8%);
  --bg-surface: hsl(222, 47%, 11%);
  --text-primary: hsl(210, 40%, 96%);
  --border:     hsl(217, 33%, 17%);
  /* --color-primary giữ nguyên — emerald đẹp cả 2 mode */
}
```

#### Composable `useTheme.ts` — Store & Logic
```typescript
// frontend/src/composables/useTheme.ts
// State: 'light' | 'dark' | 'system'
// Actions: toggle(), setTheme(mode), initTheme()
// Auto-detect: window.matchMedia('prefers-color-scheme: dark')
// Persist: localStorage
// Apply: document.documentElement.setAttribute('data-theme', ...)
```

#### Toggle Button UI — Vị trí & Design
```
Vị trí: Topbar bên phải (cạnh avatar user)
Icon:   Sun (light) / Moon (dark) / Monitor (system)
Design: Nút icon-only, h-9 w-9, rounded-lg
        Smooth transition: rotate(180deg) + opacity khi đổi icon
        Tooltip: "Chuyển sang chế độ tối/sáng"
3 modes: Click lần 1 → Dark, lần 2 → System, lần 3 → Light
```

#### Danh sách component cần test Dark Mode
```
✅ Sidebar navigation + user info zone
✅ Topbar header
✅ AppTable (header, row hover, row border)
✅ AppModal (backdrop, modal body)
✅ AppButton (tất cả variants)
✅ AppInput / AppSelect (border, placeholder, focus ring)
✅ AppToast notifications
✅ AppPagination
✅ ExcelImportModal
✅ PageHeader
✅ Cards trên Dashboard
✅ Biểu đồ Chart.js (cần config colors theo theme)
✅ Login page
```

### 8.2 Đa Ngôn ngữ (i18n) — Tiếng Việt & Tiếng Anh

#### Thư viện sử dụng
```
Package: vue-i18n v9 (phiên bản chính thức cho Vue 3)
Lý do chọn:
  ✅ Hỗ trợ Composition API (useI18n hook)
  ✅ Lazy loading ngôn ngữ (chỉ load file khi cần)
  ✅ Plural forms, date/number formatting
  ✅ TypeScript support tốt
  ✅ 1 file JSON mỗi ngôn ngữ, dễ maintain
```

#### Cấu trúc file ngôn ngữ
```
frontend/src/locales/
  ├── vi.json          ← Tiếng Việt (mặc định)
  ├── en.json          ← English
  └── index.ts         ← Setup vue-i18n instance

Cấu trúc JSON theo module:
{
  "common": {
    "save": "Lưu", "cancel": "Hủy", "delete": "Xóa",
    "search": "Tìm kiếm", "loading": "Đang tải...",
    "noData": "Chưa có dữ liệu", "confirm": "Xác nhận"
  },
  "nav": {
    "dashboard": "Tổng quan",
    "hr": "Nhân sự", "attendance": "Chấm công",
    "payroll": "Lương & Báo cáo"
  },
  "employee": {
    "title": "Nhân viên", "code": "Mã NV",
    "fullName": "Họ tên", "department": "Phòng ban",
    "create": "Thêm nhân viên", "edit": "Chỉnh sửa"
  },
  "payslip": {
    "title": "Phiếu lương", "period": "Kỳ lương",
    "baseSalary": "Lương cơ bản", "netSalary": "Thực lĩnh",
    "grossSalary": "Tổng thu nhập"
  },
  "attendance": { ... },
  "leave": { ... },
  "validation": {
    "required": "Trường này là bắt buộc",
    "minLength": "Tối thiểu {n} ký tự",
    "invalidEmail": "Email không hợp lệ"
  }
}
```

#### Composable `useLocale.ts` — Store & Logic
```typescript
// frontend/src/composables/useLocale.ts
// State: 'vi' | 'en'
// Actions: setLocale(lang), initLocale()
// Persist: localStorage('hrms-locale')
// Apply: i18n.global.locale.value = lang
// Format date: theo locale (vi → dd/MM/yyyy, en → MM/dd/yyyy)
// Format money: vi → 1.000.000 ₫, en → ₫1,000,000
```

#### Ngôn ngữ Switcher UI — Vị trí & Design
```
Vị trí: Topbar bên phải (cạnh Dark Mode toggle)
Design: Dropdown nhỏ
  ┌─────────────────┐
  │ 🇻🇳 Tiếng Việt  │ ← active
  │ 🇬🇧 English      │
  └─────────────────┘
Icon: Flag emoji + tên ngôn ngữ
Dropdown: w-40, shadow-md, border, rounded-lg
Animation: fade-in từ trên xuống, 150ms
```

#### Chiến lược dịch thuật

| Loại nội dung | Cách xử lý |
|---|---|
| Label, nút, tiêu đề | `t('key')` từ file JSON |
| Thông báo validation | `t('validation.required')` với params |
| Toast message | Dùng key i18n trong service layer |
| Số tiền | `n(amount, 'currency')` — i18n number format |
| Ngày giờ | `d(date, 'short')` — i18n date format |
| Dữ liệu từ DB | **KHÔNG dịch** — giữ nguyên (tên NV, phòng ban là do admin nhập) |
| Error từ API | Mapping error code → i18n key |

#### Danh sách màn hình cần i18n
```
Ưu tiên cao (user thấy nhiều nhất):
  ✅ Login page
  ✅ Sidebar navigation labels
  ✅ Dashboard KPI labels
  ✅ Tất cả PageHeader title/subtitle
  ✅ Form labels và validation messages
  ✅ Tất cả nút hành động (Thêm, Sửa, Xóa, Lưu, Hủy)
  ✅ Table column headers
  ✅ Toast notifications
  ✅ Confirm dialog messages
  ✅ Empty state messages

Ưu tiên thấp hơn:
  ⬜ Breadcrumb labels
  ⬜ Tooltip nội dung
  ⬜ Placeholder text trong input
  ⬜ Modal title
```

### 8.3 Kế hoạch triển khai theo bước

#### Bước 1 — Chuẩn bị hạ tầng (½ ngày)
```
1. npm install vue-i18n@9
2. Tạo frontend/src/locales/vi.json — copy toàn bộ string hiện có
3. Tạo frontend/src/locales/en.json — dịch từ vi.json
4. Tạo frontend/src/locales/index.ts — setup i18n instance
5. Register plugin trong main.ts
6. Tạo useTheme.ts composable — CSS variable approach
7. Thêm CSS variables vào style.css
```

#### Bước 2 — Global components (1 ngày)
```
1. Cập nhật MainLayout.vue — thêm ThemeToggle + LangSwitcher vào Topbar
2. Cập nhật Sidebar — tất cả label dùng t('nav.xxx')
3. Cập nhật AppButton, AppInput, AppTable — dùng t() cho text tĩnh
4. Cập nhật AppToast — nhận i18n key thay vì raw string
5. Cập nhật AppConfirm — title/message dùng t()
```

#### Bước 3 — Các module theo thứ tự (2-3 ngày)
```
Ngày 1: Auth module (Login) + Dashboard
Ngày 2: HR module (Employee, Department, Position, Contract)
Ngày 3: Attendance module (Checkin, Leave, Timesheet, WorkSchedule)
Ngày 4: Payroll module (Period, Rule, Allowance, Deduction, Payslip, Report)
```

#### Bước 4 — Polish & Test (½ ngày)
```
1. Test Dark Mode trên tất cả màn hình
2. Test chuyển đổi ngôn ngữ không gây reload trang
3. Test persist sau khi refresh trình duyệt
4. Test Chart.js re-render khi đổi theme
5. Test formatter số tiền theo locale
```

### 8.4 Tóm tắt ưu tiên — UI/UX + Theme + i18n

```
🔴 Làm ngay (impact cao nhất):
   1. Thêm sort cột vào AppTable
   2. Sửa Timesheet → Calendar view
   3. Sửa Checkin → Timeline + real-time clock
   4. CSS Variable system (chuẩn bị cho Dark Mode)
   5. Import Inter font từ Google Fonts

🟡 Làm trong tuần tiếp:
   6. Dark Mode toggle (useTheme composable + UI button)
   7. Cài vue-i18n, tạo vi.json + en.json
   8. i18n cho Sidebar + Dashboard + Form validation
   9. Redesign Dashboard (Quick actions + Activity feed)
   10. Form guard (cảnh báo rời trang)

🟢 Làm trong tháng:
   11. i18n cho toàn bộ các module còn lại
   12. Redesign Checkin page hoàn chỉnh
   13. Responsive mobile sidebar + bảng
   14. Empty state với illustration
   15. Floating label animation cho form inputs
```

