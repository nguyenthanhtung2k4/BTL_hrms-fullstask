# Nhóm 9 - Payroll & Report Service

## 1. Vai trò của nhóm 9

Nhóm 9 phụ trách Payroll & Report Service. Service này nhận dữ liệu nhân sự từ HR Core, nhận dữ liệu công/nghỉ phép từ Attendance, sau đó tính lương, tạo phiếu lương và cung cấp báo cáo tổng hợp.

Phạm vi chính:

- Payroll period: kỳ lương.
- Payroll rule: quy tắc tính lương.
- Allowance/deduction: phụ cấp và khấu trừ.
- Payslip: phiếu lương.
- Report: báo cáo nhân sự, chấm công, lương.
- Employee projection: dữ liệu nhân viên tối thiểu từ HR Core.
- Attendance projection: dữ liệu công/nghỉ phép từ Attendance.
- Export Excel/PDF nếu còn thời gian.

## 2. Mục tiêu hoàn thành

- Payroll & Report Service chạy được độc lập, có Swagger/OpenAPI.
- Consume được event nhân sự từ HR Core.
- Consume được event chấm công/nghỉ phép từ Attendance.
- Tạo và quản lý kỳ lương.
- Tính lương cơ bản theo ngày/giờ công.
- Thêm phụ cấp/khấu trừ.
- Khóa kỳ lương sau khi chốt.
- Xem phiếu lương cá nhân.
- Có báo cáo nhân sự/công/lương cơ bản.
- UI Payroll/Report gắn vào layout chung của frontend.

## 3. Phân công 3 thành viên

### 3.1. Thành viên 1 - Backend Payroll

Trách nhiệm:

- Thiết kế database Payroll & Report.
- Xây dựng API kỳ lương, quy tắc lương, phụ cấp, khấu trừ, phiếu lương.
- Xây dựng logic tính lương cơ bản.
- Xây dựng logic khóa kỳ lương.
- Viết test rule tính lương.

Task checklist:

- [x] Tạo project Payroll & Report theo skeleton nhóm 7.
- [ ] Tạo DbContext Payroll.
- [ ] Tạo entity `EmployeeProjection`.
- [ ] Tạo entity `AttendanceProjection`.
- [ ] Tạo entity `LeaveProjection`.
- [ ] Tạo entity `PayrollPeriod`.
- [ ] Tạo entity `PayrollRule`.
- [ ] Tạo entity `Allowance`.
- [ ] Tạo entity `Deduction`.
- [ ] Tạo entity `Payslip`.
- [ ] Tạo entity `AuditLog`.
- [ ] Tạo migration đầu tiên.
- [ ] Tạo seed payroll rule demo.
- [ ] Tạo CRUD `/api/payroll-periods`.
- [ ] Tạo CRUD `/api/payroll-rules`.
- [ ] Tạo CRUD `/api/allowances`.
- [ ] Tạo CRUD `/api/deductions`.
- [ ] Tạo endpoint `POST /api/payroll-periods/{id}/calculate`.
- [ ] Tạo endpoint `POST /api/payroll-periods/{id}/close`.
- [ ] Tạo endpoint `GET /api/payslips`.
- [ ] Tạo endpoint `GET /api/payslips/{id}`.
- [ ] Tạo endpoint `GET /api/payslips/me`.
- [ ] Rule: không tính lương cho kỳ đã khóa.
- [ ] Rule: không sửa phiếu lương khi kỳ đã khóa.
- [ ] Rule: nhân viên inactive vẫn có thể tính lương nếu có công trong kỳ trước khi nghỉ.
- [ ] Rule: tổng lương = lương cơ bản theo công + phụ cấp - khấu trừ.
- [ ] Ghi audit log khi tính lương.
- [ ] Ghi audit log khi khóa kỳ lương.
- [ ] Trả lỗi theo ProblemDetails.
- [ ] Không trả entity trực tiếp ra API.
- [ ] Viết unit test tính lương cơ bản.
- [ ] Viết unit test phụ cấp/khấu trừ.
- [ ] Viết integration test role PayrollStaff/Admin.

Definition of Done:

- [ ] Payroll API chạy qua gateway.
- [ ] Tính được lương từ projection.
- [ ] Kỳ lương khóa thì không sửa được.
- [ ] Swagger đầy đủ endpoint.

### 3.2. Thành viên 2 - Frontend Payroll và Report

Trách nhiệm:

- Gắn module Payroll/Report vào frontend chung.
- Làm UI kỳ lương, quy tắc lương, phiếu lương.
- Làm dashboard/report cơ bản.
- Làm export Excel/PDF nếu kịp.
- Đảm bảo role-based UI.

Task checklist:

- [ ] Thêm route `/payroll`.
- [ ] Thêm route `/reports`.
- [ ] Thêm menu Payroll theo role PayrollStaff/Admin.
- [ ] Thêm menu Reports theo role Admin/HR/PayrollStaff.
- [ ] Tạo page danh sách kỳ lương.
- [ ] Tạo form tạo/sửa kỳ lương.
- [ ] Tạo nút tính lương.
- [ ] Tạo nút khóa kỳ lương.
- [ ] Tạo page danh sách phiếu lương.
- [ ] Tạo page chi tiết phiếu lương.
- [ ] Tạo page phiếu lương cá nhân cho Employee.
- [ ] Tạo page quản lý quy tắc lương.
- [ ] Tạo page phụ cấp/khấu trừ.
- [ ] Tạo dashboard báo cáo tổng quan.
- [ ] Tạo báo cáo nhân sự theo phòng ban/trạng thái.
- [ ] Tạo báo cáo công theo tháng.
- [ ] Tạo báo cáo lương theo kỳ.
- [ ] Thêm filter theo tháng/kỳ/phòng ban.
- [ ] Thêm loading state.
- [ ] Thêm empty state.
- [ ] Thêm error state.
- [ ] Hiển thị lỗi validation từ backend.
- [ ] Thêm export Excel/PDF nếu còn thời gian.

Definition of Done:

- [ ] PayrollStaff tính lương được từ UI.
- [ ] PayrollStaff khóa kỳ lương được từ UI.
- [ ] Employee xem phiếu lương cá nhân được.
- [ ] Báo cáo cơ bản hiển thị được.

### 3.3. Thành viên 3 - Bảo mật, test và event integration

Trách nhiệm:

- Consume event từ HR Core và Attendance.
- Kiểm tra dữ liệu projection trước khi tính lương.
- Kiểm tra phân quyền.
- Viết integration test và test thủ công.
- Đảm bảo báo cáo không lộ dữ liệu sai quyền.

Task checklist:

- [ ] Consume `EmployeeCreated`.
- [ ] Consume `EmployeeUpdated`.
- [ ] Consume `EmployeeStatusChanged`.
- [ ] Consume `AttendanceRecorded`.
- [ ] Consume `LeaveApproved`.
- [ ] Lưu/cập nhật `EmployeeProjection`.
- [ ] Lưu/cập nhật `AttendanceProjection`.
- [ ] Lưu/cập nhật `LeaveProjection`.
- [ ] Xử lý idempotent theo `eventId`.
- [ ] Log lỗi consume event.
- [ ] Đối soát thiếu dữ liệu employee trước khi tính lương.
- [ ] Đối soát thiếu dữ liệu attendance trước khi tính lương.
- [ ] Publish `PayrollClosed` khi khóa kỳ lương nếu cần.
- [ ] Kiểm tra PayrollStaff/Admin mới được tính lương.
- [ ] Kiểm tra Employee chỉ xem phiếu lương cá nhân.
- [ ] Kiểm tra HR không xem chi tiết lương nếu không được phân quyền.
- [ ] Security test gọi API không token.
- [ ] Security test gọi API sai role.
- [ ] Integration test consume HR event.
- [ ] Integration test consume Attendance event.
- [ ] Integration test calculate payroll.
- [ ] Ghi test case thủ công vào docs hoặc PR.

Definition of Done:

- [ ] Payroll nhận đủ dữ liệu từ HR và Attendance.
- [ ] Tính lương không cần query database service khác.
- [ ] Security test cơ bản pass.

## 4. API Payroll & Report cần có

Payroll periods:

- `GET /api/payroll-periods`
- `GET /api/payroll-periods/{id}`
- `POST /api/payroll-periods`
- `PUT /api/payroll-periods/{id}`
- `POST /api/payroll-periods/{id}/calculate`
- `POST /api/payroll-periods/{id}/close`

Payroll rules:

- `GET /api/payroll-rules`
- `GET /api/payroll-rules/{id}`
- `POST /api/payroll-rules`
- `PUT /api/payroll-rules/{id}`
- `DELETE /api/payroll-rules/{id}`

Allowances and deductions:

- `GET /api/allowances?employeeId=&periodId=`
- `POST /api/allowances`
- `PUT /api/allowances/{id}`
- `DELETE /api/allowances/{id}`
- `GET /api/deductions?employeeId=&periodId=`
- `POST /api/deductions`
- `PUT /api/deductions/{id}`
- `DELETE /api/deductions/{id}`

Payslips:

- `GET /api/payslips?periodId=&employeeId=&departmentId=`
- `GET /api/payslips/{id}`
- `GET /api/payslips/me?periodId=`

Reports:

- `GET /api/reports/employees?departmentId=&status=`
- `GET /api/reports/attendance?month=&departmentId=`
- `GET /api/reports/payroll?periodId=&departmentId=`
- `GET /api/reports/dashboard`
- `GET /api/reports/payroll/export?periodId=` nếu làm export

## 5. Event nhóm 9 consume/publish

### 5.1. Consume từ HR Core

`EmployeeCreated`:

- [ ] Tạo `EmployeeProjection`.
- [ ] Lưu thông tin tối thiểu phục vụ tính lương và báo cáo.

`EmployeeUpdated`:

- [ ] Cập nhật phòng ban/chức vụ/tên/email/trạng thái.
- [ ] Không ghi đè dữ liệu payroll đã chốt.

`EmployeeStatusChanged`:

- [ ] Cập nhật trạng thái nhân viên.
- [ ] Vẫn giữ dữ liệu lịch sử để báo cáo và tính lương kỳ cũ.

### 5.2. Consume từ Attendance

`AttendanceRecorded`:

- [ ] Tạo/cập nhật `AttendanceProjection`.
- [ ] Cập nhật `workedMinutes`, `workDate`, `status`.

`LeaveApproved`:

- [ ] Tạo/cập nhật `LeaveProjection`.
- [ ] Phân biệt nghỉ có lương và nghỉ không lương.

### 5.3. Publish nếu cần

`PayrollClosed` payload tối thiểu:

```json
{
  "payrollPeriodId": "uuid",
  "periodName": "Luong thang 06/2026",
  "fromDate": "2026-06-01",
  "toDate": "2026-06-30",
  "closedAt": "2026-07-01T10:00:00Z",
  "closedBy": "uuid"
}
```

Checklist:

- [ ] Publish sau khi kỳ lương khóa thành công.
- [ ] Không publish chi tiết lương cá nhân nếu consumer không cần.

## 6. Quy tắc tính lương v1

Để bài tập không quá rộng, v1 dùng công thức đơn giản:

```text
workedHours = totalWorkedMinutes / 60
baseSalaryByWork = monthlyBaseSalary * workedDays / standardWorkDays
grossSalary = baseSalaryByWork + totalAllowance
netSalary = grossSalary - totalDeduction
```

Quy tắc:

- `standardWorkDays` mặc định lấy từ payroll rule của kỳ lương.
- Nghỉ có lương có thể tính như ngày công nếu rule bật.
- Nghỉ không lương bị trừ theo ngày.
- Đi muộn/về sớm là nâng cao, chỉ làm nếu còn thời gian.
- Thuế, bảo hiểm là nâng cao, có thể ghi hướng phát triển nếu không kịp.

Checklist:

- [ ] Có payroll rule chứa `standardWorkDays`.
- [ ] Có base salary cho nhân viên hoặc contract.
- [ ] Có tổng công từ AttendanceProjection.
- [ ] Có tổng phụ cấp.
- [ ] Có tổng khấu trừ.
- [ ] Tính ra gross/net salary.
- [ ] Lưu payslip draft.
- [ ] Không sửa payslip sau khi period closed.

## 7. Bảo mật Payroll & Report

Quy tắc role:

- `PayrollStaff`: quản lý kỳ lương, tính lương, xem phiếu lương, xem báo cáo lương.
- `Admin`: toàn quyền.
- `Employee`: chỉ xem phiếu lương cá nhân.
- `HR`: xem báo cáo nhân sự, không mặc định xem chi tiết lương nếu chưa được phân quyền.
- `Manager`: xem báo cáo phạm vi nhóm nếu có yêu cầu, không mặc định xem lương.

Checklist bảo mật:

- [ ] Tất cả endpoint trừ health check yêu cầu JWT.
- [ ] Tính lương chỉ cho PayrollStaff/Admin.
- [ ] Khóa kỳ lương chỉ cho PayrollStaff/Admin.
- [ ] Employee chỉ xem phiếu lương của mình.
- [ ] HR không xem chi tiết lương nếu không được cấp quyền.
- [ ] Validate period date hợp lệ.
- [ ] Không cho tính lương kỳ đã khóa.
- [ ] Không cho sửa payslip kỳ đã khóa.
- [ ] Rate limit endpoint calculate/export nếu cần.
- [ ] Audit log tính lương.
- [ ] Audit log khóa kỳ lương.
- [ ] Không log dữ liệu lương nhạy cảm quá chi tiết.

## 8. UI Payroll/Report sau bảo mật

Thứ tự làm:

1. Gắn route và menu theo role.
2. Màn hình kỳ lương.
3. Màn hình quy tắc lương.
4. Màn hình tính lương.
5. Màn hình phiếu lương.
6. Màn hình phiếu lương cá nhân.
7. Dashboard báo cáo.
8. Export nâng cao nếu còn thời gian.

Checklist UI:

- [ ] Nút tính lương chỉ hiện với PayrollStaff/Admin.
- [ ] Nút khóa kỳ lương có confirm dialog.
- [ ] Kỳ đã khóa hiển thị trạng thái rõ.
- [ ] Phiếu lương có breakdown lương cơ bản, phụ cấp, khấu trừ, thực nhận.
- [ ] Employee không thấy phiếu lương người khác.
- [ ] Report có filter kỳ/tháng/phòng ban.
- [ ] Loading/empty/error đầy đủ.

## 9. Test case cần làm

Functional:

- [ ] Tạo kỳ lương.
- [ ] Tạo quy tắc lương.
- [ ] Tính lương cho kỳ chưa khóa.
- [ ] Khóa kỳ lương.
- [ ] Không tính lại kỳ đã khóa.
- [ ] Employee xem phiếu lương cá nhân.
- [ ] PayrollStaff xem báo cáo lương.

Security:

- [ ] Không token gọi API payroll bị 401.
- [ ] Employee tính lương bị 403.
- [ ] Employee xem phiếu lương người khác bị 403.
- [ ] HR khóa kỳ lương bị 403 nếu không được phân quyền.

Integration:

- [ ] HR publish `EmployeeCreated`, Payroll tạo projection.
- [ ] HR publish `EmployeeStatusChanged`, Payroll cập nhật projection.
- [ ] Attendance publish `AttendanceRecorded`, Payroll cập nhật công.
- [ ] Attendance publish `LeaveApproved`, Payroll cập nhật nghỉ phép.
- [ ] Payroll tính lương từ projection, không query database service khác.

UI smoke:

- [ ] Login PayrollStaff, tạo kỳ lương từ UI.
- [ ] Login PayrollStaff, tính lương từ UI.
- [ ] Login PayrollStaff, khóa kỳ lương từ UI.
- [ ] Login Employee, xem phiếu lương cá nhân từ UI.

## 10. Checklist cuối cho nhóm 9

- [ ] Payroll & Report Service chạy độc lập.
- [ ] Payroll & Report Service chạy qua gateway.
- [ ] Consume event HR thành công.
- [ ] Consume event Attendance thành công.
- [ ] Tính lương cơ bản hoàn chỉnh.
- [ ] Phiếu lương hoàn chỉnh.
- [ ] Báo cáo cơ bản hoàn chỉnh.
- [ ] UI Payroll/Report hoàn chỉnh.
- [ ] Test chính pass.
- [ ] Tài liệu API/event cập nhật.
- [ ] Demo flow Payroll/Report thành công.
