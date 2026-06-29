# BTL HRMS Fullstack

Đề tài số 3: **Quản lý nhân sự và chấm công**.

Dự án dùng kiến trúc microservices trong một monorepo:

- Frontend: Vue 3 + Vite + Tailwind CSS.
- Backend: ASP.NET Core Web API.
- Database: SQL Server.
- Event bus: RabbitMQ.
- Môi trường chạy chung: Docker Compose.

Repo hiện tại đang ở giai đoạn thiết kế kiến trúc và database. Source frontend/backend sẽ được tạo sau theo checklist trong `docs/`.

## 1. Cấu trúc dự án dự kiến

```text
BTL_FULL_STASK/
  frontend/                         # Vue app
  backend/
    gateway/                        # API Gateway
    services/
      hr-core/                      # Nhóm 7
      attendance/                   # Nhóm 8
      payroll-report/               # Nhóm 9
  shared/
    contracts/                      # API/event contracts
  infra/
    docker-compose.yml              # SQL Server + RabbitMQ
    sqlserver/init/                 # SQL scripts tạo DB
  docs/                             # Tài liệu phân tích và task nhóm
```

Một repo không có nghĩa là một service duy nhất. Repo này là **monorepo**: tất cả nhóm làm chung một repo để dễ merge, nhưng backend vẫn tách thành nhiều service riêng.

## 2. Vì sao dùng 3 database?

Hệ thống có 3 service chính, nên dùng 3 database riêng:

```text
SQL Server
  HRMS_HrCoreDb
  HRMS_AttendanceDb
  HRMS_PayrollReportDb
```

Ý nghĩa:

- `HRMS_HrCoreDb`: nguồn sự thật về nhân sự, tài khoản, role, phòng ban, chức vụ, hợp đồng.
- `HRMS_AttendanceDb`: dữ liệu ca làm, lịch làm, chấm công, nghỉ phép.
- `HRMS_PayrollReportDb`: dữ liệu kỳ lương, phiếu lương, phụ cấp, khấu trừ, báo cáo.

Các database **không tạo foreign key trực tiếp qua nhau**. Liên kết giữa service dùng:

- API Gateway cho request từ frontend.
- RabbitMQ event cho đồng bộ dữ liệu nền.
- Projection table để lưu bản sao tối thiểu từ service khác.

Ví dụ:

```text
HR Core tạo Employee
  -> publish EmployeeCreated
  -> Attendance lưu EmployeeProjections
  -> Payroll lưu EmployeeProjections
```

`EmployeeProjections.EmployeeId` là external id nhận từ HR Core, không phải FK vật lý sang `HRMS_HrCoreDb.dbo.Employees`.

## 3. Chuẩn 3NF

Các bảng nghiệp vụ chính được thiết kế theo 3NF:

- Không lưu danh sách trong một cột.
- Mỗi bảng có khóa chính rõ ràng.
- Dữ liệu danh mục được tách riêng: `Departments`, `Positions`, `Roles`, `LeaveTypes`, `AllowanceTypes`, `DeductionTypes`, `PayrollRules`.
- `Employees` chỉ lưu `DepartmentId`, `PositionId`, không lặp tên phòng ban/chức vụ trong bảng nhân viên.

Ngoại lệ có kiểm soát:

- `EmployeeProjections`, `AttendanceProjections`, `LeaveProjections` là bản sao phục vụ microservices.
- `Timesheets`, `Payslips`, `PayslipItems` là snapshot/tổng hợp để phục vụ audit, báo cáo và demo.

## 4. Yêu cầu môi trường

Tối thiểu:

- SQL Server local hoặc Docker Desktop.
- `sqlcmd` để chạy script SQL.
- Git.

Khi bắt đầu code backend/frontend:

- .NET SDK theo version nhóm thống nhất.
- Node.js LTS.
- Docker Desktop nếu muốn chạy đồng nhất trên mọi máy.

Kiểm tra SQL Server local:

```powershell
sqlcmd -S localhost -E -Q "SELECT @@SERVERNAME AS ServerName, DB_NAME() AS CurrentDatabase"
```

## 5. Tạo database bằng SQL Server local

Máy nào có SQL Server local và Windows Authentication thì chạy:

```powershell
sqlcmd -S localhost -E -i infra/sqlserver/init/00_create_hrms_databases.sql
```

Kiểm tra 3 database đã tạo:

```powershell
sqlcmd -S localhost -E -Q "SELECT name FROM sys.databases WHERE name LIKE 'HRMS_%' ORDER BY name"
```

Kiểm tra bảng HR Core:

```powershell
sqlcmd -S localhost -E -d HRMS_HrCoreDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME"
```

Kiểm tra bảng Attendance:

```powershell
sqlcmd -S localhost -E -d HRMS_AttendanceDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME"
```

Kiểm tra bảng Payroll & Report:

```powershell
sqlcmd -S localhost -E -d HRMS_PayrollReportDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME"
```

Kiểm tra seed role:

```powershell
sqlcmd -S localhost -E -d HRMS_HrCoreDb -Q "SELECT Name FROM dbo.Roles ORDER BY Name"
```

