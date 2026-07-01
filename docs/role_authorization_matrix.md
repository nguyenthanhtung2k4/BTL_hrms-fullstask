# Ma Trận Phân Quyền Hệ Thống HRMS (Role & Authorization Matrix)

Tài liệu này định nghĩa chi tiết quyền hạn, luồng xử lý và giới hạn thao tác của từng Vai trò (Role) trên toàn bộ hệ thống Microservices HRMS. Tài liệu đóng vai trò là kim chỉ nam giúp đồng bộ logic lập trình ở cả Backend (API Controllers) và Frontend (Route Guards, Sidebar Menu, UI Action Buttons).

---

## 1. Định Nghĩa 5 Vai Trò (Roles) Hệ Thống

1. **Admin (Quản trị viên)**: Quyền tối cao. Quản lý tài khoản hệ thống, cấu hình hệ thống, phòng ban, chức vụ và có toàn quyền kiểm soát tất cả dữ liệu.
2. **HR (Quản lý Nhân sự)**: Quản lý hồ sơ nhân viên, hợp đồng lao động, cấu hình phòng ban/chức vụ và xem báo cáo tổng hợp nhân sự.
3. **Manager (Quản lý bộ phận)**: Quản lý trực tiếp các nhân viên thuộc phòng ban mình phụ trách. Được phân ca/lịch làm việc, duyệt đơn nghỉ phép và xem báo cáo công trong phạm vi quản lý.
4. **PayrollStaff (Kế toán lương)**: Quản lý kỳ lương, quy tắc tính lương, các khoản phụ cấp/khấu trừ, thực hiện tính/khóa lương, xuất phiếu lương (payslips) và xem báo cáo lương.
5. **Employee (Nhân viên)**: Người dùng cuối tiêu chuẩn. Chỉ thao tác trên dữ liệu cá nhân: cập nhật hồ sơ cá nhân/đổi mật khẩu, xem hợp đồng của mình, check-in/check-out chấm công hàng ngày, làm đơn xin nghỉ phép và nhận/xem phiếu lương cá nhân.

---

## 2. Ma Trận Quyền Tổng Thể (Master Permission Matrix)

| Phân hệ / Module | Admin | HR | Manager | PayrollStaff | Employee |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Quản lý Tài khoản (User Accounts)** | **CRUD** | Không | Không | Không | Không |
| **Quản lý Nhân sự (Employees)** | **CRUD** | **CRUD** | Xem (Phòng ban) | Xem (Cần thiết) | Xem (Cá nhân) |
| **Hợp đồng lao động (Contracts)** | **CRUD** | **CRUD** | Không | Xem | Xem (Cá nhân) |
| **Phòng ban & Chức vụ (Depts/Positions)** | **CRUD** | **CRUD** | Xem | Xem | Xem |
| **Ca làm việc (Shifts)** | **CRUD** | **CRUD** | **CRUD** | Xem | Xem |
| **Lịch làm việc (Work Schedules)** | **CRUD** | **CRUD** | **CRUD** | Xem | Xem (Cá nhân) |
| **Chấm công (Attendance Records)** | Xem | Xem | Xem (Phòng ban) | Xem | **Check-in/out / Xem cá nhân** |
| **Đơn nghỉ phép (Leave Requests)** | Xem | Xem | **Duyệt (Phòng ban)** | Không | **Tạo mới / Xem cá nhân** |
| **Phụ cấp & Khấu trừ (Allowance/Deduction)**| **CRUD** | **CRUD** | Không | **CRUD** | Không |
| **Tính & Khóa lương (Payroll Calculation)**| **CRUD** | Không | Không | **CRUD** | Không |
| **Phiếu lương (Payslips)** | Xem | Không | Không | **CRUD** | Xem (Cá nhân) |
| **Báo cáo (Reports)** | Toàn quyền | Nhân sự | Phòng ban | Lương/Công | Không |

*Ghi chú ký hiệu:*
*   **CRUD**: Quyền tạo mới (Create), xem (Read), sửa (Update), xóa (Delete).
*   **Xem (Cá nhân)**: Chỉ được xem dữ liệu của chính mình (kiểm tra claim `employeeId` từ token).
*   **Xem (Phòng ban) / Duyệt (Phòng ban)**: Chỉ thao tác trên nhân viên thuộc phòng ban mà Manager quản lý.

