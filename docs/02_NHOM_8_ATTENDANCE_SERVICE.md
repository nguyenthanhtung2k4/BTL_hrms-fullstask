# Nhóm 8 - Attendance Service

## 1. Vai trò của nhóm 8

Nhóm 8 phụ trách Attendance Service trong hệ thống Quản lý nhân sự và chấm công. Service này xử lý ca làm, lịch làm, check-in/check-out, bảng công, đơn nghỉ phép và duyệt nghỉ. Nhóm 8 phải làm theo khung kiến trúc do nhóm 7 dựng, không tự tạo chuẩn riêng làm khó tích hợp.

Phạm vi chính:

- Shift: quản lý ca làm.
- Work schedule: phân lịch làm.
- Attendance record: check-in/check-out.
- Timesheet: bảng công.
- Leave request: đơn nghỉ phép.
- Leave approval: duyệt/từ chối đơn nghỉ.
- Employee projection: dữ liệu nhân viên tối thiểu nhận từ HR Core.
- Publish event cho Payroll & Report.

## 2. Mục tiêu hoàn thành

- Attendance Service chạy được độc lập, có Swagger/OpenAPI.
- Consume được event nhân sự từ HR Core.
- Nhân viên active mới được chấm công.
- Có check-in/check-out chống trùng.
- Có bảng công theo ngày/tháng.
- Có đơn nghỉ phép và flow duyệt nghỉ.
- Publish event `AttendanceRecorded` và `LeaveApproved` cho nhóm 9.
- UI Attendance gắn vào layout chung của frontend.

## 3. Phân công 3 thành viên

### 3.1. Thành viên 1 - Backend Attendance

Trách nhiệm:

- Thiết kế database Attendance.
- Xây dựng API ca làm, lịch làm, chấm công, bảng công.
- Xây dựng rule chống check-in trùng và tính giờ công cơ bản.
- Tích hợp auth/role theo chuẩn nhóm 7.
- Viết unit test cho rule chấm công.

Task checklist:

- [ ] Tạo project Attendance theo skeleton nhóm 7.
- [ ] Tạo DbContext Attendance.
- [ ] Tạo entity `EmployeeProjection`.
- [ ] Tạo entity `Shift`.
- [ ] Tạo entity `WorkSchedule`.
- [ ] Tạo entity `AttendanceRecord`.
- [ ] Tạo entity `Timesheet`.
- [ ] Tạo entity `AuditLog`.
- [ ] Tạo migration đầu tiên.
- [ ] Tạo seed ca làm demo.
- [ ] Tạo endpoint CRUD `/api/shifts`.
- [ ] Tạo endpoint CRUD `/api/work-schedules`.
- [ ] Tạo endpoint `POST /api/attendance/check-in`.
- [ ] Tạo endpoint `POST /api/attendance/check-out`.
- [ ] Tạo endpoint `GET /api/attendance/me`.
- [ ] Tạo endpoint `GET /api/attendance?employeeId=&from=&to=`.
- [ ] Tạo endpoint `GET /api/timesheets?month=&departmentId=`.
- [ ] Rule: nhân viên inactive không được check-in.
- [ ] Rule: không check-in trùng một ca.
- [ ] Rule: không check-out nếu chưa check-in.
- [ ] Rule: check-out phải sau check-in.
- [ ] Rule: dùng thời gian server làm nguồn chính.
- [ ] Tính giờ công cơ bản theo check-in/check-out.
- [ ] Trả lỗi theo ProblemDetails.
- [ ] Không trả entity trực tiếp ra API.
- [ ] Viết unit test check-in trùng.
- [ ] Viết unit test check-out khi chưa check-in.
- [ ] Viết integration test role Employee/Manager/HR.

Definition of Done:

- [ ] API chấm công chạy qua gateway.
- [ ] Swagger đầy đủ endpoint.
- [ ] Rule chấm công chính có test.
- [ ] Role không đúng bị chặn.

### 3.2. Thành viên 2 - Frontend Attendance

Trách nhiệm:

- Gắn module Attendance vào frontend chung.
- Làm UI check-in/check-out cho nhân viên.
- Làm UI lịch làm, bảng công, đơn nghỉ phép.
- Làm UI duyệt nghỉ cho Manager/HR.
- Đảm bảo loading/error/empty state.

Task checklist:

- [ ] Thêm route `/attendance`.
- [ ] Thêm menu Attendance theo role.
- [ ] Tạo Attendance dashboard cá nhân.
- [ ] Tạo nút check-in/check-out.
- [ ] Hiển thị trạng thái ca hiện tại.
- [ ] Hiển thị lịch sử chấm công cá nhân.
- [ ] Tạo page quản lý ca làm cho HR/Admin.
- [ ] Tạo page quản lý lịch làm cho HR/Admin/Manager.
- [ ] Tạo page bảng công cá nhân.
- [ ] Tạo page bảng công quản lý.
- [ ] Tạo form tạo đơn nghỉ phép.
- [ ] Tạo page danh sách đơn nghỉ cá nhân.
- [ ] Tạo page duyệt nghỉ cho Manager/HR.
- [ ] Thêm filter theo ngày/tháng/phòng ban.
- [ ] Thêm loading state.
- [ ] Thêm empty state.
- [ ] Thêm error state.
- [ ] Hiển thị lỗi validation từ backend.
- [ ] Responsive ở mức cơ bản.

