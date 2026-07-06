#!/bin/bash
set -e

echo "============================================================"
echo "  HRMS Microservices - Seed All Data (SQL + API)"
echo "============================================================"
echo

# Determine the SQLCMD path inside the container
echo "Checking sqlcmd path in container..."
SQLCMD_PATH=""
if docker exec hrms-sqlserver /opt/mssql-tools18/bin/sqlcmd -? > /dev/null 2>&1; then
    SQLCMD_PATH="/opt/mssql-tools18/bin/sqlcmd"
elif docker exec hrms-sqlserver /opt/mssql-tools/bin/sqlcmd -? > /dev/null 2>&1; then
    SQLCMD_PATH="/opt/mssql-tools/bin/sqlcmd"
else
    echo "ERROR: sqlcmd not found in hrms-sqlserver container."
    exit 1
fi
echo "Using sqlcmd at: $SQLCMD_PATH"

echo "[1/5] Creating databases and tables if not exist..."
docker exec -i hrms-sqlserver $SQLCMD_PATH -S localhost -U sa -P "Hrms@123456789" -C < infra/sqlserver/init/00_create_hrms_databases.sql

echo "[2/5] Cleaning existing database content to guarantee fresh data..."
docker exec -i hrms-sqlserver $SQLCMD_PATH -S localhost -U sa -P "Hrms@123456789" -C < clean-databases.sql

echo "[3/5] Synchronizing projection databases..."
docker exec -i hrms-sqlserver $SQLCMD_PATH -S localhost -U sa -P "Hrms@123456789" -C < sync-projections.sql

echo "[4/5] Seeding Attendance database..."
docker exec -i hrms-sqlserver $SQLCMD_PATH -S localhost -U sa -P "Hrms@123456789" -C < seed-attendance-data.sql

echo "[5/5] Seeding Payroll Report database..."
docker exec -i hrms-sqlserver $SQLCMD_PATH -S localhost -U sa -P "Hrms@123456789" -C < seed-payroll-data.sql

echo "SQL seeding completed."
echo
echo "Seeding Master/Demo Data via REST API Gateway (port 5000)..."
echo "Note: This requires the Backend Microservices to be running."
python3 scripts/seed-demo-data.py

echo "============================================================"
echo "  SEED ALL DATABASES COMPLETED SUCCESSFULLY!"
echo "============================================================"
