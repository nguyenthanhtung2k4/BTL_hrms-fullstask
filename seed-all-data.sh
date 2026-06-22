#!/bin/bash
chcp 65001 2>/dev/null || true

echo "============================================================"
echo "  HRMS Microservices - Seed All Data (macOS)"
echo "============================================================"
echo

echo "[1/6] Creating databases and tables if not exist..."
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i infra/sqlserver/init/00_create_hrms_databases.sql
if [ $? -ne 0 ]; then
    echo
    echo "[ERROR] Failed to run database creation script. Make sure Docker is running."
    exit 1
fi
echo

echo "[2/6] Cleaning existing database content to guarantee fresh data..."
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i clean-databases.sql
if [ $? -ne 0 ]; then
    echo
    echo "[ERROR] Failed to clean existing databases."
    exit 1
fi
echo

echo "[3/6] Seeding Master/Demo Data via REST API Gateway (port 5000)..."
echo "Note: This requires the Backend Microservices (Gateway, HR-Core, etc.) to be running."
echo "If they are not running, this step will show connection errors."
echo
node seed-demo-data.js
echo

echo "[4/6] Synchronizing projection databases..."
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i sync-projections.sql
if [ $? -ne 0 ]; then
    echo
    echo "[ERROR] Failed to sync projection databases."
    exit 1
fi
echo

echo "[5/6] Seeding Attendance database..."
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i seed-attendance-data.sql
if [ $? -ne 0 ]; then
    echo
    echo "[ERROR] Failed to seed Attendance database."
    exit 1
fi
echo

echo "[6/6] Seeding Payroll Report database..."
sqlcmd -S localhost,1434 -U sa -P "Hrms@123456789" -C -f 65001 -i seed-payroll-data.sql
if [ $? -ne 0 ]; then
    echo
    echo "[ERROR] Failed to seed Payroll database."
    exit 1
fi
echo

echo "============================================================"
echo "  SEED ALL DATABASES COMPLETED SUCCESSFULLY!"
echo "============================================================"
echo