Definition of Done:

- [ ] Employee check-in/check-out được từ UI.
- [ ] Employee tạo đơn nghỉ được từ UI.
- [ ] Manager/HR duyệt nghỉ được từ UI.
- [ ] UI không gọi thẳng service, chỉ gọi gateway.

### 3.3. Thành viên 3 - Bảo mật, test và event integration

Trách nhiệm:

- Consume event từ HR Core.
- Publish event cho Payroll & Report.
- Kiểm tra phân quyền.
- Viết test tích hợp và test thủ công.
- Ghi tài liệu checklist tiến độ.

Task checklist:

- [ ] Consume `EmployeeCreated`.
- [ ] Consume `EmployeeUpdated`.
- [ ] Consume `EmployeeStatusChanged`.
- [ ] Lưu/cập nhật `EmployeeProjection`.
- [ ] Xử lý idempotent theo `eventId`.
- [ ] Publish `AttendanceRecorded` khi check-in/check-out hợp lệ.
- [ ] Publish `LeaveApproved` khi đơn nghỉ được duyệt.
- [ ] Log lỗi consume event.
- [ ] Không publish dữ liệu nhạy cảm không cần thiết.
- [ ] Kiểm tra Employee chỉ xem/sửa dữ liệu của mình.
- [ ] Kiểm tra Manager chỉ xem dữ liệu phạm vi quản lý nếu có dữ liệu manager.
- [ ] Kiểm tra HR/Admin xem được dữ liệu quản lý.
- [ ] Audit log sửa bảng công thủ công nếu có.
- [ ] Audit log duyệt/từ chối đơn nghỉ.
- [ ] Security test gọi API không token.
- [ ] Security test gọi API sai role.
- [ ] Integration test consume employee event.
- [ ] Integration test publish attendance event.
- [ ] Ghi test case thủ công vào docs hoặc PR.

Definition of Done:

- [ ] Employee từ HR đồng bộ sang Attendance.
- [ ] Attendance event sang Payroll đúng contract.
- [ ] Security test cơ bản pass.

## 4. API Attendance cần có

Shifts:

- `GET /api/shifts`
- `GET /api/shifts/{id}`
- `POST /api/shifts`
- `PUT /api/shifts/{id}`
- `DELETE /api/shifts/{id}`

Work schedules:

- `GET /api/work-schedules?employeeId=&from=&to=`
- `GET /api/work-schedules/{id}`
- `POST /api/work-schedules`
- `PUT /api/work-schedules/{id}`
- `DELETE /api/work-schedules/{id}`

Attendance:

- `POST /api/attendance/check-in`
- `POST /api/attendance/check-out`
- `GET /api/attendance/me?from=&to=`
- `GET /api/attendance?employeeId=&departmentId=&from=&to=`

Timesheets:

- `GET /api/timesheets/me?month=`
- `GET /api/timesheets?month=&departmentId=&employeeId=`
- `POST /api/timesheets/recalculate` nếu cần tính lại

Leaves:

- `GET /api/leaves/me`
- `GET /api/leaves?status=&departmentId=&from=&to=`
- `POST /api/leaves`
- `GET /api/leaves/{id}`
- `POST /api/leaves/{id}/approve`
- `POST /api/leaves/{id}/reject`
- `POST /api/leaves/{id}/cancel`

## 5. Event nhóm 8 consume/publish

### 5.1. Consume từ HR Core

`EmployeeCreated`:

- [ ] Tạo `EmployeeProjection`.
- [ ] Lưu `employeeId`, `employeeCode`, `fullName`, `departmentId`, `departmentName`, `positionId`, `positionName`, `status`.

`EmployeeUpdated`:

- [ ] Cập nhật `EmployeeProjection`.
- [ ] Không tạo trùng nếu event lặp lại.

`EmployeeStatusChanged`:

- [ ] Cập nhật trạng thái nhân viên.
- [ ] Chặn check-in nếu trạng thái không phải `Active`.

### 5.2. Publish cho Payroll & Report

`AttendanceRecorded` payload tối thiểu:

```json
{
  "attendanceRecordId": "uuid",
  "employeeId": "uuid",
  "workDate": "2026-06-05",
  "shiftId": "uuid",
  "checkInAt": "2026-06-05T08:00:00Z",
  "checkOutAt": "2026-06-05T17:00:00Z",
  "workedMinutes": 480,
  "status": "Completed"
}
```

