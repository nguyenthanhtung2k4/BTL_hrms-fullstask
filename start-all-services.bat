@echo off
echo ================================================
echo    HRMS Microservices - Start All Services
echo ================================================
echo.

echo [1/4] Starting API Gateway (port 5000)...
start "HRMS Gateway" cmd /k "cd /d d:\CODE\DNU_Full_Stask\BTL_FULL_STASK\backend\gateway && dotnet run"

timeout /t 3 /nobreak >nul

echo [2/4] Starting HR Core Service (port 5001)...
start "HRMS HR-Core" cmd /k "cd /d d:\CODE\DNU_Full_Stask\BTL_FULL_STASK\backend\services\hr-core && dotnet run"

timeout /t 2 /nobreak >nul

echo [3/4] Starting Attendance Service (port 5002)...
start "HRMS Attendance" cmd /k "cd /d d:\CODE\DNU_Full_Stask\BTL_FULL_STASK\backend\services\attendance && dotnet run"

timeout /t 2 /nobreak >nul

echo [4/4] Starting Payroll Service (port 5003)...
start "HRMS Payroll" cmd /k "cd /d d:\CODE\DNU_Full_Stask\BTL_FULL_STASK\backend\services\payroll-report && dotnet run"

echo.
echo ================================================
echo  All services starting in separate windows!
echo ================================================
echo.
echo  Ports:
echo    Gateway:    http://localhost:5000
echo    HR Core:    http://localhost:5001
echo    Attendance: http://localhost:5002
echo    Payroll:    http://localhost:5003
echo.
echo  Frontend:     http://localhost:5173
echo  Login:        admin@hrms.com / admin123
echo ================================================
echo.
pause
