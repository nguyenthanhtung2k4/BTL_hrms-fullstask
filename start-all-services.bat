@echo off
chcp 65001 >nul
echo ================================================
echo    HRMS Microservices - Start All Services
echo ================================================
echo.

REM Lấy đường dẫn của thư mục chứa file .bat này
set "BASE_DIR=%~dp0"

REM ================================================
REM  Step 1: Start Docker containers (SQL Server + RabbitMQ)
REM ================================================
echo [1/6] Starting Docker containers (SQL Server + RabbitMQ)...
start "HRMS Docker" cmd /k "cd /d "%BASE_DIR%infra" && docker compose up -d && echo. && echo [Docker] Containers started successfully! && echo. && docker compose ps"

echo      Waiting for Docker containers to be ready...
timeout /t 15 /nobreak >nul

REM ================================================
REM  Step 2-5: Start Backend Microservices
REM ================================================
echo [2/6] Starting API Gateway (port 5000)...
start "HRMS Gateway" cmd /k "cd /d "%BASE_DIR%backend\gateway" && dotnet run"

timeout /t 3 /nobreak >nul

echo [3/6] Starting HR Core Service (port 5001)...
start "HRMS HR-Core" cmd /k "cd /d "%BASE_DIR%backend\services\hr-core" && dotnet run"

timeout /t 2 /nobreak >nul

echo [4/6] Starting Attendance Service (port 5002)...
start "HRMS Attendance" cmd /k "cd /d "%BASE_DIR%backend\services\attendance" && dotnet run"

timeout /t 2 /nobreak >nul

echo [5/6] Starting Payroll Service (port 5003)...
start "HRMS Payroll" cmd /k "cd /d "%BASE_DIR%backend\services\payroll-report" && dotnet run"

timeout /t 2 /nobreak >nul

REM ================================================
REM  Step 6: Start Frontend (Vite + Vue)
REM ================================================
echo [6/6] Starting Frontend (port 5173)...
start "HRMS Frontend" cmd /k "cd /d "%BASE_DIR%frontend" && npm run dev"

echo.
echo ================================================
echo  All services are starting in separate windows!
echo ================================================
echo.
echo  Infrastructure:
echo    SQL Server:   localhost:1434 (sa / Hrms@123456789)
echo    RabbitMQ:     http://localhost:15672 (guest / guest)
echo.
echo  Backend:
echo    Gateway:      http://localhost:5000
echo    HR Core:      http://localhost:5001
echo    Attendance:   http://localhost:5002
echo    Payroll:      http://localhost:5003
echo.
echo  Frontend:       http://localhost:5173
echo  Login:          admin@hrms.com / admin123
echo ================================================
echo.
pause