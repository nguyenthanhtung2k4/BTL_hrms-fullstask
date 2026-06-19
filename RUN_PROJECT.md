# Chạy dự án HRMS

File này ghi các lệnh chạy nhanh backend, frontend, SQL Server và RabbitMQ.

## 0. Yêu cầu cài đặt (Prerequisites)

### 0.1 Cài Docker Desktop (bắt buộc)

Dự án sử dụng Docker để chạy **SQL Server 2022** và **RabbitMQ**. Cần cài Docker Desktop trước.

**Bước 1: Bật WSL2 (Windows Subsystem for Linux)**

Mở PowerShell **với quyền Admin** và chạy:

```powershell
wsl --install
```

Nếu đã cài WSL rồi, đảm bảo đang dùng WSL2:

```powershell
wsl --set-default-version 2
```

Khởi động lại máy sau khi cài xong.

**Bước 2: Tải và cài Docker Desktop**

- Tải từ: https://www.docker.com/products/docker-desktop/
- Chọn phiên bản **Windows (AMD64)** hoặc **ARM64** tùy máy
- Chạy file `.exe` vừa tải, chọn **Use WSL 2 instead of Hyper-V** khi được hỏi
- Cài xong → khởi động lại máy

**Bước 3: Kiểm tra Docker đã cài thành công**

```powershell
docker --version
# Kết quả mong đợi: Docker version 27.x.x, build ...

docker compose version
# Kết quả mong đợi: Docker Compose version v2.x.x
```

**Bước 4: Chạy Docker containers cho dự án**

```powershell
cd D:\CODE\DNU_Full_Stask\BTL_FULL_STASK
docker compose -f infra/docker-compose.yml up -d
```

Kiểm tra containers đang chạy:

```powershell
docker ps
```

Kết quả mong đợi:

```text
CONTAINER ID   IMAGE                                        PORTS                              NAMES
xxxxxxxxxxxx   mcr.microsoft.com/mssql/server:2022-latest   0.0.0.0:1434->1433/tcp             hrms-sqlserver
xxxxxxxxxxxx   rabbitmq:3-management                        0.0.0.0:5672->5672/tcp, 15672/tcp  hrms-rabbitmq
```

**Bước 5: Kiểm tra kết nối SQL Server**

```powershell
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -Q "SELECT @@VERSION"
```

Hoặc dùng SQL Server Management Studio (SSMS) kết nối:

```text
Server:   localhost,1434
Login:    sa
Password: Hrms@123456789
```

### 0.2 Docker — Xử lý lỗi thường gặp

| Lỗi | Nguyên nhân | Cách sửa |
|---|---|---|
| `docker: command not found` | Chưa cài Docker hoặc chưa thêm PATH | Cài lại Docker Desktop, tick "Add to PATH" |
| `Cannot connect to Docker daemon` | Docker Desktop chưa khởi động | Mở Docker Desktop, đợi icon xanh ở taskbar |
| `WSL 2 installation is incomplete` | Chưa cài WSL2 kernel | Chạy `wsl --install` rồi restart |
| `port 1434 already in use` | SQL Server local đang chạy | Tắt SQL Server local: `net stop MSSQLSERVER` |
| `port 5672 already in use` | RabbitMQ/Erlang local đang chạy | Tắt service RabbitMQ local |
| Container tự tắt sau vài giây | Thiếu RAM (SQL Server cần ≥ 2GB) | Tăng RAM cho Docker: Settings → Resources → Memory ≥ 4GB |

### 0.3 Các phần mềm khác cần cài

| Phần mềm | Phiên bản | Link tải |
|---|---|---|
| **.NET SDK** | 8.0+ | https://dotnet.microsoft.com/download/dotnet/8.0 |
| **Node.js** | 18+ (LTS) | https://nodejs.org/ |
| **Docker Desktop** | Latest | https://www.docker.com/products/docker-desktop/ |
| **Git** | Latest | https://git-scm.com/downloads |

Kiểm tra nhanh:

```powershell
dotnet --version    # >= 8.0.x
node --version      # >= 18.x
npm --version       # >= 9.x
docker --version    # >= 24.x
git --version       # >= 2.x
```

### 0.4 Thông tin kết nối

| Service | Host | Port | Credentials |
|---|---|---|---|
| SQL Server | localhost | **1434** | `sa` / `Hrms@123456789` |
| RabbitMQ | localhost | **5672** (AMQP) / **15672** (Web UI) | `guest` / `guest` |
| API Gateway | localhost | **5000** | — |
| HR Core | localhost | **5001** | — |
| Attendance | localhost | **5002** | — |
| Payroll | localhost | **5003** | — |
| Frontend | localhost | **5173** | `admin@hrms.com` / `admin123` |

---
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

