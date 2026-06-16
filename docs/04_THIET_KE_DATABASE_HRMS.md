# Thiết kế database HRMS

Tài liệu này giải thích file SQL:

```text
infra/sqlserver/init/00_create_hrms_databases.sql
```

Mục tiêu là giúp nhóm hiểu rõ:

- Mỗi service dùng database nào.
- Mỗi database có bảng nào.
- Các bảng liên kết với nhau ra sao.
- Ràng buộc chính là gì.
- Vì sao không tạo foreign key trực tiếp giữa 3 database.
- Thiết kế hiện tại có đúng chuẩn 3NF không.

## 1. Tổng quan 3 database

Hệ thống dùng 3 database riêng trong SQL Server:

| Database | Service sở hữu | Vai trò |
| --- | --- | --- |
| `HRMS_HrCoreDb` | HR Core Service | Nguồn sự thật về nhân sự, tài khoản, role, phòng ban, chức vụ, hợp đồng |
| `HRMS_AttendanceDb` | Attendance Service | Quản lý ca làm, lịch làm, chấm công, nghỉ phép, bảng công |
| `HRMS_PayrollReportDb` | Payroll & Report Service | Quản lý kỳ lương, quy tắc lương, phụ cấp, khấu trừ, phiếu lương, báo cáo |

Nguyên tắc quan trọng:

- Mỗi service chỉ đọc/ghi database của chính nó.
- Foreign key chỉ được tạo trong cùng một database.
- Không tạo FK từ `HRMS_AttendanceDb` hoặc `HRMS_PayrollReportDb` sang `HRMS_HrCoreDb`.
- Liên kết chéo service dùng event RabbitMQ và projection table.

Ví dụ liên kết chéo service:

```text
HR Core: Employees
  |
  | publish EmployeeCreated / EmployeeUpdated
  v
Attendance: EmployeeProjections
Payroll:    EmployeeProjections
```

`EmployeeProjections.EmployeeId` là ID nhận từ HR Core, nhưng không phải foreign key vật lý sang bảng `HRMS_HrCoreDb.dbo.Employees`.

## 2. HR Core DB

Database:

```text
HRMS_HrCoreDb
```

Đây là database gốc về nhân sự. Nếu service khác cần thông tin nhân sự, service đó nhận qua event hoặc API, không query trực tiếp DB này.

### 2.1. Bảng và mục đích

| Bảng | Mục đích | Khóa chính | Ràng buộc chính |
| --- | --- | --- | --- |
| `Departments` | Lưu phòng ban | `Id` | `Code` unique, self-FK `ParentDepartmentId`, FK `ManagerEmployeeId` sang `Employees` |
| `Positions` | Lưu chức vụ | `Id` | `Code` unique |
| `Employees` | Lưu nhân viên thật của hệ thống | `Id` | `EmployeeCode` unique, `Email` unique, FK `DepartmentId`, FK `PositionId`, self-FK `ManagerEmployeeId` |
| `Users` | Lưu tài khoản đăng nhập | `Id` | `Email` unique, FK nullable `EmployeeId`, unique filtered index cho `EmployeeId` |
| `Roles` | Lưu danh mục vai trò | `Id` | `Name` unique |
| `UserRoles` | Bảng nối user-role | `(UserId, RoleId)` | FK `UserId`, FK `RoleId` |
| `Contracts` | Lưu hợp đồng nhân viên | `Id` | `ContractNo` unique, FK `EmployeeId` |
| `EmployeeStatusHistories` | Lưu lịch sử đổi trạng thái nhân viên | `Id` | FK `EmployeeId`, FK `ChangedByUserId` |
| `AuditLogs` | Lưu log thao tác nhạy cảm | `Id` | FK nullable `ActorUserId` |
| `OutboxMessages` | Lưu event chờ publish | `Id` | Không FK, phục vụ outbox pattern |

### 2.2. Liên kết bảng HR Core