---

## 3. Chi Tiết Logic Phân Quyền Theo Module Nghiệp Vụ

### 3.1. Phân Hệ Nhân Sự (HR Core Service)

#### A. Quản lý Tài khoản (User Accounts)
*   **Admin**:
    *   Tạo mới, kích hoạt, khóa (lockout), hoặc thay đổi vai trò (role) của bất kỳ tài khoản nào.
    *   Đặt lại mật khẩu cho nhân viên.
*   **HR / Manager / PayrollStaff / Employee**: Không được truy cập module quản lý tài khoản.

#### B. Hồ sơ Nhân viên (Employees)
*   **HR & Admin**:
    *   Thêm mới nhân viên, cập nhật thông tin lý lịch, phòng ban, chức vụ, mức lương đóng bảo hiểm.
    *   Xóa mềm nhân viên (`IsDeleted = true`, ẩn khỏi danh sách client nhưng lưu vết DB).
*   **Manager**:
    *   Xem thông tin liên hệ cơ bản của các nhân viên thuộc phòng ban mình phụ trách để tiện trao đổi công việc.
*   **PayrollStaff**:
    *   Xem danh sách mã nhân viên, họ tên, phòng ban và chức vụ để thực hiện đối chiếu lương.
*   **Employee**:
    *   Chỉ xem thông tin hồ sơ của chính mình trên trang cá nhân.
    *   Được phép đổi ảnh đại diện (avatar) và đổi mật khẩu cá nhân.

#### C. Hợp đồng lao động (Contracts)
*   **HR & Admin**:
    *   Tạo mới hợp đồng lao động, tải lên file scan hợp đồng đính kèm.
    *   Sửa thông tin hợp đồng hoặc chấm dứt hợp đồng khi nhân viên nghỉ việc.
*   **PayrollStaff**:
    *   Xem thông tin hợp đồng và mức lương cơ bản để tính toán lương chính xác.
*   **Employee**:
    *   Xem danh sách các hợp đồng của bản thân thông qua trang Hồ sơ cá nhân.
    *   Xem trực tuyến (Online preview) và tải xuống file đính kèm hợp đồng của chính mình.

---

### 3.2. Phân Hệ Chấm Công & Nghỉ Phép (Attendance Service)

#### A. Ca làm việc (Shifts)
*   **Admin, HR, Manager**: Tạo mới ca làm việc (Giờ vào, giờ ra, số công quy đổi), chỉnh sửa hoặc vô hiệu hóa ca làm.
*   **Employee / PayrollStaff**: Xem danh sách ca làm việc để nắm lịch và đối chiếu công.

#### B. Lịch làm việc (Work Schedules)
*   **Admin, HR, Manager**:
    *   Phân lịch làm việc hàng loạt cho nhân viên theo khoảng thời gian (Date range).
    *   Chỉnh sửa ca làm hoặc cập nhật trạng thái lịch làm việc đơn lẻ (Ví dụ: Đánh dấu `Planned` thành `Completed` hoặc `Absent` nếu nhân viên vắng mặt).
    *   Xóa lịch làm việc đã phân.
    *   *Ràng buộc*: Không cho phép chọn hoặc phân lịch cho nhân viên đã nghỉ việc (Status != `Active` hoặc `IsDeleted == true`).
*   **PayrollStaff**: Xem lịch làm việc để đối soát số ngày công tiêu chuẩn.
*   **Employee**: Chỉ được xem lịch làm việc cá nhân của mình để biết lịch đi làm.

#### C. Chấm công hàng ngày (Attendance Records)
*   **Employee**:
    *   Thực hiện Check-in / Check-out trên giao diện dựa trên thời gian thực tế từ server.
    *   Xem lịch sử chấm công cá nhân hàng tháng.
    *   *Ràng buộc*: Vai trò **Admin** không được phép thực hiện check-in/out.
*   **Admin, HR, PayrollStaff**: Xem toàn bộ lịch sử chấm công của tất cả nhân viên.
*   **Manager**: Xem lịch sử chấm công của các nhân viên thuộc phòng ban của mình.

