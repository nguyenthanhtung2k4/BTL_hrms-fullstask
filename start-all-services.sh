#!/bin/bash

# Get directory where the script is located
BASE_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$BASE_DIR"

echo "=================================================="
echo "   HRMS Microservices - Start All Services (macOS)"
echo "=================================================="
echo

# Step 1: Start Docker containers
echo "[1/6] Starting Docker containers (SQL Server + RabbitMQ)..."
docker compose -f infra/docker-compose.yml up -d
if [ $? -ne 0 ]; then
    echo
    echo "[ERROR] Failed to start Docker containers. Make sure Docker Desktop is open and running!"
    exit 1
fi
echo "[Docker] Containers started successfully!"
echo "Waiting 15 seconds for databases to be ready..."
sleep 15
echo

# Function to run service in a new Terminal window on macOS
run_in_new_terminal() {
    local title=$1
    local dir=$2
    local cmd=$3
    echo "Starting $title..."
    # Using AppleScript to tell Terminal to open a new tab/window, cd to the dir, set the title, and run the command
    osascript -e "tell application \"Terminal\" to do script \"cd '$dir' && echo -n -e '\\\033]0;$title\\\007' && $cmd\""
}

run_in_new_terminal "HRMS Gateway" "$BASE_DIR/backend/gateway" "dotnet run"
sleep 3

run_in_new_terminal "HRMS HR-Core" "$BASE_DIR/backend/services/hr-core" "dotnet run"
sleep 2

run_in_new_terminal "HRMS Attendance" "$BASE_DIR/backend/services/attendance" "dotnet run"
sleep 2

run_in_new_terminal "HRMS Payroll" "$BASE_DIR/backend/services/payroll-report" "dotnet run"
sleep 2

echo
echo "=================================================="
echo "  All backend services are starting in Terminal!"
echo "=================================================="
echo "  Infrastructure:"
echo "    SQL Server:   localhost:1434 (sa / Hrms@123456789)"
echo "    RabbitMQ:     http://localhost:15672 (guest / guest)"
echo
echo "  Backend:"
echo "    Gateway:      http://localhost:5005"
echo "    HR Core:      http://localhost:5001"
echo "    Attendance:   http://localhost:5002"
echo "    Payroll:      http://localhost:5003"
echo
echo "  Note: Frontend is already running in your main terminal."
echo "  Once the backend services compile and start, you can view the app at:"
echo "    http://localhost:5173"
echo "  Login: admin@hrms.com / admin123"
echo "=================================================="
echo