```text
Departments 1 ---- N Employees
Positions   1 ---- N Employees
Employees   1 ---- N Contracts
Employees   1 ---- N EmployeeStatusHistories
Employees   0/1 -- 1 Users
Users       N ---- N Roles qua UserRoles
Users       1 ---- N AuditLogs
Departments self relationship qua ParentDepartmentId
Employees   self relationship qua ManagerEmployeeId
```

Chi tiết:

| Quan hệ | Ý nghĩa |
| --- | --- |
| `Employees.DepartmentId -> Departments.Id` | Mỗi nhân viên thuộc một phòng ban |
| `Employees.PositionId -> Positions.Id` | Mỗi nhân viên giữ một chức vụ chính |
| `Employees.ManagerEmployeeId -> Employees.Id` | Nhân viên có thể có quản lý trực tiếp |
| `Departments.ParentDepartmentId -> Departments.Id` | Phòng ban có thể nằm dưới phòng ban khác |
| `Departments.ManagerEmployeeId -> Employees.Id` | Phòng ban có thể có trưởng phòng |
| `Users.EmployeeId -> Employees.Id` | Tài khoản có thể gắn với một nhân viên |
| `UserRoles.UserId -> Users.Id` | Một user có thể có nhiều role |
| `UserRoles.RoleId -> Roles.Id` | Một role có thể gắn cho nhiều user |
| `Contracts.EmployeeId -> Employees.Id` | Một nhân viên có thể có nhiều hợp đồng |
| `EmployeeStatusHistories.EmployeeId -> Employees.Id` | Theo dõi lịch sử đổi trạng thái nhân viên |
| `AuditLogs.ActorUserId -> Users.Id` | Theo dõi user thực hiện thao tác |

### 2.3. Ràng buộc nghiệp vụ HR Core

| Ràng buộc | Lý do |
| --- | --- |
| `Departments.Code` unique | Không được trùng mã phòng ban |
| `Positions.Code` unique | Không được trùng mã chức vụ |
| `Employees.EmployeeCode` unique | Không được trùng mã nhân viên |
| `Employees.Email` unique | Không được trùng email nhân viên |
| `Users.Email` unique | Không được trùng email đăng nhập |
| `Users.EmployeeId` unique khi không null | Một nhân viên chỉ gắn tối đa một tài khoản |
| `Roles.Name` unique | Không trùng tên role |
| `UserRoles` dùng composite PK | Không gán trùng cùng một role cho cùng một user |
| `Contracts.ContractNo` unique | Không trùng số hợp đồng |

## 3. Attendance DB

Database:

```text
HRMS_AttendanceDb
```

Database này không lưu bảng `Employees` thật. Nó lưu `EmployeeProjections`, tức bản sao tối thiểu nhận từ HR Core.

### 3.1. Bảng và mục đích

| Bảng | Mục đích | Khóa chính | Ràng buộc chính |
| --- | --- | --- | --- |
| `DepartmentProjections` | Bản sao phòng ban từ HR Core | `DepartmentId` | `Code` unique |
| `PositionProjections` | Bản sao chức vụ từ HR Core | `PositionId` | `Code` unique |
| `EmployeeProjections` | Bản sao nhân viên từ HR Core | `EmployeeId` | `EmployeeCode` unique, FK nội bộ đến projection phòng ban/chức vụ |
| `Shifts` | Danh mục ca làm | `Id` | `Code` unique |
| `WorkSchedules` | Lịch làm của nhân viên | `Id` | Unique `(EmployeeId, WorkDate, ShiftId)` |
| `AttendanceRecords` | Bản ghi check-in/check-out | `Id` | FK `EmployeeId`, `WorkScheduleId`, `ShiftId` |
| `LeaveTypes` | Danh mục loại nghỉ | `Id` | `Code` unique |
| `LeaveRequests` | Đơn nghỉ phép | `Id` | FK `EmployeeId`, FK `LeaveTypeId`, FK `ApprovedByEmployeeId` |
| `Timesheets` | Bảng công tổng hợp theo tháng | `Id` | Unique `(EmployeeId, Year, Month)` |
| `InboxMessages` | Event đã nhận từ service khác | `Id` | Chống xử lý trùng event |
| `OutboxMessages` | Event chờ publish từ Attendance | `Id` | Phục vụ outbox pattern |
| `AuditLogs` | Log thao tác nhạy cảm | `Id` | FK nullable `ActorEmployeeId` |