Checklist:

- [ ] Publish khi record tạo/cập nhật ảnh hưởng tính công.
- [ ] Có `eventId`, `correlationId`, `occurredAt`.
- [ ] Payroll consume được để tính lương.

`LeaveApproved` payload tối thiểu:

```json
{
  "leaveRequestId": "uuid",
  "employeeId": "uuid",
  "fromDate": "2026-06-10",
  "toDate": "2026-06-11",
  "leaveType": "Annual",
  "paid": true,
  "approvedBy": "uuid"
}
```

Checklist:

- [ ] Chỉ publish khi trạng thái chuyển sang approved.
- [ ] Nếu đơn bị reject thì không publish `LeaveApproved`.
- [ ] Payroll biết ngày nghỉ có lương/không lương.

## 6. Bảo mật Attendance

Quy tắc role:

- `Employee`: check-in/check-out, xem bảng công cá nhân, tạo đơn nghỉ cá nhân.
- `Manager`: xem bảng công nhân viên thuộc phạm vi quản lý, duyệt nghỉ.
- `HR`: quản lý ca/lịch làm, xem bảng công, duyệt nghỉ nếu được phân quyền.
- `Admin`: toàn quyền.
- `PayrollStaff`: chỉ đọc dữ liệu cần cho đối soát nếu có endpoint đọc.

Checklist bảo mật:

- [ ] Tất cả endpoint trừ health check yêu cầu JWT.
- [ ] Endpoint quản lý ca làm chỉ cho Admin/HR.
- [ ] Endpoint phân lịch làm chỉ cho Admin/HR/Manager.
- [ ] Employee không xem bảng công người khác.
- [ ] Employee không tự duyệt đơn nghỉ.
- [ ] Manager không duyệt đơn ngoài phạm vi quản lý nếu có dữ liệu manager.
- [ ] Validate ngày bắt đầu <= ngày kết thúc.
- [ ] Validate check-out sau check-in.
- [ ] Rate limit endpoint check-in/check-out nếu cần.
- [ ] Audit log khi sửa công thủ công.
- [ ] Audit log khi duyệt/từ chối đơn nghỉ.

## 7. UI Attendance sau bảo mật

Thứ tự làm:

1. Gắn route và menu theo role.
2. Màn hình check-in/check-out cá nhân.
3. Lịch sử chấm công cá nhân.
4. Quản lý ca làm.
5. Quản lý lịch làm.
6. Đơn nghỉ phép cá nhân.
7. Duyệt nghỉ.
8. Bảng công quản lý.

Checklist UI:

- [ ] Màn hình check-in hiển thị giờ server hoặc giờ gần đúng.
- [ ] Nút check-in/check-out disabled khi không hợp lệ.
- [ ] Bảng công có filter theo tháng.
- [ ] Đơn nghỉ có trạng thái Pending/Approved/Rejected/Cancelled.
- [ ] Manager/HR có nút approve/reject.
- [ ] Employee không thấy nút approve/reject.
- [ ] Loading/empty/error đầy đủ.

## 8. Test case cần làm

Functional:

- [ ] Employee check-in thành công.
- [ ] Employee không check-in trùng.
- [ ] Employee check-out thành công.
- [ ] Employee không check-out khi chưa check-in.
- [ ] Employee tạo đơn nghỉ.
- [ ] Manager duyệt đơn nghỉ.
- [ ] HR xem bảng công theo tháng.

Security:

- [ ] Không token gọi check-in bị 401.
- [ ] Employee xem bảng công người khác bị 403.
- [ ] Employee duyệt nghỉ bị 403.
- [ ] PayrollStaff sửa chấm công bị 403.

Integration:

- [ ] HR publish `EmployeeCreated`, Attendance tạo projection.
- [ ] HR publish `EmployeeStatusChanged` inactive, Attendance chặn check-in.
- [ ] Attendance publish `AttendanceRecorded`, Payroll nhận được.
- [ ] Attendance publish `LeaveApproved`, Payroll nhận được.

UI smoke:

- [ ] Login Employee, check-in/check-out từ UI.
- [ ] Login Manager, duyệt đơn nghỉ từ UI.
- [ ] Login HR, xem bảng công từ UI.

## 9. Checklist cuối cho nhóm 8

- [ ] Attendance Service chạy độc lập.
- [ ] Attendance Service chạy qua gateway.
- [ ] Consume event HR thành công.
- [ ] Publish event Attendance thành công.
- [ ] Check-in/check-out hoàn chỉnh.
- [ ] Leave request/approval hoàn chỉnh.
- [ ] Timesheet hoàn chỉnh.
- [ ] UI Attendance hoàn chỉnh.
- [ ] Test chính pass.
- [ ] Tài liệu API/event cập nhật.
- [ ] Demo flow Attendance thành công.