Script tạo DB là idempotent: chạy lại không drop database/table cũ.

## 6. Chạy bằng Docker

Docker dùng để khóa version môi trường, giúp máy các thành viên chạy giống nhau:

- SQL Server cùng image.
- RabbitMQ cùng image.
- Port và connection string thống nhất.
- Sau này có thể thêm container cho gateway, services và frontend.

Yêu cầu riêng cho SQL Server container:

- Docker Desktop cần cấp ít nhất 2GB RAM cho engine.
- Khuyến nghị cấp 4GB RAM để SQL Server chạy ổn định.
- Nếu Docker Desktop dùng WSL 2 backend, UI sẽ không có thanh chỉnh Memory. Khi đó chỉnh trong file `C:\Users\<ten-user>\.wslconfig`.
- File `.wslconfig` nên có cấu hình như sau:

```ini
[wsl2]
memory=4GB
processors=2
```

- Sau khi sửa `.wslconfig`, chạy `wsl --shutdown`, rồi mở lại Docker Desktop.
- Nếu log container báo `sqlservr: This program requires a machine with at least 2000 megabytes of memory`, Docker/WSL đang được cấp thiếu RAM hoặc chưa restart sau khi sửa `.wslconfig`.
- Nếu PowerShell không nhận lệnh `docker`, dùng tạm:

```powershell
$env:PATH = 'C:\Program Files\Docker\Docker\resources\bin;' + $env:PATH
```

Start hạ tầng:

```powershell
docker compose -f infra/docker-compose.yml up -d
```

Xem container:

```powershell
docker compose -f infra/docker-compose.yml ps
```

Tạo database trong SQL Server container:

```powershell
docker exec -i hrms-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Hrms@123456789" -C -i /docker-entrypoint-initdb.d/00_create_hrms_databases.sql
```

Nếu container không có đường dẫn `mssql-tools18`, thử:

```powershell
docker exec -i hrms-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Hrms@123456789" -i /docker-entrypoint-initdb.d/00_create_hrms_databases.sql
```

Mở RabbitMQ Management:

```text
http://localhost:15672
user: guest
password: guest
```

Dừng hạ tầng:

```powershell
docker compose -f infra/docker-compose.yml down
```

Dừng và xóa volume dữ liệu:

```powershell
docker compose -f infra/docker-compose.yml down -v
```

## 7. Connection string dự kiến

Local SQL Server:

```text
Server=localhost;Database=HRMS_HrCoreDb;Trusted_Connection=True;TrustServerCertificate=True
Server=localhost;Database=HRMS_AttendanceDb;Trusted_Connection=True;TrustServerCertificate=True
Server=localhost;Database=HRMS_PayrollReportDb;Trusted_Connection=True;TrustServerCertificate=True
```

Docker SQL Server:

```text
Server=localhost,1434;Database=HRMS_HrCoreDb;User Id=sa;Password=Hrms@123456789;TrustServerCertificate=True
Server=localhost,1434;Database=HRMS_AttendanceDb;User Id=sa;Password=Hrms@123456789;TrustServerCertificate=True
Server=localhost,1434;Database=HRMS_PayrollReportDb;User Id=sa;Password=Hrms@123456789;TrustServerCertificate=True
```

RabbitMQ:

```text
Host=localhost
Port=5672
User=guest
Password=guest
```

## 8. Cách chạy cuối cùng khi đã có source code

Thứ tự chạy dự kiến:

1. Start SQL Server và RabbitMQ.
2. Tạo DB bằng script SQL hoặc EF Core migrations.
3. Chạy API Gateway.
4. Chạy HR Core Service.
5. Chạy Attendance Service.
6. Chạy Payroll & Report Service.
7. Chạy Vue frontend.

Command dự kiến:

```powershell
docker compose -f infra/docker-compose.yml up -d
sqlcmd -S localhost -E -i infra/sqlserver/init/00_create_hrms_databases.sql

dotnet run --project backend/gateway
dotnet run --project backend/services/hr-core
dotnet run --project backend/services/attendance
dotnet run --project backend/services/payroll-report

cd frontend
npm ci
npm run dev
```

Port dự kiến:

```text
Frontend:              http://localhost:5173
API Gateway:           http://localhost:5005
HR Core Service:        http://localhost:5001
Attendance Service:    http://localhost:5002
Payroll Report Service:http://localhost:5003
SQL Server Docker:     localhost,1434
SQL Server local:      localhost
RabbitMQ:              localhost:5672
RabbitMQ Management:   http://localhost:15672
```

Frontend chỉ gọi API Gateway. Không gọi thẳng từng service từ Vue.

## 9. Tài liệu liên quan

- `docs/00_PHAN_TICH_TONG_THE_HRMS.md`: phân tích tổng thể.
- `docs/01_NHOM_7_HR_CORE_KIEN_TRUC.md`: task nhóm 7.
- `docs/02_NHOM_8_ATTENDANCE_SERVICE.md`: task nhóm 8.
- `docs/03_NHOM_9_PAYROLL_REPORT_SERVICE.md`: task nhóm 9.