### 3.2. Liên kết bảng Attendance

```text
DepartmentProjections 1 ---- N EmployeeProjections
PositionProjections   1 ---- N EmployeeProjections
EmployeeProjections   1 ---- N WorkSchedules
EmployeeProjections   1 ---- N AttendanceRecords
EmployeeProjections   1 ---- N LeaveRequests
EmployeeProjections   1 ---- N Timesheets
Shifts                1 ---- N WorkSchedules
Shifts                1 ---- N AttendanceRecords
LeaveTypes            1 ---- N LeaveRequests
EmployeeProjections   self relationship qua ManagerEmployeeId
```

Chi tiết:

| Quan hệ | Ý nghĩa |
| --- | --- |
| `EmployeeProjections.DepartmentId -> DepartmentProjections.DepartmentId` | Nhân viên projection thuộc phòng ban projection |
| `EmployeeProjections.PositionId -> PositionProjections.PositionId` | Nhân viên projection giữ chức vụ projection |
| `EmployeeProjections.ManagerEmployeeId -> EmployeeProjections.EmployeeId` | Quản lý trực tiếp của nhân viên trong Attendance |
| `WorkSchedules.EmployeeId -> EmployeeProjections.EmployeeId` | Lịch làm thuộc một nhân viên |
| `WorkSchedules.ShiftId -> Shifts.Id` | Lịch làm dùng một ca làm |
| `AttendanceRecords.EmployeeId -> EmployeeProjections.EmployeeId` | Bản ghi chấm công thuộc một nhân viên |
| `AttendanceRecords.WorkScheduleId -> WorkSchedules.Id` | Bản ghi chấm công có thể gắn với một lịch làm |
| `AttendanceRecords.ShiftId -> Shifts.Id` | Bản ghi chấm công thuộc một ca |
| `LeaveRequests.EmployeeId -> EmployeeProjections.EmployeeId` | Đơn nghỉ thuộc một nhân viên |
| `LeaveRequests.LeaveTypeId -> LeaveTypes.Id` | Đơn nghỉ có một loại nghỉ |
| `LeaveRequests.ApprovedByEmployeeId -> EmployeeProjections.EmployeeId` | Người duyệt đơn nghỉ |
| `Timesheets.EmployeeId -> EmployeeProjections.EmployeeId` | Bảng công tháng thuộc một nhân viên |
| `AuditLogs.ActorEmployeeId -> EmployeeProjections.EmployeeId` | Nhân viên thực hiện thao tác |

### 3.3. Ràng buộc nghiệp vụ Attendance

| Ràng buộc | Lý do |
| --- | --- |
| `DepartmentProjections.Code` unique | Không trùng mã phòng ban projection |
| `PositionProjections.Code` unique | Không trùng mã chức vụ projection |
| `EmployeeProjections.EmployeeCode` unique | Không trùng mã nhân viên projection |
| `Shifts.Code` unique | Không trùng mã ca làm |
| `WorkSchedules(EmployeeId, WorkDate, ShiftId)` unique | Một nhân viên không bị phân trùng cùng ca trong cùng ngày |
| `Timesheets(EmployeeId, Year, Month)` unique | Một nhân viên chỉ có một bảng công cho một tháng |
| `InboxMessages.Id` PK | Chống consume trùng event |
| `AttendanceRecords.EmployeeId + WorkDate` có index | Tối ưu tra cứu chấm công theo nhân viên/ngày |
| `LeaveRequests.EmployeeId + Status` có index | Tối ưu lọc đơn nghỉ theo nhân viên/trạng thái |

## 4. Payroll & Report DB

Database:

```text
HRMS_PayrollReportDb
```

Database này nhận dữ liệu từ HR Core và Attendance qua event, rồi lưu thành projection để tính lương và báo cáo.

