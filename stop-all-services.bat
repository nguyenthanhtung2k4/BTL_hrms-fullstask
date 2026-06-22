@echo off
chcp 65001 >nul
echo ================================================
echo    HRMS Microservices - Stop All Services
echo ================================================
echo.

echo Stopping services on port 5000 (Gateway)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5000 ^| findstr LISTENING') do taskkill /f /pid %%a 2>nul

echo Stopping services on port 5001 (HR Core)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5001 ^| findstr LISTENING') do taskkill /f /pid %%a 2>nul

echo Stopping services on port 5002 (Attendance)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5002 ^| findstr LISTENING') do taskkill /f /pid %%a 2>nul

echo Stopping services on port 5003 (Payroll)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5003 ^| findstr LISTENING') do taskkill /f /pid %%a 2>nul

echo Stopping services on port 5173 (Frontend)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5173 ^| findstr LISTENING') do taskkill /f /pid %%a 2>nul

echo.
echo ================================================
echo All services have been stopped.
echo You can now run start-all-services.bat again.
echo ================================================
pause