#### D. Đơn nghỉ phép (Leave Requests)
*   **Employee**:
    *   Tạo đơn xin nghỉ phép (Chọn loại nghỉ phép, số ngày nghỉ, lý do).
    *   Xem trạng thái duyệt của đơn nghỉ phép cá nhân.
*   **Manager**:
    *   Nhận thông báo đơn nghỉ phép từ nhân viên trong phòng ban.
    *   Thực hiện Duyệt (Approve) hoặc Từ chối (Reject) đơn nghỉ phép kèm ý kiến phản hồi.
*   **Admin & HR**: Xem toàn bộ đơn nghỉ phép trên toàn hệ thống để giám sát hoạt động.

---

### 3.3. Phân Hệ Lương & Phiếu Lương (Payroll & Report Service)

#### A. Phụ cấp & Khấu trừ (Allowance & Deduction)
*   **Admin, HR, PayrollStaff**: Định nghĩa các danh mục phụ cấp (trợ cấp ăn trưa, đi lại, điện thoại...) và khấu trừ (đóng phạt, bảo hiểm tự nguyện...) và gán mức tiền cụ thể cho từng nhân viên.
*   **Employee**: Không có quyền xem hay can thiệp. Mức phụ cấp/khấu trừ sẽ được hiển thị chi tiết trên phiếu lương hàng tháng.

#### B. Kỳ Lương & Tính Lương (Payroll Periods)
*   **PayrollStaff & Admin**:
    *   Tạo kỳ tính lương mới hàng tháng (Ví dụ: Kỳ lương tháng 06/2026).
    *   Chạy công cụ tự động tính toán lương cho toàn bộ nhân viên dựa trên: Lương cơ bản trong hợp đồng + Số ngày công thực tế (từ Chấm công) + Phụ cấp - Khấu trừ.
    *   Khóa kỳ lương (Lock period) sau khi hoàn tất đối soát. Khi đã khóa, dữ liệu bảng lương sẽ được cố định và không thể tính toán lại.
*   **HR / Manager / Employee**: Không được quyền thao tác.

#### C. Phiếu Lương (Payslips)
*   **PayrollStaff & Admin**: Xem, xuất file Excel và gửi phiếu lương nháp (Draft) hoặc chính thức cho tất cả nhân sự.
*   **Employee**: Nhận thông báo và xem chi tiết phiếu lương cá nhân (bao gồm bảng phân rã lương cơ bản, ngày công làm việc, phụ cấp nhận được, các khoản bảo hiểm/thuế khấu trừ và thực nhận).

---

## 4. Hướng Dẫn Phát Triển (Đảm Bảo Tính Đồng Nhất)

### 4.1. Phía Backend (C# Web API)
Mọi Endpoint nghiệp vụ phải được cấu hình thuộc tính `[Authorize]` đi kèm danh sách roles cụ thể.
*   *Ví dụ cấu hình cho hành động tạo lịch làm việc*:
    ```csharp
    [HttpPost]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult> Create([FromBody] CreateWorkScheduleDto dto) { ... }
    ```
*   *Ví dụ kiểm tra quyền sở hữu dữ liệu cá nhân*:
    ```csharp
    var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
    var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
    if (!isPrivileged)
    {
        var employeeIdClaim = User.FindFirst("employeeId")?.Value;
        if (employeeIdClaim != requestEmployeeId) return Forbid();
    }
    ```

### 4.2. Phía Frontend (Vue 3 / TypeScript)
*   Sử dụng store `auth` (`useAuthStore`) để hiển thị hoặc ẩn các nút bấm hành động nhạy cảm:
    ```html
    <AppButton v-if="auth.isManager" @click="openCreateModal">Phân lịch</AppButton>
    ```
*   Sử dụng Router Guards trong `router/index.ts` để chặn truy cập trực tiếp từ thanh địa chỉ trình duyệt:
    ```typescript
    {
      path: '/hr/employees',
      component: EmployeeListView,
      meta: { requiresAuth: true, roles: ['Admin', 'HR'] }
    }
    ```