### 4.1. Bảng và mục đích

| Bảng | Mục đích | Khóa chính | Ràng buộc chính |
| --- | --- | --- | --- |
| `DepartmentProjections` | Bản sao phòng ban từ HR Core | `DepartmentId` | `Code` unique |
| `PositionProjections` | Bản sao chức vụ từ HR Core | `PositionId` | `Code` unique |
| `EmployeeProjections` | Bản sao nhân viên từ HR Core | `EmployeeId` | `EmployeeCode` unique, FK nội bộ đến projection phòng ban/chức vụ |
| `EmployeeSalaryProjections` | Bản sao lương cơ bản từ hợp đồng HR | `Id` | FK `EmployeeId`, index theo hiệu lực |
| `AttendanceProjections` | Bản sao dữ liệu công từ Attendance | `AttendanceRecordId` | FK `EmployeeId` |
| `LeaveProjections` | Bản sao dữ liệu nghỉ phép từ Attendance | `LeaveRequestId` | FK `EmployeeId` |
| `PayrollRules` | Quy tắc tính lương | `Id` | `Code` unique |
| `PayrollPeriods` | Kỳ lương | `Id` | `Code` unique, FK `PayrollRuleId` |
| `AllowanceTypes` | Danh mục loại phụ cấp | `Id` | `Code` unique |
| `DeductionTypes` | Danh mục loại khấu trừ | `Id` | `Code` unique |
| `EmployeeAllowances` | Phụ cấp theo nhân viên/kỳ lương | `Id` | FK employee, period, allowance type |
| `EmployeeDeductions` | Khấu trừ theo nhân viên/kỳ lương | `Id` | FK employee, period, deduction type |
| `Payslips` | Phiếu lương | `Id` | Unique `(PayrollPeriodId, EmployeeId)` |
| `PayslipItems` | Dòng chi tiết phiếu lương | `Id` | FK `PayslipId` |
| `InboxMessages` | Event đã nhận từ service khác | `Id` | Chống xử lý trùng event |
| `OutboxMessages` | Event chờ publish từ Payroll | `Id` | Phục vụ outbox pattern |
| `AuditLogs` | Log thao tác nhạy cảm | `Id` | FK nullable `ActorEmployeeId` |

### 4.2. Liên kết bảng Payroll & Report

```text
DepartmentProjections 1 ---- N EmployeeProjections
PositionProjections   1 ---- N EmployeeProjections
EmployeeProjections   1 ---- N EmployeeSalaryProjections
EmployeeProjections   1 ---- N AttendanceProjections
EmployeeProjections   1 ---- N LeaveProjections
EmployeeProjections   1 ---- N EmployeeAllowances
EmployeeProjections   1 ---- N EmployeeDeductions
EmployeeProjections   1 ---- N Payslips
PayrollRules          1 ---- N PayrollPeriods
PayrollPeriods        1 ---- N EmployeeAllowances
PayrollPeriods        1 ---- N EmployeeDeductions
PayrollPeriods        1 ---- N Payslips
AllowanceTypes        1 ---- N EmployeeAllowances
DeductionTypes        1 ---- N EmployeeDeductions
Payslips              1 ---- N PayslipItems
```

Chi tiết:

| Quan hệ | Ý nghĩa |
| --- | --- |
| `EmployeeProjections.DepartmentId -> DepartmentProjections.DepartmentId` | Nhân viên projection thuộc phòng ban projection |
| `EmployeeProjections.PositionId -> PositionProjections.PositionId` | Nhân viên projection giữ chức vụ projection |
| `EmployeeSalaryProjections.EmployeeId -> EmployeeProjections.EmployeeId` | Lương cơ bản theo hợp đồng của nhân viên |
| `AttendanceProjections.EmployeeId -> EmployeeProjections.EmployeeId` | Dữ liệu công dùng tính lương |
| `LeaveProjections.EmployeeId -> EmployeeProjections.EmployeeId` | Dữ liệu nghỉ phép dùng tính lương |
| `PayrollPeriods.PayrollRuleId -> PayrollRules.Id` | Kỳ lương dùng một bộ quy tắc |
| `EmployeeAllowances.EmployeeId -> EmployeeProjections.EmployeeId` | Phụ cấp thuộc một nhân viên |
| `EmployeeAllowances.PayrollPeriodId -> PayrollPeriods.Id` | Phụ cấp thuộc một kỳ lương |
| `EmployeeAllowances.AllowanceTypeId -> AllowanceTypes.Id` | Phụ cấp có một loại phụ cấp |
| `EmployeeDeductions.EmployeeId -> EmployeeProjections.EmployeeId` | Khấu trừ thuộc một nhân viên |
| `EmployeeDeductions.PayrollPeriodId -> PayrollPeriods.Id` | Khấu trừ thuộc một kỳ lương |
| `EmployeeDeductions.DeductionTypeId -> DeductionTypes.Id` | Khấu trừ có một loại khấu trừ |
| `Payslips.PayrollPeriodId -> PayrollPeriods.Id` | Phiếu lương thuộc một kỳ lương |
| `Payslips.EmployeeId -> EmployeeProjections.EmployeeId` | Phiếu lương thuộc một nhân viên |
| `PayslipItems.PayslipId -> Payslips.Id` | Dòng chi tiết thuộc phiếu lương |
| `AuditLogs.ActorEmployeeId -> EmployeeProjections.EmployeeId` | Nhân viên thực hiện thao tác |

### 4.3. Ràng buộc nghiệp vụ Payroll & Report

| Ràng buộc | Lý do |
| --- | --- |
| `DepartmentProjections.Code` unique | Không trùng mã phòng ban projection |
| `PositionProjections.Code` unique | Không trùng mã chức vụ projection |
| `EmployeeProjections.EmployeeCode` unique | Không trùng mã nhân viên projection |
| `PayrollRules.Code` unique | Không trùng mã quy tắc lương |
| `PayrollPeriods.Code` unique | Không trùng mã kỳ lương |
| `AllowanceTypes.Code` unique | Không trùng mã loại phụ cấp |
| `DeductionTypes.Code` unique | Không trùng mã loại khấu trừ |
| `Payslips(PayrollPeriodId, EmployeeId)` unique | Một nhân viên chỉ có một phiếu lương trong một kỳ |
| `AttendanceProjections(EmployeeId, WorkDate)` có index | Tối ưu tính lương theo công trong kỳ |
| `LeaveProjections(EmployeeId, FromDate, ToDate)` có index | Tối ưu tính ngày nghỉ trong kỳ |
| `EmployeeSalaryProjections(EmployeeId, EffectiveFrom, EffectiveTo)` có index | Tìm lương hiệu lực theo kỳ |
| `EmployeeAllowances(PayrollPeriodId, EmployeeId)` có index | Tối ưu lấy phụ cấp theo kỳ/nhân viên |
| `EmployeeDeductions(PayrollPeriodId, EmployeeId)` có index | Tối ưu lấy khấu trừ theo kỳ/nhân viên |

## 5. Vì sao không nối FK giữa 3 database?

Nếu dùng một database truyền thống, có thể tạo:

```text
AttendanceRecords.EmployeeId -> Employees.Id
```

Nhưng với microservices, cách đó không nên dùng vì:

- Attendance Service sẽ phụ thuộc trực tiếp vào database của HR Core.
- Payroll Service sẽ phụ thuộc trực tiếp vào database của Attendance.
- Một nhóm đổi schema có thể làm hỏng nhóm khác.
- Service không còn độc lập triển khai/migration.
- Khó chia ownership cho từng nhóm.

Thiết kế đúng trong dự án này:

```text
HR Core DB
  Employees
      |
      | event EmployeeCreated / EmployeeUpdated
      v
Attendance DB
  EmployeeProjections

Payroll DB
  EmployeeProjections
```

Vì vậy FK chỉ nằm trong từng database:

```text
AttendanceRecords.EmployeeId -> AttendanceDb.EmployeeProjections.EmployeeId
```

