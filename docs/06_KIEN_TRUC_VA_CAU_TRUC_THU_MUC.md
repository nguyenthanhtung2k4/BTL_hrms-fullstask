# Kiến trúc Hệ thống & Cách Tổ chức Thư mục Chi tiết
## Đề tài số 3: Hệ thống Quản lý Nhân sự & Chấm công (HRMS)

Tài liệu này cung cấp sơ đồ kiến trúc chi tiết (mô tả luồng đi của request, event, port, database) và quy chuẩn tổ chức thư mục (folder structure) cho cả phần Backend (ASP.NET Core) và Frontend (Vue 3).

---

## 1. Sơ đồ Kiến trúc Chi tiết (Detailed System Architecture)

Dưới đây là sơ đồ kiến trúc mô tả luồng đi của dữ liệu từ Frontend, qua API Gateway đến các Service xử lý nghiệp vụ, sự tương tác với Database riêng và cơ chế đồng bộ qua RabbitMQ:

```mermaid
flowchart TB
    %% Định nghĩa các Client & Entry Point
    subgraph ClientLayer ["1. Tầng Giao diện (Frontend)"]
        VueApp["Vue 3 SPA (Port 5173)\n[Router, Pinia, Axios]"]
    end

    subgraph GatewayLayer ["2. Tầng API Gateway"]
        Yarp["YARP API Gateway (Port 5000)\n- Lắng nghe mọi request từ Vue\n- Định tuyến request theo URL\n- Validate JWT Token tập trung"]
    end

    %% Tầng Backend Services
    subgraph ServiceLayer ["3. Tầng Nghiệp vụ (Backend Microservices)"]
        %% Service 7
        subgraph HRService ["HR Core Service (Port 5001)"]
            HR_API["Controllers / Minimal APIs"]
            HR_BL["Nghiệp vụ: Auth, Users, Roles,\nEmployees, Contracts"]
            HR_Bus["MassTransit Publisher"]
        end

        %% Service 8
        subgraph ATService ["Attendance Service (Port 5002)"]
            AT_API["Controllers / Minimal APIs"]
            AT_BL["Nghiệp vụ: Shifts, WorkSchedules,\nCheck-in/out, Timesheets, Leaves"]
            AT_Bus["MassTransit Publisher / Consumer"]
        end

        %% Service 9
        subgraph PRService ["Payroll & Report Service (Port 5003)"]
            PR_API["Controllers / Minimal APIs"]
            PR_BL["Nghiệp vụ: PayrollRules,\nPayrollPeriods, Payslips, Reports"]
            PR_Bus["MassTransit Consumer"]
        end
    end

    %% Tầng Message Broker
    subgraph BrokerLayer ["4. Tầng Event Bus (Message Broker)"]
        RMQ["RabbitMQ Broker (Port 5672 / Management: 15672)"]
        ExChange["Direct/Fanout Exchange\n'Hrms.Contracts.Events'"]
        
        %% Hàng đợi nhận tin nhắn
        Q_AT["Queue: 'attendance-employee-created'"]
        Q_PR_Emp["Queue: 'payroll-employee-created'"]
        Q_PR_Att["Queue: 'payroll-attendance-recorded'"]
    end

    %% Tầng Database
    subgraph DbLayer ["5. Tầng Dữ liệu (Databases)"]
        HR_DB[("SQL Server\nDB: HRMS_HrCoreDb")]
        AT_DB[("SQL Server\nDB: HRMS_AttendanceDb")]
        PR_DB[("SQL Server\nDB: HRMS_PayrollReportDb")]
    end

    %% Mối liên kết luồng gọi API từ Frontend
    VueApp -->|Gọi API duy nhất: http://localhost:5000| Yarp
    
    Yarp -->|Route: /api/hr/{**catch-all}| HR_API
    Yarp -->|Route: /api/attendance/{**catch-all}| AT_API
    Yarp -->|Route: /api/payroll/{**catch-all}| PR_API

    %% Kết nối Database nội bộ của từng service
    HR_BL <-->|EF Core| HR_DB
    AT_BL <-->|EF Core| AT_DB
    PR_BL <-->|EF Core| PR_DB

    %% Giao tiếp Event-driven bất đồng bộ
    HR_Bus -->|1. Publish: EmployeeCreated| ExChange
    ExChange -->|Route| Q_AT
    ExChange -->|Route| Q_PR_Emp
    
    Q_AT -->|2. Consume & Lưu Projection| AT_Bus
    Q_PR_Emp -->|2. Consume & Lưu Projection| PR_Bus

    AT_Bus -->|3. Publish: AttendanceRecorded / LeaveApproved| ExChange
    ExChange -->|Route| Q_PR_Att
    Q_PR_Att -->|4. Consume & Lưu Projection| PR_Bus

    %% Giao tiếp đồng bộ trực tiếp khi cần thiết (HttpClient)
    PR_BL -.->|HttpClient GET /api/hr/employees/{id}| Yarp
    PR_BL -.->|HttpClient GET /api/attendance/timesheets/{id}| Yarp

    %% Style chỉnh sửa đồ họa
    classDef client fill:#e1f5fe,stroke:#01579b,stroke-width:2px;
    classDef gateway fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px;
    classDef service fill:#fff8e1,stroke:#ff8f00,stroke-width:2px;
    classDef broker fill:#f3e5f5,stroke:#6a1b9a,stroke-width:2px;
    classDef db fill:#eceff1,stroke:#37474f,stroke-width:2px;

    class VueApp client;
    class Yarp gateway;
    class HRService,ATService,PRService service;
    class RMQ,ExChange,Q_AT,Q_PR_Emp,Q_PR_Att broker;
    class HR_DB,AT_DB,PR_DB db;
```

