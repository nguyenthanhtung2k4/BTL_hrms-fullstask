# Nhóm 7 - HR Core Service và kiến trúc khung

## 1. Vai trò của nhóm 7

Nhóm 7 là nhóm chủ đạo. Ngoài việc phát triển HR Core Service, nhóm 7 phải dựng khung xương dự án để nhóm 8 và nhóm 9 làm theo. Nhóm 7 chịu trách nhiệm giữ kiến trúc thống nhất, review các thay đổi ảnh hưởng đến contract, auth, gateway, Docker, CI và merge code.

Phạm vi chính:

- HR Core Service.
- Auth, user, role, JWT.
- Employee, department, position, contract, employee status.
- API Gateway.
- Shared API/event contract.
- Docker Compose.
- GitHub workflow.
- Layout frontend chung, login, route guard.
- Hướng dẫn tích hợp cho nhóm 8 và nhóm 9.

## 2. Mục tiêu hoàn thành

- Dựng được skeleton monorepo để cả 3 nhóm clone về chạy cùng một cách.
- Có API Gateway làm điểm vào duy nhất cho frontend.
- Có HR Core Service chạy được, có Swagger/OpenAPI.
- Có login JWT và phân quyền role cơ bản.
- Có CRUD nhân sự/phòng ban/chức vụ/hợp đồng.
- Có event nhân sự để Attendance và Payroll consume.
- Có frontend layout chung để các nhóm gắn module UI vào.
- Có quy trình GitHub rõ ràng để kiểm soát merge.

## 3. Phân công 3 thành viên

### 3.1. Thành viên 1 - Kiến trúc sư tích hợp và DevOps

Trách nhiệm:

- Dựng cấu trúc monorepo.
- Tạo backend solution, API Gateway, Docker Compose.
- Tạo chuẩn thư mục cho frontend/backend/shared/infra/docs.
- Thiết lập RabbitMQ, SQL Server local bằng Docker Compose.
- Thiết lập GitHub branch flow, PR template, CODEOWNERS nếu kịp.
- Thiết lập CI cơ bản: build backend, build frontend, chạy test.
- Review contract/API/event của nhóm 8 và nhóm 9 trước khi merge.
- Là người chịu trách nhiệm cuối cùng cho việc tích hợp demo.

Task checklist:

- [ ] Tạo cấu trúc thư mục `frontend/`, `backend/`, `shared/`, `infra/`, `docs/`.
- [ ] Tạo `backend/HRMS.sln`.
- [ ] Tạo project `backend/gateway`.
- [ ] Tạo project `backend/services/hr-core`.
- [ ] Tạo placeholder project cho `attendance` và `payroll-report` để nhóm khác làm tiếp.
- [ ] Tạo `shared/contracts/events`.
- [ ] Tạo `shared/contracts/api`.
- [ ] Tạo `infra/docker-compose.yml` cho SQL Server và RabbitMQ.
- [ ] Thêm service gateway vào Docker Compose.
- [ ] Thêm HR Core vào Docker Compose.
- [ ] Tạo route gateway đến HR Core.
- [ ] Tạo file `.env.example`.
- [ ] Tạo PR template có checklist contract/security/test/UI.
- [ ] Tạo quy định branch trong README hoặc docs.
- [ ] Tạo GitHub Actions build backend.
- [ ] Tạo GitHub Actions build frontend.
- [ ] Tạo tài liệu hướng dẫn chạy local.

Definition of Done:

- [ ] Clone repo mới có thể chạy hạ tầng bằng Docker Compose.
- [ ] Gateway forward được request đến HR Core.
- [ ] CI build được trên PR.
- [ ] Nhóm 8 và 9 có hướng dẫn rõ để thêm service/module.

### 3.2. Thành viên 2 - Backend HR/Auth

Trách nhiệm:

- Thiết kế database HR Core.
- Xây dựng auth JWT.
- Xây dựng user/role policy.
- Xây dựng CRUD employee, department, position, contract.
- Publish event nhân sự.
- Viết test backend quan trọng.

Task checklist:

- [ ] Tạo DbContext HR Core.
- [ ] Tạo entity `User`.
- [ ] Tạo entity `Role` hoặc enum role.
- [ ] Tạo entity `Employee`.
- [ ] Tạo entity `Department`.
- [ ] Tạo entity `Position`.
- [ ] Tạo entity `Contract`.
- [ ] Tạo entity `AuditLog`.
- [ ] Tạo migration đầu tiên.
- [ ] Tạo seed data role Admin, HR, Manager, Employee, PayrollStaff.
- [ ] Tạo seed user Admin demo.
- [ ] Tạo endpoint `POST /api/auth/login`.
- [ ] Tạo endpoint `GET /api/auth/me`.
- [ ] Tạo JWT service.
- [ ] Tạo password hashing.
- [ ] Tạo authorization policy theo role.
- [ ] Tạo CRUD `/api/departments`.
- [ ] Tạo CRUD `/api/positions`.
- [ ] Tạo CRUD `/api/employees`.
- [ ] Tạo CRUD `/api/contracts`.
- [ ] Thêm paging/search/filter cho danh sách employee.
- [ ] Validate employee code không trùng.
- [ ] Validate email không trùng nếu dùng email đăng nhập.
- [ ] Không trả entity trực tiếp ra API.
- [ ] Trả lỗi validation theo ProblemDetails.
- [ ] Ghi audit log khi tạo/sửa/xóa mềm nhân viên.
- [ ] Publish `EmployeeCreated`.
- [ ] Publish `EmployeeUpdated`.
- [ ] Publish `EmployeeStatusChanged`.
- [ ] Viết unit test cho rule trạng thái nhân viên.
- [ ] Viết integration test login.
- [ ] Viết integration test role HR/Admin cho CRUD nhân sự.

Definition of Done:

- [ ] HR Core chạy được độc lập.
- [ ] Swagger hiển thị đủ endpoint.
- [ ] Login nhận JWT hợp lệ.
- [ ] Role không đúng bị chặn.
- [ ] Event nhân sự publish đúng contract.

### 3.3. Thành viên 3 - Frontend HR và khung UI chung

Trách nhiệm:

- Tạo Vue app và Tailwind.
- Tạo layout chung.
- Tạo login page.
- Tạo route guard theo token/role.
- Tạo module UI HR.
- Viết hướng dẫn để nhóm 8 và nhóm 9 gắn route/menu/module theo cùng pattern.

Task checklist:

- [ ] Tạo Vue 3 + Vite app trong `frontend/`.
- [ ] Cấu hình Tailwind CSS.
- [ ] Cấu hình router.
- [ ] Cấu hình store auth.
- [ ] Tạo API client dùng base URL gateway.
- [ ] Tạo interceptor gắn JWT vào request.
- [ ] Tạo login page.
- [ ] Tạo route guard nếu chưa login.
- [ ] Tạo role-based route guard.
- [ ] Tạo main layout.
- [ ] Tạo sidebar.
- [ ] Tạo topbar/user menu.
- [ ] Tạo component table dùng chung.
- [ ] Tạo component form input/select/date dùng chung.
- [ ] Tạo toast/alert lỗi.
- [ ] Tạo confirm dialog cho thao tác nguy hiểm.
- [ ] Tạo Employee list page.
- [ ] Tạo Employee create/edit form.
- [ ] Tạo Employee detail page/drawer.
- [ ] Tạo Department list/create/edit.
- [ ] Tạo Position list/create/edit.
- [ ] Tạo Contract list/create/edit.
- [ ] Thêm loading state cho các page.
- [ ] Thêm empty state cho các page.
- [ ] Thêm error state cho các page.
- [ ] Viết tài liệu cách thêm module UI mới.

Definition of Done:

- [ ] Login thành công và điều hướng vào dashboard.
- [ ] Menu thay đổi theo role.
- [ ] UI HR gọi API qua gateway.
- [ ] Nhóm 8 và 9 có thể copy pattern để thêm module.

## 4. API HR Core cần có

Auth:

- `POST /api/auth/login`
- `GET /api/auth/me`

Employees:

- `GET /api/employees?page=1&pageSize=20&keyword=&departmentId=&status=`
- `GET /api/employees/{id}`
- `POST /api/employees`
- `PUT /api/employees/{id}`
- `PATCH /api/employees/{id}/status`
- `DELETE /api/employees/{id}` nếu cần xóa mềm

Departments:

- `GET /api/departments`
- `GET /api/departments/{id}`
- `POST /api/departments`
- `PUT /api/departments/{id}`
- `DELETE /api/departments/{id}`

Positions:

- `GET /api/positions`
- `GET /api/positions/{id}`
- `POST /api/positions`
- `PUT /api/positions/{id}`
- `DELETE /api/positions/{id}`

Contracts:

- `GET /api/contracts?employeeId=`
- `GET /api/contracts/{id}`
- `POST /api/contracts`
- `PUT /api/contracts/{id}`
- `PATCH /api/contracts/{id}/terminate`

## 5. Event nhóm 7 phải publish

### 5.1. `EmployeeCreated`

Payload tối thiểu:

```json
{
  "employeeId": "uuid",
  "employeeCode": "NV001",
  "fullName": "Nguyen Van A",
  "email": "a@example.com",
  "departmentId": "uuid",
  "departmentName": "IT",
  "positionId": "uuid",
  "positionName": "Developer",
  "status": "Active"
}
```

Checklist:

- [ ] Publish sau khi transaction tạo employee thành công.
- [ ] Không publish mật khẩu hoặc dữ liệu nhạy cảm không cần thiết.
- [ ] Có `eventId`, `correlationId`, `occurredAt`.

### 5.2. `EmployeeUpdated`

