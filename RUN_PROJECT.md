# Chạy dự án HRMS

File này ghi các lệnh chạy nhanh backend, frontend, SQL Server và RabbitMQ.

## 1. Chạy tất cả bằng một lệnh

Mở PowerShell tại root repo:

```powershell
cd D:\CODE\DNU_Full_Stask\BTL_FULL_STASK
```

Chạy toàn bộ:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/run-all.ps1
```

Script sẽ start:

- Docker Compose: SQL Server + RabbitMQ.
- HR Core Service: `http://localhost:5001`.
- Attendance Service: `http://localhost:5002`.
- Payroll & Report Service: `http://localhost:5003`.
- API Gateway: `http://localhost:5000`.
- Frontend: `http://localhost:5173`.

Sau khi chạy xong, mở:

```text
http://localhost:5173
```

Test gateway:

```text
http://localhost:5000/api/v1/hr/info
http://localhost:5000/api/attendance/info
http://localhost:5000/api/payroll/info
```

## 2. Dừng toàn bộ service app

```powershell
powershell -ExecutionPolicy Bypass -File scripts/stop-all.ps1
```

Lệnh này dừng các process đang giữ port:

```text
5000, 5001, 5002, 5003, 5173
```

Nếu muốn dừng cả Docker:

```powershell
docker compose -f infra/docker-compose.yml down
```

## 3. Chạy thủ công từng phần

Start Docker SQL Server + RabbitMQ:

```powershell
$env:PATH = 'C:\Program Files\Docker\Docker\resources\bin;' + $env:PATH
docker compose -f infra/docker-compose.yml up -d
```

Terminal 1:

```powershell
dotnet run --project backend/services/hr-core/Hrms.HrCore.Api.csproj --launch-profile http
```

Terminal 2:

```powershell
dotnet run --project backend/services/attendance/Hrms.Attendance.Api.csproj --launch-profile http
```

Terminal 3:

```powershell
dotnet run --project backend/services/payroll-report/Hrms.PayrollReport.Api.csproj --launch-profile http
```

Terminal 4:

```powershell
dotnet run --project backend/gateway/Hrms.Gateway.csproj --launch-profile http
```

Terminal 5:

```powershell
cd frontend
npm ci
npm run dev
```

## 4. Kiểm tra build

Backend:

```powershell
dotnet build backend/HRMS.sln
```

Frontend:

```powershell
cd frontend
npm run build
```

## 5. Xem database Docker

```powershell
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -Q "SELECT name FROM sys.databases WHERE name LIKE 'HRMS_%' ORDER BY name"
```

RabbitMQ UI:

```text
http://localhost:15672
user: guest
password: guest
```