---

## 2. Cách Tổ chức Thư mục Chi tiết (Folder Structure)

Dưới đây là cấu trúc thư mục đề xuất cho toàn bộ Monorepo. Cấu trúc này được tối ưu hóa để các nhóm có thể làm việc song song mà không sợ bị xung đột tệp tin (conflict) khi merge code.

```text
BTL_FULL_STASK/
│
├── .github/                           # Cấu hình GitHub Actions
│   └── workflows/
│       ├── backend-ci.yml             # Tự động build và test backend
│       └── frontend-ci.yml            # Tự động build và linter frontend
│
├── docs/                              # Tài liệu phân tích và hướng dẫn
│   ├── 00_PHAN_TICH_TONG_THE_HRMS.md
│   ├── 01_NHOM_7_HR_CORE_KIEN_TRUC.md
│   ├── 02_NHOM_8_ATTENDANCE_SERVICE.md
│   ├── 03_NHOM_9_PAYROLL_REPORT_SERVICE.md
│   ├── 04_THIET_KE_DATABASE_HRMS.md
│   └── 06_KIEN_TRUC_VA_CAU_TRUC_THU_MUC.md
│
├── shared/                            # Thư viện dùng chung
│   └── contracts/                     # Các hợp đồng API & Event dùng chung
│       └── Hrms.Contracts/
│           ├── Hrms.Contracts.csproj
│           ├── Api/
│           │   └── ServiceInfoResponse.cs
│           └── Events/
│               ├── EventNames.cs       # Chứa các string hằng số tên Event
│               ├── IntegrationEvent.cs # Struct bọc Event (Envelope)
│               ├── EmployeeEvents.cs   # Bản tin Event nhân sự
│               ├── AttendanceEvents.cs # Bản tin Event chấm công
│               └── PayrollEvents.cs    # Bản tin Event tính lương
│
├── infra/                             # Cơ sở hạ tầng local
│   ├── docker-compose.yml             # Khởi chạy SQL Server & RabbitMQ
│   └── sqlserver/
│       └── init/
│           └── 00_create_hrms_databases.sql  # Script tạo DB
│
├── backend/                           # Phần Backend (ASP.NET Core 8.0)
│   ├── HRMS.sln                       # Solution lớn chứa tất cả project
│   │
│   ├── gateway/                       # API Gateway (YARP)
│   │   ├── Hrms.Gateway.csproj
│   │   ├── Program.cs                 # Cấu hình Routing, CORS, HTTPS
│   │   ├── appsettings.json           # File cấu hình YARP mapping routes
│   │   └── Properties/
│   │
│   └── services/                      # Danh sách các microservices con
│       │
      ├── hr-core/                   # Nhóm 7: HR Core Microservice (Single-Project Clean Architecture)
      │   ├── Hrms.HrCore.Api.csproj
      │   ├── Program.cs             # Cấu hình DI (Dependency Injection) kết nối các thư mục/tầng
      │   │
      │   ├── Domain/                # TẦNG DOMAIN: Thực thể & Quy tắc nghiệp vụ cốt lõi
      │   │   └── Entities/          # Các thực thể database (User.cs, Employee.cs, Department.cs, Position.cs)
      │   │
      │   ├── Application/           # TẦNG APPLICATION: Logic nghiệp vụ, DTOs & Interfaces
      │   │   ├── Dtos/              # Data Transfer Objects (LoginRequest.cs, EmployeeDto.cs)
      │   │   ├── Interfaces/        # Định nghĩa các giao diện (IAuthService.cs, IEmployeeService.cs)
      │   │   └── Services/          # Triển khai lớp Service cụ thể (AuthService.cs, EmployeeService.cs)
      │   │
      │   ├── Infrastructure/        # TẦNG INFRASTRUCTURE: Database Access & External Messaging
      │   │   ├── Persistence/       # Cấu hình EF Core DbContext (HrDbContext.cs) & Thư mục Migrations/
      │   │   └── Messaging/         # Cấu hình Publish Event qua RabbitMQ (EmployeePublisher.cs)
      │   │
      │   └── Controllers/           # TẦNG PRESENTATION: Đón nhận API từ API Gateway
      │       ├── AuthController.cs
      │       └── EmployeesController.cs
      │
      ├── attendance/                # Nhóm 8: Attendance Microservice (Single-Project Clean Architecture)
      │   ├── Hrms.Attendance.Api.csproj
      │   ├── Program.cs
      │   │
      │   ├── Domain/
      │   │   └── Entities/          # Shift.cs, WorkSchedule.cs, LeaveRequest.cs, EmployeeProjection.cs
      │   │
      │   ├── Application/
      │   │   ├── Dtos/
      │   │   ├── Interfaces/        # IScheduleService.cs, ILeaveService.cs
      │   │   └── Services/
      │   │
      │   ├── Infrastructure/
      │   │   ├── Persistence/       # DbContext (AttendanceDbContext.cs) & Migrations/
      │   │   └── Messaging/
      │   │       └── Consumers/     # Consumer lắng nghe đồng bộ từ RabbitMQ (EmployeeCreatedConsumer.cs)
      │   │
      │   └── Controllers/           # CheckinController.cs, LeaveController.cs
      │
      └── payroll-report/            # Nhóm 9: Payroll & Report Microservice (Single-Project Clean Architecture)
          ├── Hrms.PayrollReport.Api.csproj
          ├── Program.cs
          │
          ├── Domain/
          │   └── Entities/          # PayrollPeriod.cs, Payslip.cs, EmployeeProjection.cs, AttendanceProjection.cs
          │
          ├── Application/
          │   ├── Dtos/
          │   ├── Interfaces/        # IPayrollService.cs, IReportService.cs
          │   └── Services/
          │
          ├── Infrastructure/
          │   ├── Persistence/       # DbContext (PayrollDbContext.cs) & Migrations/
          │   └── Messaging/
          │       └── Consumers/     # Consumer nhận tin (EmployeeCreatedConsumer.cs, AttendanceRecordedConsumer.cs)
          │
          └── Controllers/           # PayrollController.cs, ReportsController.cs


│
└── frontend/                          # Phần Frontend (Vue 3 + Vite + TS)
    ├── package.json
    ├── vite.config.ts
    ├── index.html
    │
    └── src/
        ├── main.ts                    # Điểm khởi chạy Vue App
        ├── App.vue                    # Component gốc
        ├── style.css                  # Chứa cấu hình Tailwind & custom CSS
        │
        ├── assets/                    # Chứa ảnh, icons, font
        ├── components/                # Các UI Component dùng chung (Table, Modal, Button)
        ├── layouts/                   # Các Layout (MainLayout, AuthLayout)
        │   └── MainLayout.vue         # Layout có Sidebar và Topbar
        │
        ├── router/                    # Cấu hình chuyển trang & Route Guard
        │   └── index.ts
        │
        ├── stores/                    # Quản lý State tập trung (Pinia)
        │   └── auth.ts                # Lưu trữ thông tin đăng nhập và JWT Token
        │
        ├── services/                  # Các file gọi API tập trung
        │   └── apiClient.ts           # Axios Client cấu hình base URL là Gateway (Port 5000)
        │
        └── modules/                   # Các Module nghiệp vụ riêng của từng nhóm
            ├── auth/                  # Nhóm 7: Màn hình Đăng nhập
            │   └── LoginView.vue
            ├── hr/                    # Nhóm 7: Quản lý nhân viên, phòng ban
            │   ├── EmployeeList.vue
            │   └── DepartmentList.vue
            ├── attendance/            # Nhóm 8: Check-in/out, Đơn nghỉ
            │   ├── AttendanceCheck.vue
            │   └── LeaveRequestList.vue
            └── payroll/               # Nhóm 9: Tính lương, Phiếu lương
                ├── PayrollPeriodList.vue
                └── PayslipDetail.vue
```

---

## 3. Quy chuẩn Thêm mới Tính năng (Workflow)

Để đảm bảo hệ thống không bị lỗi cấu trúc khi các thành viên code chung, hãy tuân thủ quy tắc sau:

1.  **Khi thêm bảng mới:** 
    *   Phải xác định bảng thuộc Database nào trong 3 DB.
    *   Tạo Entity tương ứng trong thư mục `Infrastructure/Data` của Service đó.
    *   Chạy lệnh Add-Migration và Update-Database riêng cho Service đó.
2.  **Khi tạo API mới:**
    *   Viết logic Endpoint trong thư mục `Features/<Tên-Chức-Năng>` dưới dạng Minimal API hoặc Controller.
    *   Khai báo API route dạng: `/api/hr/employees`, `/api/attendance/check-in`, `/api/payroll/calculate`.
3.  **Khi cần truyền dữ liệu giữa các Service:**
    *   **Bất đồng bộ:** Khai báo cấu trúc Event trong dự án `shared/contracts`. Tạo class `Consumer` tại Service nhận để lưu thông tin projection.
    *   **Đồng bộ:** Dùng `HttpClient` từ Service gửi yêu cầu HTTP GET thông qua cổng Gateway (Port 5000) để lấy dữ liệu.