Payload tối thiểu:

```json
{
  "employeeId": "uuid",
  "employeeCode": "NV001",
  "fullName": "Nguyen Van A",
  "email": "a@example.com",
  "departmentId": "uuid",
  "departmentName": "IT",
  "positionId": "uuid",
  "positionName": "Senior Developer",
  "status": "Active"
}
```

Checklist:

- [ ] Publish khi thông tin ảnh hưởng Attendance/Payroll thay đổi.
- [ ] Consumer có thể update projection theo `employeeId`.

### 5.3. `EmployeeStatusChanged`

Payload tối thiểu:

```json
{
  "employeeId": "uuid",
  "oldStatus": "Active",
  "newStatus": "Inactive",
  "reason": "Resigned"
}
```

Checklist:

- [ ] Publish khi nhân viên nghỉ việc/tạm nghỉ/kích hoạt lại.
- [ ] Attendance dùng để chặn chấm công.
- [ ] Payroll dùng để xử lý tính lương cuối kỳ.

## 6. Bảo mật nhóm 7 phải làm trước

- [ ] JWT secret không hard-code trong source.
- [ ] Password được hash.
- [ ] Login sai không trả thông tin quá chi tiết.
- [ ] Endpoint HR yêu cầu role phù hợp.
- [ ] Admin/HR mới được tạo/sửa nhân sự.
- [ ] Employee chỉ xem được hồ sơ cá nhân nếu có endpoint self-profile.
- [ ] CORS chỉ allow frontend origin.
- [ ] Thêm rate limit cho login.
- [ ] Audit log thao tác sửa nhân sự.
- [ ] Không commit `.env` thật.

## 7. UI nhóm 7 phải hoàn thành trước khi nhóm khác mở rộng

Thứ tự:

1. Login.
2. Auth store và route guard.
3. Main layout.
4. Role-based sidebar.
5. Component table/form/toast/confirm.
6. Employee UI.
7. Department UI.
8. Position UI.
9. Contract UI.
10. Tài liệu hướng dẫn thêm menu/route/module.

Checklist UI:

- [ ] Login page có loading/error.
- [ ] Token hết hạn thì tự logout hoặc chuyển về login.
- [ ] Sidebar ẩn menu không đúng quyền.
- [ ] List page có search/filter/paging.
- [ ] Form có validation frontend.
- [ ] Backend validation error hiển thị dễ hiểu.
- [ ] Responsive desktop/mobile ở mức cơ bản.

## 8. Trách nhiệm merge và kiểm soát 3 nhóm

### 8.1. Quy trình tích hợp

- Nhóm 8 và nhóm 9 tạo PR vào `develop`.
- Nhóm 7 kiểm tra contract, gateway route, auth, Docker, CI.
- Nếu PR đổi event/API dùng chung, nhóm 7 yêu cầu cập nhật `shared/contracts`.
- Merge theo thứ tự: backend service trước, gateway route, frontend module, integration test.

Checklist review của nhóm 7:

- [ ] Service chạy local.
- [ ] Swagger truy cập được.
- [ ] Gateway route đúng.
- [ ] API yêu cầu JWT nếu cần.
- [ ] Role policy đúng.
- [ ] Không query database của service khác.
- [ ] Event contract đúng envelope.
- [ ] Docker Compose không phá service khác.
- [ ] Frontend không gọi thẳng service, chỉ gọi gateway.
- [ ] Test hoặc test thủ công đã ghi lại.

### 8.2. Lịch tích hợp đề xuất

- Ngày 1-2: nhóm 7 dựng skeleton, auth, gateway, Docker base.
- Ngày 3-5: nhóm 7 hoàn thành HR CRUD cơ bản; nhóm 8/9 bắt đầu service riêng theo skeleton.
- Ngày 6-7: tích hợp event nhân sự sang Attendance/Payroll.
- Ngày 8-10: Attendance hoàn thành chấm công/nghỉ phép; Payroll hoàn thành tính lương cơ bản.
- Ngày 11-12: nối UI toàn hệ thống.
- Ngày 13: test end-to-end, fix lỗi tích hợp.
- Ngày 14: hoàn thiện báo cáo SAD và demo.

## 9. Checklist cuối cho nhóm 7

- [ ] Khung repo hoàn chỉnh.
- [ ] Gateway hoàn chỉnh.
- [ ] Docker Compose chạy được.
- [ ] Auth/JWT hoàn chỉnh.
- [ ] HR Core CRUD hoàn chỉnh.
- [ ] HR UI hoàn chỉnh.
- [ ] Event nhân sự publish đúng.
- [ ] Nhóm 8 consume được event HR.
- [ ] Nhóm 9 consume được event HR.
- [ ] CI build được.
- [ ] PR template có checklist.
- [ ] Hướng dẫn chạy local rõ ràng.
- [ ] Tích hợp demo end-to-end thành công.