Không có:

```text
AttendanceRecords.EmployeeId -> HrCoreDb.Employees.Id
```

## 6. Đánh giá 3NF

### 6.1. Các phần đạt 3NF

Thiết kế đạt 3NF trong phạm vi từng service vì:

- Dữ liệu danh mục được tách riêng.
- Bảng nhân viên không lặp tên phòng ban/chức vụ.
- User-role tách bảng nối `UserRoles`.
- Loại nghỉ tách bảng `LeaveTypes`.
- Loại phụ cấp/khấu trừ tách bảng `AllowanceTypes`, `DeductionTypes`.
- Quy tắc tính lương tách bảng `PayrollRules`.
- Mỗi thuộc tính phụ thuộc vào khóa chính của bảng chứa nó, không phụ thuộc bắc cầu vào thuộc tính không khóa.

Ví dụ đúng 3NF:

```text
Employees.DepartmentId -> Departments.Id
Employees không lưu DepartmentName
```

Không nên làm:

```text
Employees(EmployeeId, FullName, DepartmentId, DepartmentName)
```

Vì `DepartmentName` phụ thuộc vào `DepartmentId`, không phụ thuộc trực tiếp vào `EmployeeId`.

### 6.2. Ngoại lệ có kiểm soát

Một số bảng nhìn giống lặp dữ liệu nhưng là cần thiết trong microservices:

| Bảng | Vì sao chấp nhận |
| --- | --- |
| `EmployeeProjections` | Bản sao tối thiểu để Attendance/Payroll không query HR Core DB |
| `DepartmentProjections` | Bản sao tối thiểu để filter/report theo phòng ban |
| `PositionProjections` | Bản sao tối thiểu để filter/report theo chức vụ |
| `AttendanceProjections` | Bản sao dữ liệu công để Payroll tự tính lương |
| `LeaveProjections` | Bản sao dữ liệu nghỉ để Payroll tự tính lương |
| `Timesheets` | Snapshot/tổng hợp tháng để xem nhanh và audit |
| `Payslips` | Snapshot lương tại thời điểm tính, cần giữ lịch sử |
| `PayslipItems` | Chi tiết lương tại thời điểm tính, cần giữ lịch sử |

Kết luận:

```text
Các bảng nghiệp vụ chính đạt 3NF trong phạm vi từng DB/service.
Projection và snapshot là denormalization có kiểm soát để phục vụ microservices, audit và báo cáo.
```

## 7. Checklist khi nhóm sửa database

Khi thêm/sửa bảng, cần kiểm tra:

- [ ] Bảng thuộc đúng database của service sở hữu.
- [ ] Không tạo FK xuyên database.
- [ ] Không query trực tiếp database service khác.
- [ ] Có PK rõ ràng.
- [ ] Có unique constraint cho mã nghiệp vụ quan trọng.
- [ ] Có FK nội bộ nếu bảng phụ thuộc bảng khác trong cùng DB.
- [ ] Không lặp tên danh mục nếu đã có bảng danh mục riêng.
- [ ] Nếu là dữ liệu nhận từ service khác, đặt tên dạng `...Projections`.
- [ ] Nếu là dữ liệu tổng hợp/lịch sử, ghi rõ đây là snapshot.
- [ ] Nếu thay đổi dữ liệu cần đồng bộ cho service khác, bổ sung event/outbox.

## 8. Lệnh kiểm tra nhanh trên terminal

Kiểm tra 3 database trong Docker SQL Server:

```powershell
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -Q "SELECT name FROM sys.databases WHERE name LIKE 'HRMS_%' ORDER BY name"
```

Xem bảng HR Core:

```powershell
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -d HRMS_HrCoreDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"
```

Xem bảng Attendance:

```powershell
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -d HRMS_AttendanceDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"
```

Xem bảng Payroll & Report:

```powershell
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -d HRMS_PayrollReportDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"
```

Kiểm tra role seed:

```powershell
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -d HRMS_HrCoreDb -Q "SELECT Name FROM dbo.Roles ORDER BY Name"
```

