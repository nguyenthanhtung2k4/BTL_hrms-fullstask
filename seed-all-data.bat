@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo ============================================================
echo   HRMS Microservices - Seed All Data (SQL + API)
echo ============================================================
echo.

echo [1/6] Creating databases and tables if not exist...
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i infra/sqlserver/init/00_create_hrms_databases.sql
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Failed to run database creation script. Make sure Docker is running.
    pause
    exit /b %errorlevel%
)
echo.

echo [2/6] Cleaning existing database content to guarantee fresh data...
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i clean-databases.sql
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Failed to clean existing databases.
    pause
    exit /b %errorlevel%
)
echo.

echo [3/6] Seeding Master/Demo Data via REST API Gateway (port 5005)...
echo Note: This requires the Backend Microservices (Gateway, HR-Core, etc.) to be running.
echo If they are not running, this step will show connection errors.
echo.
powershell -ExecutionPolicy Bypass -File seed-demo-data.ps1
echo.

echo [4/6] Synchronizing projection databases...
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i sync-projections.sql
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Failed to sync projection databases.
    pause
    exit /b %errorlevel%
)
echo.

echo [5/6] Seeding Attendance database...
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i seed-attendance-data.sql
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Failed to seed Attendance database.
    pause
    exit /b %errorlevel%
)
echo.

echo [6/6] Seeding Payroll Report database...
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i seed-payroll-data.sql
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Failed to seed Payroll database.
    pause
    exit /b %errorlevel%
)
echo.

echo ============================================================
echo   SEED ALL DATABASES COMPLETED SUCCESSFULLY!
echo ============================================================
echo.
pause
